using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pausalio.Application.Mappings;
using Pausalio.Application.Services.Implementations;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Implementations;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Localization;
using Pausalio.Evaluation.Models;

namespace Pausalio.Evaluation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run --project Pausalio.Evaluation [seed|run|judge|report|all]");
                return;
            }

            var command = args[0].ToLower();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    // Bind EvaluationSettings
                    services.Configure<EvaluationSettings>(configuration.GetSection("EvaluationSettings"));

                    // Bind OpenRouterSettings by mapping from EvaluationSettings
                    var evalSettings = configuration.GetSection("EvaluationSettings").Get<EvaluationSettings>()
                        ?? new EvaluationSettings();

                    services.Configure<OpenRouterSettings>(options =>
                    {
                        options.ApiKey = evalSettings.OpenRouterApiKey;
                        options.ApiUrl = evalSettings.OpenRouterApiUrl;
                        options.Model = evalSettings.GenerationModel;
                    });

                    // Bind ExchangeRateSettings to avoid potential DI missing-option errors
                    services.Configure<ExchangeRateSettings>(options =>
                    {
                        options.BaseUrl = "https://api.kursna-lista.info"; // default
                    });

                    // Database
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<PausalioDbContext>(options =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    );

                    // Repositories
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped<IActivityCodeRepository, ActivityCodeRepository>();
                    services.AddScoped<IBankAccountRepository, BankAccountRepository>();
                    services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
                    services.AddScoped<IUserBusinessProfileRepository, UserBusinessProfileRepository>();
                    services.AddScoped<ICityRepository, CityRepository>();
                    services.AddScoped<IClientRepository, ClientRepository>();
                    services.AddScoped<ICountryRepository, CountryRepository>();
                    services.AddScoped<IDocumentRepository, DocumentRepository>();
                    services.AddScoped<IExpenseRepository, ExpenseRepository>();
                    services.AddScoped<IInvoiceRepository, InvoiceRepository>();
                    services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
                    services.AddScoped<IItemRepository, ItemRepository>();
                    services.AddScoped<IPaymentRepository, PaymentRepository>();
                    services.AddScoped<IReminderRepository, ReminderRepository>();
                    services.AddScoped<ITaxObligationRepository, TaxObligationRepository>();
                    services.AddScoped<IUserProfileRepository, UserProfileRepository>();
                    services.AddScoped<IBusinessInviteRepository, BusinessInviteRepository>();
                    services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
                    services.AddScoped<IAiToolCallRepository, AiToolCallRepository>();
                    services.AddScoped<IAiConversationRepository, AIConversationRepository>();
                    services.AddScoped<IAiMessageRepository, AiMessageRepository>();
                    // Shared and Infrastructure
                    services.AddScoped<IEncryptionService, NoOpEncryptionService>();
                    services.AddScoped<ICurrentUserService, EvalCurrentUserService>();
                    services.AddMemoryCache();
                    services.AddLocalization();
                    services.AddScoped<ILocalizationHelper, LocalizationHelper>();
                    services.AddHttpClient();

                    // AutoMapper
                    services.AddAutoMapper(typeof(ClientMappingProfile).Assembly);

                    // Application Services
                    services.AddScoped<IActivityCodeService, ActivityCodeService>();
                    services.AddScoped<IBankAccountService, BankAccountService>();
                    services.AddScoped<IBusinessProfileService, BusinessProfileService>();
                    services.AddScoped<ICityService, CityService>();
                    services.AddScoped<IClientService, ClientService>();
                    services.AddScoped<ICountryService, CountryService>();
                    services.AddScoped<IDocumentService, DocumentService>();
                    services.AddScoped<IExpenseService, ExpenseService>();
                    services.AddScoped<IInvoiceService, InvoiceService>();
                    services.AddScoped<IInvoiceItemService, InvoiceItemService>();
                    services.AddScoped<IItemService, ItemService>();
                    services.AddScoped<IPaymentService, PaymentService>();
                    services.AddScoped<IReminderService, ReminderService>();
                    services.AddScoped<ITaxObligationService, TaxObligationService>();
                    services.AddScoped<IUserBusinessProfileService, UserBusinessProfileService>();
                    services.AddScoped<IUserProfileService, UserProfileService>();
                    services.AddScoped<IAIAssistantService, AIAssistantService>();
                    services.AddScoped<IFinancialContextService, FinancialContextService>();
                    services.AddScoped<IExchangeRateService, ExchangeRateService>();

                    // Harness Classes
                    services.AddScoped<HarnessRunner>();
                    services.AddScoped<JudgeClient>();
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var dbContext = serviceProvider.GetRequiredService<PausalioDbContext>();
            var settings = serviceProvider.GetRequiredService<IOptions<EvaluationSettings>>().Value;

            try
            {
                switch (command)
                {
                    case "seed":
                        logger.LogInformation("Seeding evaluation database...");
                        await EvaluationSeeder.SeedAsync(dbContext, settings.OpenRouterApiKey, settings.GenerationModel);
                        logger.LogInformation("Seeding completed successfully.");
                        break;

                    case "run":
                        logger.LogInformation("Starting evaluation run (L1 & L2)...");
                        var runner = serviceProvider.GetRequiredService<HarnessRunner>();
                        var questions = DatasetLoader.Load("eval-dataset.json");
                        logger.LogInformation("Loaded {Count} questions.", questions.Count);
                        var results = await runner.RunAsync(questions);
                        logger.LogInformation("Run completed. Exporting initial reports...");
                        ReportExporter.Export(results, settings);
                        break;

                    case "judge":
                        logger.LogInformation("Starting LLM-as-a-judge scoring...");
                        var judge = serviceProvider.GetRequiredService<JudgeClient>();
                        if (!File.Exists("results.jsonl"))
                        {
                            logger.LogError("results.jsonl not found. Run 'run' subcommand first.");
                            return;
                        }
                        var resultsToJudge = new List<EvalResult>();
                        foreach (var line in File.ReadLines("results.jsonl"))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var result = JsonSerializer.Deserialize<EvalResult>(line);
                            if (result != null) resultsToJudge.Add(result);
                        }
                        logger.LogInformation("Loaded {Count} results from results.jsonl.", resultsToJudge.Count);
                        await judge.EvaluateResultsAsync(resultsToJudge);
                        logger.LogInformation("Judge evaluations completed. Saving judged results to results.jsonl...");
                        File.WriteAllText("results.jsonl", string.Empty);
                        foreach (var res in resultsToJudge)
                        {
                            var options = new JsonSerializerOptions { WriteIndented = false };
                            var json = JsonSerializer.Serialize(res, options);
                            File.AppendAllText("results.jsonl", json + Environment.NewLine);
                        }
                        logger.LogInformation("Judged results saved. Exporting reports...");
                        ReportExporter.Export(resultsToJudge, settings);
                        break;

                    case "report":
                        logger.LogInformation("Loading results to generate report...");
                        if (!File.Exists("results.jsonl"))
                        {
                            logger.LogError("results.jsonl not found. Run 'run'/'judge' subcommand first.");
                            return;
                        }
                        var resultsForReport = new List<EvalResult>();
                        foreach (var line in File.ReadLines("results.jsonl"))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var result = JsonSerializer.Deserialize<EvalResult>(line);
                            if (result != null) resultsForReport.Add(result);
                        }
                        logger.LogInformation("Loaded {Count} results. Exporting reports...", resultsForReport.Count);
                        ReportExporter.Export(resultsForReport, settings);
                        logger.LogInformation("Report generation complete.");
                        break;

                    case "all":
                        logger.LogInformation("Running full evaluation pipeline: seed -> run -> judge -> report");
                        
                        logger.LogInformation("Step 1/4: Seeding evaluation database...");
                        await EvaluationSeeder.SeedAsync(dbContext, settings.OpenRouterApiKey, settings.GenerationModel);
                        
                        logger.LogInformation("Step 2/4: Running evaluations (L1 & L2)...");
                        var pipelineRunner = serviceProvider.GetRequiredService<HarnessRunner>();
                        var pipelineQuestions = DatasetLoader.Load("eval-dataset.json");
                        var pipelineResults = await pipelineRunner.RunAsync(pipelineQuestions);
                        ReportExporter.Export(pipelineResults, settings);

                        logger.LogInformation("Step 3/4: Running LLM judging (L3)...");
                        var pipelineJudge = serviceProvider.GetRequiredService<JudgeClient>();
                        await pipelineJudge.EvaluateResultsAsync(pipelineResults);
                        
                        logger.LogInformation("Step 4/4: Re-saving judged results and exporting final reports...");
                        File.WriteAllText("results.jsonl", string.Empty);
                        foreach (var res in pipelineResults)
                        {
                            var options = new JsonSerializerOptions { WriteIndented = false };
                            var json = JsonSerializer.Serialize(res, options);
                            File.AppendAllText("results.jsonl", json + Environment.NewLine);
                        }
                        ReportExporter.Export(pipelineResults, settings);
                        logger.LogInformation("Pipeline completed successfully.");
                        break;

                    default:
                        Console.WriteLine($"Unknown subcommand: {command}");
                        Console.WriteLine("Available subcommands: seed, run, judge, report, all");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Execution failed with error: {Message}", ex.Message);
            }
        }
    }
}
