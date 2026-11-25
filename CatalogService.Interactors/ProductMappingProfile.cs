using CatalogService.Logic;
using Mappify;

namespace CatalogService.Interactors;

public sealed class ProductMappingProfile : BaseMappingProfile
{
    public override void CreateMaps(IMappify mappify)
    {
        mappify.CreateMap<Product, ProductDto>(product =>
            new ProductDto(product.Id, product.Name, product.Description, product.Price));

        mappify.CreateMap<CreateProductRequest, Product>(request =>
            new Product(Guid.NewGuid(), request.Name, request.Description, request.Price));
    }
}
