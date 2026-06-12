using SaborMercado.Web.Domain.Shopping;

namespace SaborMercado.Web.Storage;

public interface IShoppingPatternStore
{
    Task<ShoppingPattern?> GetAsync(Guid id);

    Task SaveAsync(ShoppingPattern pattern);
}
