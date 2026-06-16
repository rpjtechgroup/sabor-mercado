using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class IndexedDbShoppingReminderStore(IndexedDbInterop indexedDb) : IShoppingReminderStore
{
    public Task<List<ShoppingReminder>> GetAllAsync() =>
        indexedDb.GetAllAsync<ShoppingReminder>(StorageSchema.ShoppingRemindersStore).AsTask();

    public Task SaveAsync(ShoppingReminder reminder) =>
        indexedDb.PutAsync(StorageSchema.ShoppingRemindersStore, reminder).AsTask();

    public Task DeleteAsync(Guid reminderId) =>
        indexedDb.DeleteAsync(StorageSchema.ShoppingRemindersStore, reminderId).AsTask();

    public Task ClearAsync() =>
        indexedDb.ClearAsync(StorageSchema.ShoppingRemindersStore).AsTask();
}
