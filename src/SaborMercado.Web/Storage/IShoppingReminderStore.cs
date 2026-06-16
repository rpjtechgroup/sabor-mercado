using SaborMercado.Web.Domain.Shopping;

namespace SaborMercado.Web.Storage;

public interface IShoppingReminderStore
{
    Task<List<ShoppingReminder>> GetAllAsync();

    Task SaveAsync(ShoppingReminder reminder);

    Task DeleteAsync(Guid reminderId);

    Task ClearAsync();
}
