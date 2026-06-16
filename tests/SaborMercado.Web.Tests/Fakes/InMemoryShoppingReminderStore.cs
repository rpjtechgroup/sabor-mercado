using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryShoppingReminderStore : IShoppingReminderStore
{
    public Dictionary<Guid, ShoppingReminder> Reminders { get; } = [];

    public Task<List<ShoppingReminder>> GetAllAsync() =>
        Task.FromResult(Reminders.Values.ToList());

    public Task SaveAsync(ShoppingReminder reminder)
    {
        Reminders[reminder.Id] = reminder;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid reminderId)
    {
        Reminders.Remove(reminderId);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        Reminders.Clear();
        return Task.CompletedTask;
    }
}
