namespace CatalogService.Interactors;

/// <summary>Quelle für das Mapping beim Ändern: die Id des Produkts und die neuen Angaben.</summary>
internal sealed record ReplaceProductCommand(Guid Id, UpdateProductRequest Request);
