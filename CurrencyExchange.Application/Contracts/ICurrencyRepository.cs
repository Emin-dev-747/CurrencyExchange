using CurrencyExchange.Data.Models;
using CurrencyExchange.Data.Abstractions;

namespace CurrencyExchange.Application.Contracts
{
    public interface ICurrencyRepository
    {
        Task<Result<IEnumerable<Currency>>> GetCacheOfCurrenciesAsync(string defaultCurrency, CancellationToken cancellationToken);
        Task<float> GetCurrencyRateAsync(string sourceCurrency, string targetCurrency, CancellationToken token = default);
    }
}
