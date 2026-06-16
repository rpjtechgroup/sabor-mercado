using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SaborMercado.Shared.StarterCatalog;

public sealed class StarterCatalogProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private StarterCatalogDto? _cached;
    private string? _etag;

    public StarterCatalogDto GetCatalog()
    {
        if (_cached is not null)
        {
            return _cached;
        }

        using var stream = OpenCatalogStream();
        _cached = JsonSerializer.Deserialize<StarterCatalogDto>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Starter catalog JSON is invalid.");

        var json = JsonSerializer.Serialize(_cached, JsonOptions);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        _etag = $"\"v{_cached.Version}-{hash[..16]}\"";

        return _cached;
    }

    public string GetETag() => _etag ??= $"\"v{GetCatalog().Version}\"";

    private static Stream OpenCatalogStream()
    {
        var assembly = typeof(StarterCatalogProvider).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("starter-catalog.pt-BR.json", StringComparison.Ordinal));

        if (resourceName is not null)
        {
            var embedded = assembly.GetManifestResourceStream(resourceName);
            if (embedded is not null)
            {
                return embedded;
            }
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "data", "starter-catalog.pt-BR.json");
        if (File.Exists(filePath))
        {
            return File.OpenRead(filePath);
        }

        throw new InvalidOperationException("Starter catalog JSON was not found.");
    }
}
