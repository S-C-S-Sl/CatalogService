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
    private readonly ProductInteractor productInteractor;

    public ProductInteractorTests()
    {
        var services = new ServiceCollection();
        services.AddMappify();
        services.AddMappifyProfile<ProductMappingProfile>();
        var mappify = services.BuildServiceProvider().GetRequiredService<IMappify>();

        productInteractor = new ProductInteractor(productRepository, mappify);
    }

    [Fact]
    public async Task CreateAsync_StoresProductWithNewId()
    {
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);

        var created = await productInteractor.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, created.Id);
        await productRepository.Received(1).AddAsync(
            Arg.Is<Product>(product => product.Id == created.Id
                                       && product.Name == "Kaffee"
                                       && product.Description == "Gemahlen"
                                       && product.Price == 4.99m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsTheSavedValues()
    {
        var request = new CreateProductRequest("Kaffee", "Gemahlen", 4.99m);

        var created = await productInteractor.CreateAsync(request);

        Assert.Equal("Kaffee", created.Name);
        Assert.Equal("Gemahlen", created.Description);
        Assert.Equal(4.99m, created.Price);
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
    public async Task UpdateAsync_ReplacesTheStoredProduct()
    {
        productRepository.GetAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(new Product(ProductId, "Kaffee", "Gemahlen", 4.99m));
        productRepository.ReplaceAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>()).Returns(true);

        var updated = await productInteractor.UpdateAsync(ProductId, new UpdateProductRequest("Tee", null, 3.50m));

        Assert.Equal(new ProductDto(ProductId, "Tee", null, 3.50m), updated);
        await productRepository.Received(1).ReplaceAsync(
            Arg.Is<Product>(product => product.Id == ProductId && product.Name == "Tee" && product.Price == 3.50m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullAndStoresNothingWhenProductDoesNotExist()
    {
        productRepository.GetAsync(ProductId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var updated = await productInteractor.UpdateAsync(ProductId, new UpdateProductRequest("Tee", null, 3.50m));

        Assert.Null(updated);
        await productRepository.DidNotReceive().ReplaceAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
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
