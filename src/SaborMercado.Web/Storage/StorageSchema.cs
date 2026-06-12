namespace SaborMercado.Web.Storage;

public static class StorageSchema
{
    public const string DatabaseName = "sabor-mercado";

    public const int DatabaseVersion = 4;

    
    public const int CurrentSchemaVersion = 1;

    public const string ShoppingSessionsStore = "shoppingSessions";
    public const string CartItemsStore = "cartItems";
    public const string ProductsStore = "products";
    public const string PriceRecordsStore = "priceRecords";

    public const string PendingSharesStore = "pendingShares";

    public const string ShoppingPatternsStore = "shoppingPatterns";

    public const string StoresStore = "stores";

    public static readonly Guid DefaultPatternId = Guid.Parse("00000000-0000-4000-8000-000000000001");
}
