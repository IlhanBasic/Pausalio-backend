using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Enums;

namespace Pausalio.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IMemoryCache _cache;

        public ExchangeRateController(
            IExchangeRateService exchangeRateService,
            IMemoryCache cache)
        {
            _exchangeRateService = exchangeRateService;
            _cache = cache;
        }

        /// <summary>
        /// Dohvata trenutni kurs za određenu valutu prema RSD-u (sa caching-om)
        /// </summary>
        /// <param name="currency">Valuta (0=RSD, 1=USD, 2=EUR, 3=GBP, 4=CHF)</param>
        [HttpGet("rate/{currency}")]
        public async Task<IActionResult> GetExchangeRate(Currency currency)
        {
            try
            {
                var cacheKey = $"exchange_rate_{currency}_{DateTime.UtcNow:yyyy-MM-dd}";

                if (!_cache.TryGetValue(cacheKey, out decimal? rate))
                {
                    rate = await _exchangeRateService.GetExchangeRateAsync(currency);

                    if (rate.HasValue)
                    {
                        var cacheExpiration = DateTime.Today.AddDays(1) - DateTime.Now;
                        _cache.Set(cacheKey, rate.Value, cacheExpiration);
                    }
                }

                if (rate == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Kurs za {currency} nije dostupan"
                    });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        currency = currency.ToString(),
                        rate = rate.Value,
                        description = $"1 {currency} = {rate.Value:N4} RSD",
                        cached = _cache.TryGetValue(cacheKey, out _)
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Konvertuje iznos iz jedne valute u drugu (sa caching-om)
        /// </summary>
        /// <param name="amount">Iznos koji se konvertuje</param>
        /// <param name="from">Iz valute (0=RSD, 1=USD, 2=EUR, 3=GBP, 4=CHF)</param>
        /// <param name="to">U valutu (0=RSD, 1=USD, 2=EUR, 3=GBP, 4=CHF)</param>
        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency(
            [FromQuery] decimal amount,
            [FromQuery] Currency from,
            [FromQuery] Currency to)
        {
            try
            {
                if (amount <= 0)
                    return BadRequest(new { success = false, message = "Iznos mora biti veći od 0" });

                var cacheKey = $"convert_{from}_{to}_{amount}_{DateTime.UtcNow:yyyy-MM-dd}";

                if (!_cache.TryGetValue(cacheKey, out decimal? result))
                {
                    result = await _exchangeRateService.ConvertAsync(amount, from, to);

                    if (result.HasValue)
                    {
                        var cacheExpiration = DateTime.Today.AddDays(1) - DateTime.Now;
                        _cache.Set(cacheKey, result.Value, cacheExpiration);
                    }
                }

                if (result == null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Konverzija iz {from} u {to} nije uspela"
                    });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        originalAmount = amount,
                        fromCurrency = from.ToString(),
                        toCurrency = to.ToString(),
                        convertedAmount = Math.Round(result.Value, 2),
                        calculation = $"{amount:N2} {from} = {Math.Round(result.Value, 2):N2} {to}",
                        cached = _cache.TryGetValue(cacheKey, out _)
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Dohvata trenutne kurseve za sve podržane valute (sa caching-om)
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRates()
        {
            try
            {
                var cacheKey = $"all_rates_{DateTime.UtcNow:yyyy-MM-dd}";

                if (!_cache.TryGetValue(cacheKey, out Dictionary<Currency, decimal>? rates))
                {
                    rates = await _exchangeRateService.GetAllRatesAsync();

                    if (rates != null && rates.Any())
                    {
                        var cacheExpiration = DateTime.Today.AddDays(1) - DateTime.Now;
                        _cache.Set(cacheKey, rates, cacheExpiration);
                    }
                }

                if (rates == null || !rates.Any())
                    return NotFound(new { success = false, message = "Kursevi nisu dostupni" });

                return Ok(new
                {
                    success = true,
                    data = rates.Select(x => new
                    {
                        currency = x.Key.ToString(),
                        rate = x.Value,
                        description = $"1 {x.Key} = {x.Value:N4} RSD"
                    }),
                    count = rates.Count,
                    cached = _cache.TryGetValue(cacheKey, out _)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [ADMIN] Briše cache za kurseve (force refresh)
        /// </summary>
        [HttpDelete("cache/clear")]
        public IActionResult ClearCache()
        {
            try
            {
                var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var currencies = new[] { Currency.RSD, Currency.USD, Currency.EUR, Currency.GBP, Currency.CHF };

                foreach (var currency in currencies)
                {
                    var key = $"exchange_rate_{currency}_{today}";
                    _cache.Remove(key);
                }

                _cache.Remove($"all_rates_{today}");

                return Ok(new
                {
                    success = true,
                    message = "Cache je uspešno obrisan. Sledeći poziv će dohvatiti nove podatke sa API-ja."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}