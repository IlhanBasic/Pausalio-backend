using Pausalio.Shared.Enums;

namespace Pausalio.Application.Services.Interfaces
{
    public interface IExchangeRateService
    {
        Task<decimal?> GetExchangeRateAsync(Currency currency);
        Task<decimal?> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency);
        Task<Dictionary<Currency, decimal>> GetAllRatesAsync();
    }
}