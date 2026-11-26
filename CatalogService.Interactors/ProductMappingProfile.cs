using CatalogService.Logic;
using CatalogService.Resources;
using Mappify;

namespace CatalogService.Interactors;

public sealed class ProductMappingProfile(IProductIdGenerator productIdGenerator) : BaseMappingProfile
{
    public override void CreateMaps(IMappify mappify)
    {
        mappify.CreateMap<Product, ProductDto>(product =>
            new ProductDto(product.Id, product.Name, product.Description, product.Price));

        mappify.CreateMap<CreateProductRequest, Product>(request =>
            new Product(productIdGenerator.Generate(), request.Name, request.Description, request.Price));

        mappify.CreateMap<ReplaceProductCommand, Product>(command =>
            new Product(command.Id, command.Request.Name, command.Request.Description, command.Request.Price));
    }
}
