using CatalogService.Logic;
using CatalogService.Resources;
using Mappify;

namespace CatalogService.Interactors;

public sealed class ProductInteractor(IProductRepository productRepository, IMappify mappify) : IProductInteractor
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        return products.Select(product => mappify.Map<ProductDto>(product)).ToList();
    }

    public async Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetAsync(id, cancellationToken);

        return product is null ? null : mappify.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = mappify.Map<Product>(request);
        await productRepository.AddAsync(product, cancellationToken);

        return mappify.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = mappify.Map<Product>(new ReplaceProductCommand(id, request));
        var wasReplaced = await productRepository.ReplaceAsync(product, cancellationToken);

        return wasReplaced ? mappify.Map<ProductDto>(product) : null;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productRepository.DeleteAsync(id, cancellationToken);
    }
}
