using SaborMercado.Shared.StarterCatalog;

namespace SaborMercado.Modules.Rewards.Tests;

public sealed class StarterCatalogTests
{
    private static readonly HashSet<string> ValidUnits = ["g", "kg", "ml", "l", "un"];

    [Fact]
    public void GetCatalog_HasAtLeast300Products()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();

        Assert.True(catalog.Products.Count >= 300,
            $"Expected at least 300 products, got {catalog.Products.Count}");
    }

    [Fact]
    public void GetCatalog_HasAtLeast50Stores()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();

        Assert.True(catalog.Stores.Count >= 50,
            $"Expected at least 50 stores, got {catalog.Stores.Count}");
    }

    [Fact]
    public void GetCatalog_AllStoreKeysAreUnique()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();
        var keys = catalog.Stores.Select(s => s.Key).ToList();
        var duplicates = keys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void GetCatalog_AllStoreKeysAreKebabCase()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();

        var invalid = catalog.Stores
            .Where(s => !System.Text.RegularExpressions.Regex.IsMatch(s.Key, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
            .Select(s => s.Key)
            .ToList();

        Assert.Empty(invalid);
    }

    [Fact]
    public void GetCatalog_HasVersion2OrHigher()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();

        Assert.True(catalog.Version >= 2);
    }

    [Fact]
    public void GetCatalog_AllProductKeysAreUnique()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();
        var keys = catalog.Products.Select(p => p.Key).ToList();
        var duplicates = keys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void GetCatalog_AllDefaultStoreKeysExist()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();
        var storeKeys = catalog.Stores.Select(s => s.Key).ToHashSet(StringComparer.Ordinal);

        var invalid = catalog.Products
            .Where(p => !storeKeys.Contains(p.DefaultStoreKey))
            .Select(p => p.Key)
            .ToList();

        Assert.Empty(invalid);
    }

    [Fact]
    public void GetCatalog_AllQuantityUnitsAreValid()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();

        var invalid = catalog.Products
            .Where(p => p.QuantityUnit is not null && !ValidUnits.Contains(p.QuantityUnit))
            .Select(p => $"{p.Key}:{p.QuantityUnit}")
            .ToList();

        Assert.Empty(invalid);
    }

    [Fact]
    public void GetCatalog_HasEightDepartments()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();
        const string separator = " > ";

        var departments = catalog.Products
            .Where(p => !string.IsNullOrWhiteSpace(p.Category))
            .Select(p => p.Category!.Split(separator)[0])
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.True(departments.Count >= 8,
            $"Expected at least 8 departments, got {departments.Count}: {string.Join(", ", departments)}");
    }

    [Fact]
    public void GetCatalog_CategoriesAreHierarchical()
    {
        var catalog = new StarterCatalogProvider().GetCatalog();
        const string separator = " > ";

        var flat = catalog.Products
            .Where(p => !string.IsNullOrWhiteSpace(p.Category))
            .Where(p => p.Category!.Split(separator).Length != 3)
            .Select(p => p.Key)
            .ToList();

        Assert.Empty(flat);
    }
}
