using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pausalio.Application.Services.Implementations
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly IOptions<ExchangeRateSettings> _exchangeRateSettings;
        private readonly IMemoryCache _cache;

        public ExchangeRateService(
            HttpClient httpClient,
            ILogger<ExchangeRateService> logger,
            IOptions<ExchangeRateSettings> exchangeRateSettings,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _exchangeRateSettings = exchangeRateSettings;
            _cache = cache;
        }

        public async Task<decimal?> GetExchangeRateAsync(Currency currency)
        {
            if (currency == Currency.RSD)
                return 1m;

            var cacheKey = $"exchange_rate_{currency}_{DateTime.UtcNow:yyyy-MM-dd}";

            if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                _logger.LogInformation("Using cached exchange rate for {Currency}: {Rate}", currency, cachedRate);
                return cachedRate;
            }

            var currencyCode = currency.ToString().ToLower();
            var requestUri = $"{_exchangeRateSettings.Value.BaseUrl}/{currencyCode}/rates/today";

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Exchange Rate API returned {StatusCode} for {Currency}",
                        response.StatusCode, currency);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ExchangeRateApiResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null)
                {
                    _logger.LogWarning("Deserialization failed for {Currency}", currency);
                    return null;
                }

                var expiration = GetUtcMidnightExpiration();

                _cache.Set(cacheKey, result.ExchangeMiddle, expiration);

                _logger.LogInformation("Fetched and cached exchange rate for {Currency}: {Rate}",
                    currency, result.ExchangeMiddle);

                return result.ExchangeMiddle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate for {Currency}", currency);
                return null;
            }
        }

        public async Task<decimal?> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Invalid amount {Amount}", amount);
                return null;
            }

            if (fromCurrency == toCurrency)
                return amount;

            try
            {
                decimal amountInRsd = amount;

                if (fromCurrency != Currency.RSD)
                {
                    var fromRate = await GetExchangeRateAsync(fromCurrency);
                    if (fromRate == null) return null;

                    amountInRsd = amount * fromRate.Value;
                }

                if (toCurrency != Currency.RSD)
                {
                    var toRate = await GetExchangeRateAsync(toCurrency);
                    if (toRate == null) return null;

                    return amountInRsd / toRate.Value;
                }

                return amountInRsd;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Conversion failed");
                return null;
            }
        }

        public async Task<Dictionary<Currency, decimal>> GetAllRatesAsync()
        {
            var cacheKey = $"all_exchange_rates_{DateTime.UtcNow:yyyy-MM-dd}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<Currency, decimal>? cached) && cached != null)
            {
                _logger.LogInformation("Using cached all rates");
                return cached;
            }

            var rates = new Dictionary<Currency, decimal>
            {
                { Currency.RSD, 1m }
            };

            var currencies = new[] { Currency.USD, Currency.EUR, Currency.GBP, Currency.CHF };

            foreach (var currency in currencies)
            {
                var rate = await GetExchangeRateAsync(currency);
                if (rate.HasValue)
                    rates[currency] = rate.Value;
            }

            var expiration = GetUtcMidnightExpiration();

            _cache.Set(cacheKey, rates, expiration);

            return rates;
        }

        private static TimeSpan GetUtcMidnightExpiration()
        {
            var now = DateTime.UtcNow;
            var midnight = now.Date.AddDays(1);
            return midnight - now;
        }

        private class ExchangeRateApiResponse
        {
            public string? Code { get; set; }
            public string? Date { get; set; }

            [JsonPropertyName("exchange_middle")]
            public decimal ExchangeMiddle { get; set; }

            [JsonPropertyName("exchange_buy")]
            public decimal ExchangeBuy { get; set; }

            [JsonPropertyName("exchange_sell")]
            public decimal ExchangeSell { get; set; }
        }
    }
}