using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Pausalio.Application.Mappings;
using Pausalio.Application.Services.Implementations;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Application.Validators;
using Pausalio.Infrastructure.Persistence;
using Pausalio.Infrastructure.Repositories.Implementations;
using Pausalio.Infrastructure.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PausalioDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(ClientMappingProfile).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(AddClientDtoValidator).Assembly);
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
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
