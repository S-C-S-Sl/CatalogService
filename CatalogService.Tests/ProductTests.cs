using CatalogService.Logic;

namespace CatalogService.Tests;

public sealed class ProductTests
{
    private static readonly Guid ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Constructor_SetsAllValues()
    {
        var product = new Product(ProductId, "Kaffee", "Gemahlen", 4.99m);

        Assert.Equal(ProductId, product.Id);
        Assert.Equal("Kaffee", product.Name);
        Assert.Equal("Gemahlen", product.Description);
        Assert.Equal(4.99m, product.Price);
    }

    [Fact]
    public void Constructor_RejectsEmptyId()
    {
        Assert.Throws<ArgumentException>(() => new Product(Guid.Empty, "Kaffee", null, 4.99m));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsMissingName(string name)
    {
        Assert.Throws<ArgumentException>(() => new Product(ProductId, name, null, 4.99m));
    }

    [Fact]
    public void Constructor_RejectsNegativePrice()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(ProductId, "Kaffee", null, -0.01m));
    }

    [Fact]
    public void Constructor_AcceptsPriceZero()
    {
        var product = new Product(ProductId, "Gratisprobe", null, 0m);

        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void ChangeDetails_ReplacesAllValuesAndKeepsId()
    {
        var product = new Product(ProductId, "Kaffee", "Gemahlen", 4.99m);

        product.ChangeDetails("Tee", null, 3.50m);

        Assert.Equal(ProductId, product.Id);
        Assert.Equal("Tee", product.Name);
        Assert.Null(product.Description);
        Assert.Equal(3.50m, product.Price);
    }

    [Fact]
    public void ChangeDetails_KeepsOldValuesWhenNewValuesAreInvalid()
    {
        var product = new Product(ProductId, "Kaffee", "Gemahlen", 4.99m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ChangeDetails("Tee", "Schwarz", -1m));

        Assert.Equal("Kaffee", product.Name);
        Assert.Equal("Gemahlen", product.Description);
        Assert.Equal(4.99m, product.Price);
    }
}
