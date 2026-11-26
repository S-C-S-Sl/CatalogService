using CatalogService.Logic;
using Mappify;

namespace CatalogService.Resources.MongoDb;

public sealed class ProductDocumentMappingProfile : BaseMappingProfile
{
    public override void CreateMaps(IMappify mappify)
    {
        mappify.CreateMap<Product, ProductDocument>(product => new ProductDocument
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price
        });

        mappify.CreateMap<ProductDocument, Product>(document =>
            new Product(document.Id, document.Name, document.Description, document.Price));
    }
}
