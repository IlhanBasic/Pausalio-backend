using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Pausalio.Application.Mappings;
using Pausalio.Application.Services.Implementations;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Application.Validators;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Implementations;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Localization;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Serilog --------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// -------------------- Database --------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PausalioDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// -------------------- Unit of Work & Repositories --------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IActivityCodeRepository, ActivityCodeRepository>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
builder.Services.AddScoped<IUserBusinessProfileRepository, UserBusinessProfileRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<ITaxObligationRepository, TaxObligationRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

// -------------------- AutoMapper --------------------
builder.Services.AddAutoMapper(typeof(ClientMappingProfile).Assembly);

// -------------------- Controllers --------------------
builder.Services.AddControllers(); // DODATO - ovo ti je nedostajalo!

// -------------------- Authorization --------------------
builder.Services.AddAuthorization();

// -------------------- Application Services --------------------
builder.Services.AddScoped<IActivityCodeService, ActivityCodeService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IBusinessProfileService, BusinessProfileService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoiceItemService, InvoiceItemService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<ITaxObligationService, TaxObligationService>();
builder.Services.AddScoped<IUserBusinessProfileService, UserBusinessProfileService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
// -------------------- FluentValidation --------------------
builder.Services.AddValidatorsFromAssemblyContaining<AddBankAccountDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
// -------------------- Localization --------------------
builder.Services.AddLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("sr-Latn"),
    };

    options.DefaultRequestCulture = new RequestCulture("sr-Latn");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddScoped<ILocalizationHelper, LocalizationHelper>();

// -------------------- Swagger --------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Pausalio API",
        Version = "v1"
    });
});

// -------------------- Build Application --------------------
var app = builder.Build();

// -------------------- Middleware --------------------
app.UseSerilogRequestLogging();
app.UseRequestLocalization();
app.UseHttpsRedirection();

// -------------------- Swagger UI --------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pausalio API V1");
    c.RoutePrefix = string.Empty;
});

// -------------------- Authorization & Controllers --------------------
app.UseAuthorization();
app.MapControllers();

// -------------------- Run --------------------
app.Run();