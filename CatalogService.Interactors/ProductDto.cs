namespace CatalogService.Interactors
{
    /// <summary>Produkt, wie es die API nach außen gibt.</summary>
    public sealed record ProductDto(Guid Id, string Name, string? Description, decimal Price);
}
