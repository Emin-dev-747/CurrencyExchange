namespace CurrencyExchange.Data.Errors
{
    public static class CurrencyErrors
    {
        public static readonly Error NotFound = new(
        "Currency.NotFound",
        "The currency with the specified identifier was not found");
    }
}
