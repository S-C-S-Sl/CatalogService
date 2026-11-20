using System.ComponentModel.DataAnnotations;

namespace CatalogService.Interactors;

/// <summary>Eingabe zum Ändern eines Produkts (ersetzt alle Angaben).</summary>
public sealed record UpdateProductRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [StringLength(2000)] string? Description,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal Price);