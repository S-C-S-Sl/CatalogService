using Testcontainers.MongoDb;

namespace CatalogService.Resources.MongoDb.Tests;

/// <summary>Startet eine MongoDB im Container, die sich alle Tests einer Klasse teilen. Der Cluster nutzt dieselbe Version.</summary>
public sealed class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer mongoDbContainer = new MongoDbBuilder("mongo:8.3").Build();

    public string ConnectionString
    {
        get
        {
            return mongoDbContainer.GetConnectionString();
        }
    }

    public Task InitializeAsync()
    {
        return mongoDbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await mongoDbContainer.DisposeAsync();
    }
}
