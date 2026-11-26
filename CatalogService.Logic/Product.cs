namespace CatalogService.Logic;

public sealed class Product
{
    public Product(Guid id, string name, string? description, decimal price)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die Id darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Der Name darf nicht leer sein.", nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Der Preis darf nicht negativ sein.");
        }

        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string? Description { get; }

    public decimal Price { get; }
}
