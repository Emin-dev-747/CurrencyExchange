namespace CurrencyExchange.Data.Abstractions;

public abstract class Entity
{
    protected Entity(Guid id){Id = id;}

    protected Entity(){}

    public Guid Id { get; init; }

    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}
