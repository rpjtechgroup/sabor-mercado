using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Tests.Fakes;

/// <summary>Comércios fixos para testes de catálogo e compras.</summary>
public static class CatalogTestStores
{
    public static readonly Guid StoreAId = Guid.Parse("11111111-1111-4111-8111-111111111111");

    public static readonly Guid StoreBId = Guid.Parse("22222222-2222-4222-8222-222222222222");

    public static readonly Guid StoreCId = Guid.Parse("33333333-3333-4333-8333-333333333333");

    public static readonly Guid DefaultId = StoreAId;

    public static void Seed(InMemoryStoreStore store, DateTimeOffset createdAt)
    {
        store.Stores[StoreAId] = new Store { Id = StoreAId, Name = "Mercado A", CreatedAt = createdAt };
        store.Stores[StoreBId] = new Store { Id = StoreBId, Name = "Mercado B", CreatedAt = createdAt };
        store.Stores[StoreCId] = new Store { Id = StoreCId, Name = "Mercado C", CreatedAt = createdAt };
    }
}
