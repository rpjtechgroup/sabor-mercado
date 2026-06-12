using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Features.Catalog;

public sealed class StoreFormModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public static StoreFormModel FromStore(Store store) => new()
    {
        Id = store.Id,
        Name = store.Name,
        City = store.City,
        State = store.State,
        Address = store.Address,
        Latitude = store.Latitude,
        Longitude = store.Longitude,
    };

    public Store ToStore() => new()
    {
        Id = Id,
        Name = Name.Trim(),
        City = Normalize(City),
        State = Normalize(State),
        Address = Normalize(Address),
        Latitude = Latitude,
        Longitude = Longitude,
    };

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
