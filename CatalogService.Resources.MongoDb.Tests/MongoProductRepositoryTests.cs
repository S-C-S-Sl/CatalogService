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
    public async Task AddAsync_StoresTheProductWithAllValues()
    {
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(CoffeeId, "Kaffee", "Gemahlen", 1234.5678m));

        var stored = await productRepository.GetAsync(CoffeeId);

        Assert.NotNull(stored);
        Assert.Equal(CoffeeId, stored.Id);
        Assert.Equal("Kaffee", stored.Name);
        Assert.Equal("Gemahlen", stored.Description);
        Assert.Equal(1234.5678m, stored.Price);
    }

    [Fact]
    public async Task AddAsync_StoresTheProductWithoutDescription()
    {
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(CoffeeId, "Kaffee", null, 4.99m));

        var stored = await productRepository.GetAsync(CoffeeId);

        Assert.NotNull(stored);
        Assert.Null(stored.Description);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenProductDoesNotExist()
    {
        var productRepository = CreateProductRepository();

        var stored = await productRepository.GetAsync(CoffeeId);

        Assert.Null(stored);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProductsSortedByName()
    {
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(TeaId, "Tee", null, 3.50m));
        await productRepository.AddAsync(new Product(CoffeeId, "Kaffee", null, 4.99m));
        await productRepository.AddAsync(new Product(BreadId, "Brot", null, 2.20m));

        var products = await productRepository.GetAllAsync();

        Assert.Equal(["Brot", "Kaffee", "Tee"], products.Select(product => product.Name));
    }

    [Fact]
    public async Task ReplaceAsync_ReplacesAllValuesOfTheStoredProduct()
    {
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(CoffeeId, "Kaffee", "Gemahlen", 4.99m));

        var wasReplaced = await productRepository.ReplaceAsync(new Product(CoffeeId, "Espresso", null, 5.50m));

        var stored = await productRepository.GetAsync(CoffeeId);
        Assert.True(wasReplaced);
        Assert.NotNull(stored);
        Assert.Equal("Espresso", stored.Name);
        Assert.Null(stored.Description);
        Assert.Equal(5.50m, stored.Price);
    }

    [Fact]
    public async Task ReplaceAsync_ReturnsFalseWhenProductDoesNotExist()
    {
        var productRepository = CreateProductRepository();

        var wasReplaced = await productRepository.ReplaceAsync(new Product(CoffeeId, "Kaffee", null, 4.99m));

        Assert.False(wasReplaced);
        Assert.Null(await productRepository.GetAsync(CoffeeId));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheStoredProduct()
    {
        var productRepository = CreateProductRepository();
        await productRepository.AddAsync(new Product(CoffeeId, "Kaffee", null, 4.99m));

        var wasDeleted = await productRepository.DeleteAsync(CoffeeId);

        Assert.True(wasDeleted);
        Assert.Null(await productRepository.GetAsync(CoffeeId));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalseWhenProductDoesNotExist()
    {
        var productRepository = CreateProductRepository();

        var wasDeleted = await productRepository.DeleteAsync(CoffeeId);

        Assert.False(wasDeleted);
    }

    [Fact]
    public async Task Products_AreStillAvailableForANewRepositoryOnTheSameDatabase()
    {
        const string databaseName = "catalog-products-survive-restart";
        var firstProductRepository = CreateProductRepository(databaseName);
        await firstProductRepository.AddAsync(new Product(CoffeeId, "Kaffee", "Gemahlen", 4.99m));

        var restartedProductRepository = CreateProductRepository(databaseName);
        var stored = await restartedProductRepository.GetAsync(CoffeeId);

        Assert.NotNull(stored);
        Assert.Equal("Kaffee", stored.Name);
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
