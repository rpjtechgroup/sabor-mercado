namespace SaborMercado.Shared.StarterCatalog;

public sealed class StarterCatalogDto
{
    public int Version { get; set; }

    public string Locale { get; set; } = "pt-BR";

    public List<StarterStoreDto> Stores { get; set; } = [];

    public List<StarterProductDto> Products { get; set; } = [];
}

public sealed class StarterStoreDto
{
    public required string Key { get; set; }

    public required string Name { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }
}

public sealed class StarterProductDto
{
    public required string Key { get; set; }

    public required string Name { get; set; }

    public string? Brand { get; set; }

    public string? Category { get; set; }

    public decimal? QuantityValue { get; set; }

    public string? QuantityUnit { get; set; }

    public required string DefaultStoreKey { get; set; }
}
