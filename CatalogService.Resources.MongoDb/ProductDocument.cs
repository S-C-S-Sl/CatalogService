using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CatalogService.Resources.MongoDb;

/// <summary>Produkt, wie es in der MongoDB gespeichert wird.</summary>
public sealed class ProductDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }
}
