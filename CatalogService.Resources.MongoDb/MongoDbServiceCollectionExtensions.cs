using Mappify;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CatalogService.Resources.MongoDb;

public static class MongoDbServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDbResources(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMappifyProfile<ProductDocumentMappingProfile>();
        services.AddSingleton<IMongoClient>(CreateMongoClient);
        services.AddSingleton(CreateProductCollection);
        services.AddSingleton<IProductRepository, MongoProductRepository>();

        return services;
    }

    private static IMongoClient CreateMongoClient(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>().Value;

        return new MongoClient(options.ConnectionString);
    }

    private static IMongoCollection<ProductDocument> CreateProductCollection(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();

        return mongoClient
            .GetDatabase(options.DatabaseName)
            .GetCollection<ProductDocument>(options.ProductCollectionName);
    }
}
