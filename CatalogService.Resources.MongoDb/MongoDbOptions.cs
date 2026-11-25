using System.ComponentModel.DataAnnotations;

namespace CatalogService.Resources.MongoDb;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = "catalog";

    [Required]
    public string ProductCollectionName { get; set; } = "products";
}
