using System.Runtime.CompilerServices;
using CatalogService.Logic;
using Mappify;
using MongoDB.Driver;

namespace CatalogService.Resources.MongoDb;

public sealed class MongoProductRepository(IMongoCollection<ProductDocument> productCollection, IMappify mappify)
    : IProductRepository
{
    public async IAsyncEnumerable<Product> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var documents = productCollection
            .Find(FilterDefinition<ProductDocument>.Empty)
            .SortBy(document => document.Name)
            .ToAsyncEnumerable();

        await foreach (var document in documents.WithCancellation(cancellationToken))
        {
            yield return mappify.Map<Product>(document);
        }
    }

    public async Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await productCollection
            .Find(candidate => candidate.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : mappify.Map<Product>(document);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        var document = mappify.Map<ProductDocument>(product);
        await productCollection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task<bool> ReplaceAsync(Product product, CancellationToken cancellationToken = default)
    {
        var document = mappify.Map<ProductDocument>(product);
        var result = await productCollection.ReplaceOneAsync(
            candidate => candidate.Id == document.Id, document, cancellationToken: cancellationToken);

        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await productCollection.DeleteOneAsync(candidate => candidate.Id == id, cancellationToken);

        return result.DeletedCount > 0;
    }
}
