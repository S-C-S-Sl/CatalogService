using System.Runtime.CompilerServices;
using CatalogService.Logic;
using Mappify;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CatalogService.Resources.MongoDb.Tests;

public sealed class MongoProductRepositoryTests(MongoDbFixture mongoDbFixture) : IClassFixture<MongoDbFixture>
{
    private static readonly Guid CoffeeId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TeaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid BreadId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public async Task AddAsync_ProductHasAllValues_StoresAllValues()
    {
        var expectedId = CoffeeId;
        var expectedName = "Kaffee";
        var expectedDescription = "Gemahlen";
        var expectedPrice = 1234.5678m;
        var productRepository = CreateProductRepository();

        await productRepository.AddAsync(new Product(expectedId, expectedName, expectedDescription, expectedPrice));

        var actualProduct = await productRepository.GetAsync(expectedId);
        Assert.NotNull(actualProduct);
        Assert.Equal(expectedId, actualProduct.Id);
        Assert.Equal(expectedName, actualProduct.Name);
        Assert.Equal(expectedDescription, actualProduct.Description);
        Assert.Equal(expectedPrice, actualProduct.Price);
    }

    [Fact]
    public async Task AddAsync_NoDescription_StoresProductWithoutDescription()
    {
        var name = "Kaffee";
        var productRepository = CreateProductRepository();

        await productRepository.AddAsync(new Product(CoffeeId, name, null, 4.99m));

        var actualProduct = await productRepository.GetAsync(CoffeeId);
        Assert.NotNull(actualProduct);
        Assert.Null(actualProduct.Description);
    }

    [Fact]
    public async Task GetAsync_ProductDoesNotExist_ReturnsNull()
    {
        var productRepository = CreateProductRepository();

        var actualProduct = await productRepository.GetAsync(CoffeeId);

        Assert.Null(actualProduct);
    }

    [Fact]
    public async Task GetAllAsync_ProductsExist_ReturnsProductsSortedByName()
    {
        var breadName = "Brot";
        var coffeeName = "Kaffee";
        var teaName = "Tee";
        string[] expectedNames = [breadName, coffeeName, teaName];
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(TeaId, teaName, null, 3.50m));
        await productRepository.AddAsync(new Product(CoffeeId, coffeeName, null, 4.99m));
        await productRepository.AddAsync(new Product(BreadId, breadName, null, 2.20m));

        var actualProducts = await productRepository.GetAllAsync().ToListAsync();

        var actualNames = actualProducts.Select(product => product.Name);
        Assert.Equal(expectedNames, actualNames);
    }

    [Fact]
    public async Task ReplaceAsync_ProductExists_ReplacesAllValues()
    {
        var expectedName = "Espresso";
        var expectedPrice = 5.50m;
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(CoffeeId, "Kaffee", "Gemahlen", 4.99m));

        var actualWasReplaced = await productRepository.ReplaceAsync(new Product(CoffeeId, expectedName, null, expectedPrice));

        Assert.True(actualWasReplaced);
        var actualProduct = await productRepository.GetAsync(CoffeeId);
        Assert.NotNull(actualProduct);
        Assert.Equal(expectedName, actualProduct.Name);
        Assert.Null(actualProduct.Description);
        Assert.Equal(expectedPrice, actualProduct.Price);
    }

    [Fact]
    public async Task ReplaceAsync_ProductDoesNotExist_ReturnsFalse()
    {
        var name = "Kaffee";
        var productRepository = CreateProductRepository();

        var actualWasReplaced = await productRepository.ReplaceAsync(new Product(CoffeeId, name, null, 4.99m));

        Assert.False(actualWasReplaced);
    }

    [Fact]
    public async Task DeleteAsync_ProductExists_RemovesProduct()
    {
        var name = "Kaffee";
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(CoffeeId, name, null, 4.99m));

        var actualWasDeleted = await productRepository.DeleteAsync(CoffeeId);

        Assert.True(actualWasDeleted);
        Assert.Null(await productRepository.GetAsync(CoffeeId));
    }

    [Fact]
    public async Task DeleteAsync_ProductDoesNotExist_ReturnsFalse()
    {
        var productRepository = CreateProductRepository();

        var actualWasDeleted = await productRepository.DeleteAsync(CoffeeId);

        Assert.False(actualWasDeleted);
    }

    [Fact]
    public async Task GetAsync_NewRepositoryOnSameDatabase_ReturnsStoredProduct()
    {
        const string databaseName = "catalog-products-survive-restart";
        var expectedName = "Kaffee";
        var firstProductRepository = CreateProductRepository(databaseName);
        await firstProductRepository.AddAsync(new Product(CoffeeId, expectedName, "Gemahlen", 4.99m));
        var restartedProductRepository = CreateProductRepository(databaseName);

        var actualProduct = await restartedProductRepository.GetAsync(CoffeeId);

        Assert.NotNull(actualProduct);
        Assert.Equal(expectedName, actualProduct.Name);
    }

    private MongoProductRepository CreateProductRepository([CallerMemberName] string databaseName = "")
    {
        var services = new ServiceCollection();
        services.AddMappify();
        services.AddMappifyProfile<ProductDocumentMappingProfile>();
        var mappify = services.BuildServiceProvider().GetRequiredService<IMappify>();

        var productCollection = new MongoClient(mongoDbFixture.ConnectionString)
            .GetDatabase(databaseName)
            .GetCollection<ProductDocument>("products");

        return new MongoProductRepository(productCollection, mappify);
    }
}
