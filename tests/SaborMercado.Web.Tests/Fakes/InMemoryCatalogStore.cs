using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryCatalogStore : ICatalogStore
{
    public Dictionary<Guid, Product> Products { get; } = [];

    public Dictionary<Guid, PriceRecord> PriceRecords { get; } = [];

    public Task<List<Product>> GetAllProductsAsync() =>
        Task.FromResult(Products.Values.ToList());

    public Task SaveProductAsync(Product product)
    {
        Products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task DeleteProductAsync(Guid productId)
    {
        Products.Remove(productId);
        return Task.CompletedTask;
    }

    public Task<List<PriceRecord>> GetPriceRecordsAsync(Guid productId) =>
        Task.FromResult(PriceRecords.Values.Where(r => r.ProductId == productId).ToList());

    public Task SavePriceRecordAsync(PriceRecord record)
    {
        PriceRecords[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task DeletePriceRecordsAsync(Guid productId)
    {
        foreach (var key in PriceRecords.Where(kv => kv.Value.ProductId == productId).Select(kv => kv.Key).ToList())
        {
            PriceRecords.Remove(key);
        }

        return Task.CompletedTask;
    }
}
