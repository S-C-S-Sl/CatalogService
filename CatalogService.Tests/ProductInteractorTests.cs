using CatalogService.Interactors;
using CatalogService.Logic;
using CatalogService.Resources;
using Mappify;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CatalogService.Tests;

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
    public async Task CreateAsync_StoresProductWithGeneratedId()
    {
        productIdGenerator.Generate().Returns(ProductId);
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);

        await productInteractor.CreateAsync(request);

        await productRepository.Received(1).AddAsync(
            Arg.Is<Product>(product => product.Id == ProductId
                                       && product.Name == "Kaffee"
                                       && product.Description == "Gemahlen"
                                       && product.Price == 4.99m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsTheSavedValues()
    {
        productIdGenerator.Generate().Returns(ProductId);
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);

        var created = await productInteractor.CreateAsync(request);

        Assert.Equal(new ProductDto(ProductId, "Kaffee", "Gemahlen", 4.99m), created);
    }

    [Fact]
    public async Task GetAsync_ReturnsTheStoredProduct()
    {
        productRepository.GetAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(new Product(ProductId, "Kaffee", "Gemahlen", 4.99m));

        var found = await productInteractor.GetAsync(ProductId);

        Assert.Equal(new ProductDto(ProductId, "Kaffee", "Gemahlen", 4.99m), found);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenProductDoesNotExist()
    {
        productRepository.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var found = await productInteractor.GetAsync(ProductId);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStoredProducts()
    {
        var otherProductId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        IReadOnlyList<Product> storedProducts =
        [
            new Product(ProductId, "Kaffee", null, 4.99m),
            new Product(otherProductId, "Tee", "Schwarz", 3.50m)
        ];
        productRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(storedProducts);

        var products = await productInteractor.GetAllAsync();

        Assert.Equal(
            [new ProductDto(ProductId, "Kaffee", null, 4.99m), new ProductDto(otherProductId, "Tee", "Schwarz", 3.50m)],
            products);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesTheStoredProductAndKeepsItsId()
    {
        productRepository.ReplaceAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(true);

        var updated = await productInteractor.UpdateAsync(ProductId, new UpdateProductRequest("Tee", null, 3.50m));

        Assert.Equal(new ProductDto(ProductId, "Tee", null, 3.50m), updated);
        await productRepository.Received(1).ReplaceAsync(
            Arg.Is<Product>(product => product.Id == ProductId && product.Name == "Tee" && product.Price == 3.50m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullWhenProductDoesNotExist()
    {
        productRepository.ReplaceAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(false);

        var updated = await productInteractor.UpdateAsync(ProductId, new UpdateProductRequest("Tee", null, 3.50m));

        Assert.Null(updated);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_ReturnsWhetherTheProductExisted(bool existed)
    {
        productRepository.DeleteAsync(ProductId, Arg.Any<CancellationToken>()).Returns(existed);

        var deleted = await productInteractor.DeleteAsync(ProductId);

        Assert.Equal(existed, deleted);
    }
}
