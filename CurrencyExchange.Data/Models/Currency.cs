using CurrencyExchange.Data.Abstractions;

namespace CurrencyExchange.Data.Models
{
    public class Currency : Entity
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public float Rate { get; set; }
    }
}
