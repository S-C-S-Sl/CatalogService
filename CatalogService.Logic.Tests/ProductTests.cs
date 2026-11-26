namespace CatalogService.Logic.Tests;

public sealed class ProductTests
{
    private static readonly Guid ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Constructor_AllValuesAreValid_SetsAllValues()
    {
        var expectedId = ProductId;
        var expectedName = "Kaffee";
        var expectedDescription = "Gemahlen";
        var expectedPrice = 4.99m;

        var actualProduct = new Product(expectedId, expectedName, expectedDescription, expectedPrice);

        Assert.Equal(expectedId, actualProduct.Id);
        Assert.Equal(expectedName, actualProduct.Name);
        Assert.Equal(expectedDescription, actualProduct.Description);
        Assert.Equal(expectedPrice, actualProduct.Price);
    }

    [Fact]
    public void Constructor_IdIsEmpty_ThrowsException()
    {
        var emptyId = Guid.Empty;
        var name = "Kaffee";

        var actualException = Record.Exception(() => new Product(emptyId, name, null, 4.99m));

        Assert.IsType<ArgumentException>(actualException);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NameIsMissing_ThrowsException(string missingName)
    {
        var price = 4.99m;

        var actualException = Record.Exception(() => new Product(ProductId, missingName, null, price));

        Assert.IsType<ArgumentException>(actualException);
    }

    [Fact]
    public void Constructor_PriceIsNegative_ThrowsException()
    {
        var name = "Kaffee";
        var negativePrice = -0.01m;

        var actualException = Record.Exception(() => new Product(ProductId, name, null, negativePrice));

        Assert.IsType<ArgumentOutOfRangeException>(actualException);
    }

    [Fact]
    public void Constructor_PriceIsZero_AcceptsPrice()
    {
        var name = "Gratisprobe";
        var expectedPrice = 0m;

        var actualProduct = new Product(ProductId, name, null, expectedPrice);

        Assert.Equal(expectedPrice, actualProduct.Price);
    }
}
