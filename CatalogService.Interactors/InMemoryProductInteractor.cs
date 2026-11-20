using System.Collections.Concurrent;

namespace CatalogService.Interactors;

public sealed class InMemoryProductInteractor : IProductInteractor
{
    private readonly ConcurrentDictionary<Guid, ProductDto> _products = new();

    public Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProductDto>>(_products.Values.OrderBy(p => p.Name).ToList());

    public Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_products.GetValueOrDefault(id));

    public Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new ProductDto(Guid.NewGuid(), request.Name, request.Description, request.Price);
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (!_products.ContainsKey(id)) return Task.FromResult<ProductDto?>(null);
        var product = new ProductDto(id, request.Name, request.Description, request.Price);
        _products[id] = product;
        return Task.FromResult<ProductDto?>(product);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_products.TryRemove(id, out _));
}