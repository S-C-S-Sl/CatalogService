using System.ComponentModel.DataAnnotations;

namespace CatalogService.Interactors;

/// <summary>Eingabe zum Anlegen eines Produkts. Die API prüft die Angaben vor dem Aufruf.</summary>
public sealed record CreateProductRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [StringLength(2000)] string? Description,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal Price);