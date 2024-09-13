using CurrencyExchange.Application.Contracts;
using CurrencyExchange.Application.Policies;
using CurrencyExchange.Data.Abstractions;
using CurrencyExchange.Data.Models;
using CurrencyExchange.Data;
using System.Text.Json;
using CurrencyExchange.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Application.Repositories
{
    public sealed class CurrencyRepository : ICurrencyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ClientPolicy _clientPolicy;
        private readonly ICacheService _cacheService;
        private const string CurrenciesUrl = "https://api.exchangerate-api.com/v4/latest/";
        public CurrencyRepository(ApplicationDbContext context,
            IDistributedCache distributedCache,
            ClientPolicy clientPolicy,
            ICacheService cacheService)
        {
            _context = context;
            _clientPolicy = clientPolicy;
            _cacheService = cacheService;
        }
        public async Task<Result<IEnumerable<Currency>>> GetCacheOfCurrenciesAsync(string defaultCurrency,
            CancellationToken cancellationToken)
        {
            var currentDate = DateTime.Now.Date;
            var oneHour = TimeSpan.FromHours(1);
            string key = $"CurreciesFor - {defaultCurrency} on {currentDate}";

            var cahsedCurrencies = await _cacheService.GetAsync<IEnumerable<Currency>>(key, cancellationToken);
            
            if (cahsedCurrencies != null)
            {
                return Result.Success(cahsedCurrencies);
            }

            var currencies = await GetCurrynciesAsync(defaultCurrency, cancellationToken);
            await _cacheService.SetAsync(key, currencies, oneHour, cancellationToken);
            return Result.Success(currencies);
        }
        private async Task<IEnumerable<Currency>> GetCurrynciesAsync(string defaultCurrency, CancellationToken cancellationToken)
        {
            var currencies = await _context.Currencies.ToListAsync(cancellationToken);

            string url = string.Format(CurrenciesUrl + defaultCurrency);
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await _clientPolicy.ExponentialHttpRetry
                        .ExecuteAsync(() => client.GetAsync(url, cancellationToken));

                    string content = await response.Content.ReadAsStringAsync(cancellationToken);

                    JsonDocument document = JsonDocument.Parse(content);
                    JsonElement rates = document.RootElement.GetProperty("rates");

                    foreach (var item in currencies)
                    {
                        item.Rate = rates.GetProperty(item.ShortName).GetSingle();
                        _context.Update(item);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return currencies;
            }
            catch (Exception)
            {
                return Enumerable.Empty<Currency>();
            }
        }

        public async Task<float> GetCurrencyRateAsync(string sourceCurrency, string targetCurrency, CancellationToken cancellationToken)
        {
            float exchangeRate = -1f;
            string url = string.Format(CurrenciesUrl + sourceCurrency);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await _clientPolicy.ExponentialHttpRetry
                    .ExecuteAsync(() => client.GetAsync(url, cancellationToken));

                    string content = await response.Content.ReadAsStringAsync(cancellationToken);

                    JsonDocument document = JsonDocument.Parse(content);
                    JsonElement rates = document.RootElement.GetProperty("rates");
                    exchangeRate = rates.GetProperty(targetCurrency).GetSingle();
                    return exchangeRate;
                }
            }
            catch
            {
                return exchangeRate;
            }
        }
    }
}
