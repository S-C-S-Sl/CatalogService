namespace CatalogService.Interactors;

public interface IProductInteractor
{
    IAsyncEnumerable<ProductDto> GetAllAsync(CancellationToken cancellationToken = default);

    /// <returns>Das Produkt oder <c>null</c>, wenn es nicht existiert.</returns>
    Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <returns>Das geänderte Produkt oder <c>null</c>, wenn es nicht existiert.</returns>
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    /// <returns><c>false</c>, wenn das Produkt nicht existiert.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}