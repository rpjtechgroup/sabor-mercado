using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Catalog;

public sealed class Store
{
    public Guid Id { get; set; } = Ids.NewId();

    public required string Name { get; set; }

    public string? StarterKey { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;

    public string DisplayLabel
    {
        get
        {
            var parts = new List<string> { Name };
            if (!string.IsNullOrWhiteSpace(City))
            {
                parts.Add(City);
            }

            if (!string.IsNullOrWhiteSpace(State))
            {
                parts.Add(State);
            }

            return string.Join(" · ", parts);
        }
    }
}
