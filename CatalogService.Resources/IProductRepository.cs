using CatalogService.Logic;

namespace CatalogService.Resources;

public interface IProductRepository
{
    IAsyncEnumerable<Product> GetAllAsync(CancellationToken cancellationToken = default);

    /// <returns>Das Produkt oder <c>null</c>, wenn es nicht existiert.</returns>
    Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <returns><c>false</c>, wenn das Produkt nicht existiert.</returns>
    Task<bool> ReplaceAsync(Product product, CancellationToken cancellationToken = default);

    /// <returns><c>false</c>, wenn das Produkt nicht existiert.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
