using CatalogService.Logic;
using CatalogService.Resources;
using Mappify;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CatalogService.Interactors.Tests;

public sealed class ProductInteractorTests
{
    private static readonly Guid ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IProductRepository productRepository = Substitute.For<IProductRepository>();
    private readonly IProductIdGenerator productIdGenerator = Substitute.For<IProductIdGenerator>();
    private readonly ProductInteractor productInteractor;

    public ProductInteractorTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(productIdGenerator);
        services.AddMappify();
        services.AddMappifyProfile<ProductMappingProfile>();
        var mappify = services.BuildServiceProvider().GetRequiredService<IMappify>();

        productInteractor = new ProductInteractor(productRepository, mappify);
    }

    [Fact]
    public async Task CreateAsync_RequestIsValid_StoresProductWithGeneratedId()
    {
        var expectedId = ProductId;
        var expectedName = "Kaffee";
        var expectedDescription = "Gemahlen";
        var expectedPrice = 4.99m;
        productIdGenerator.Generate().Returns(expectedId);
        var request = new CreateProductRequest(expectedName, expectedDescription, expectedPrice);

        await productInteractor.CreateAsync(request);

        await productRepository.Received(1).AddAsync(
            Arg.Is<Product>(product => product.Id == expectedId
                                       && product.Name == expectedName
                                       && product.Description == expectedDescription
                                       && product.Price == expectedPrice),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_RequestIsValid_ReturnsTheSavedValues()
    {
        var expectedProduct = new ProductDto(ProductId, "Kaffee", "Gemahlen", 4.99m);
        productIdGenerator.Generate().Returns(ProductId);
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);

        var actualProduct = await productInteractor.CreateAsync(request);

        Assert.Equal(expectedProduct, actualProduct);
    }

    [Fact]
    public async Task GetAsync_ProductExists_ReturnsProductDto()
    {
        var expectedProduct = new ProductDto(ProductId, "Kaffee", "Gemahlen", 4.99m);
        productRepository.GetAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(new Product(ProductId, "Kaffee", "Gemahlen", 4.99m));

        var actualProduct = await productInteractor.GetAsync(ProductId);

        Assert.Equal(expectedProduct, actualProduct);
    }

    [Fact]
    public async Task GetAsync_ProductDoesNotExist_ReturnsNull()
    {
        productRepository.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var actualProduct = await productInteractor.GetAsync(ProductId);

        Assert.Null(actualProduct);
    }

    [Fact]
    public async Task GetAllAsync_ProductsExist_ReturnsAllProductDtos()
    {
        var otherProductId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var storedProducts = new[]
        {
            new Product(ProductId, "Kaffee", null, 4.99m),
            new Product(otherProductId, "Tee", "Schwarz", 3.50m)
        };
        var expectedProducts = new[]
        {
            new ProductDto(ProductId, "Kaffee", null, 4.99m),
            new ProductDto(otherProductId, "Tee", "Schwarz", 3.50m)
        };
        productRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(storedProducts.ToAsyncEnumerable());

        var actualProducts = await productInteractor.GetAllAsync().ToListAsync();

        Assert.Equal(expectedProducts, actualProducts);
    }

    [Fact]
    public async Task UpdateAsync_ProductExists_ReplacesProductAndKeepsItsId()
    {
        var expectedId = ProductId;
        var expectedName = "Tee";
        var expectedPrice = 3.50m;
        var expectedProduct = new ProductDto(expectedId, expectedName, null, expectedPrice);
        productRepository.ReplaceAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(true);

        var actualProduct = await productInteractor.UpdateAsync(expectedId, new UpdateProductRequest(expectedName, null, expectedPrice));

        Assert.Equal(expectedProduct, actualProduct);
        await productRepository.Received(1).ReplaceAsync(
            Arg.Is<Product>(product => product.Id == expectedId && product.Name == expectedName && product.Price == expectedPrice),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ProductDoesNotExist_ReturnsNull()
    {
        productRepository.ReplaceAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(false);

        var actualProduct = await productInteractor.UpdateAsync(ProductId, new UpdateProductRequest("Tee", null, 3.50m));

        Assert.Null(actualProduct);
    }

    [Fact]
    public async Task DeleteAsync_ProductExisted_ReturnsTrue()
    {
        productRepository.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(true);

        var actualWasDeleted = await productInteractor.DeleteAsync(ProductId);

        Assert.True(actualWasDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ProductDidNotExist_ReturnsFalse()
    {
        productRepository.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(false);

        var actualWasDeleted = await productInteractor.DeleteAsync(ProductId);

        Assert.False(actualWasDeleted);
    }
}
