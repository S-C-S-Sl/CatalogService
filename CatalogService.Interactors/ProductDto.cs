namespace CatalogService.Interactors
{
    /// <summary>Produkt, wie es die API nach außen gibt.</summary>
    /// <param name="Id">Eindeutige Id des Produkts.</param>
    /// <param name="Name">Name des Produkts.</param>
    /// <param name="Description">Beschreibung, kann fehlen.</param>
    /// <param name="Price">Preis, nicht negativ.</param>
    public sealed record ProductDto(

        Guid Id,
        string Name,
        string? Description,
        decimal Price);
}
