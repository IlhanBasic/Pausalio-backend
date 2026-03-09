using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pausalio.API.Hubs;
using Pausalio.API.Middlewares;
using Pausalio.Application.Mappings;
using Pausalio.Application.Services.Implementations;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Application.Validators;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Implementations;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Localization;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.EnableCaching = true;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
// -------------------- Serilog --------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
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
builder.Services.AddScoped<IBusinessInviteRepository, BusinessInviteRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();


// -------------------- Configuration --------------------
builder.Services.Configure<OpenRouterSettings>(builder.Configuration.GetSection("OpenRouterSettings"));
builder.Services.Configure<ExchangeRateSettings>(builder.Configuration.GetSection("ExchangeRateSettings"));
builder.Services.Configure<AzureBlobStorageSettings>(builder.Configuration.GetSection("AzureBlobStorageSettings"));
builder.Services.Configure<UrlSettings>(builder.Configuration.GetSection("UrlSettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings nije konfigurisan.");
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

// -------------------- JWT Authentication --------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// -------------------- AutoMapper --------------------
builder.Services.AddAutoMapper(typeof(ClientMappingProfile).Assembly);

// -------------------- Controllers --------------------
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

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
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IBusinessInviteService, BusinessInviteService>();
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IInvoiceExportService, InvoiceExportService>();
builder.Services.AddSingleton<IPdfFactoryService, PdfFactoryService>();
builder.Services.AddHttpClient<IAIAssistantService, AIAssistantService>();
builder.Services.AddScoped<IFinancialContextService, FinancialContextService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IEncryptionService, XorEncryptionService>();
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
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// -------------------- Swagger --------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Unesite JWT token ovde sa prefixom 'Bearer '",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                 "http://localhost:4200",
                 "https://app-pausalio.netlify.app"
               )
               .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// -------------------- SignalR --------------------
builder.Services.AddSignalR();

// -------------------- Build Application --------------------
var app = builder.Build();

// -------------------- Middleware --------------------
app.UseSerilogRequestLogging();
app.UseRequestLocalization();
app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<BusinessContextMiddleware>();
// -------------------- Swagger UI --------------------
app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pausalio API V1");
//    c.RoutePrefix = string.Empty;
//});

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

// -------------------- Run --------------------
app.Run();
