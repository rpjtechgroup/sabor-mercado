using SaborMercado.Web.Domain.Shopping;

namespace SaborMercado.Web.Storage;

public interface IShoppingStore
{
    Task<List<ShoppingSession>> GetAllSessionsAsync();

    Task SaveSessionAsync(ShoppingSession session);

    Task<List<CartItem>> GetItemsAsync(Guid sessionId);

    Task SaveItemAsync(CartItem item);

    Task DeleteItemAsync(Guid itemId);
}
