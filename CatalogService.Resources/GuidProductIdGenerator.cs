namespace CatalogService.Resources;

public sealed class GuidProductIdGenerator : IProductIdGenerator
{
    public Guid Generate()
    {
        return Guid.NewGuid();
    }
}
