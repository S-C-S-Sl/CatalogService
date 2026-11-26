namespace CatalogService.Interactors;

/// <summary>Quelle für das Mapping beim Ändern: die Id des Produkts und die neuen Angaben.</summary>
/// <param name="Id">Id des Produkts, das ersetzt wird.</param>
/// <param name="Request">Die neuen Angaben.</param>
internal sealed record ReplaceProductCommand(

    Guid Id,
    UpdateProductRequest Request);
