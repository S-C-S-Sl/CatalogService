namespace CatalogService.Logic;

public sealed class Product
{
    public Product(Guid id, string name, string? description, decimal price)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die Id darf nicht leer sein.", nameof(id));
        }

        Id = id;
        ChangeDetails(name, description, price);
    }

    public Guid Id { get; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public void ChangeDetails(string name, string? description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Der Name darf nicht leer sein.", nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Der Preis darf nicht negativ sein.");
        }

        Name = name;
        Description = description;
        Price = price;
    }
}
