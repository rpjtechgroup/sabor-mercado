using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Shopping;

public sealed class ShoppingReminderService(
    IShoppingReminderStore store,
    CatalogService catalog,
    TimeProvider clock)
{
    public event Action? StateChanged;

    public async Task<IReadOnlyList<ShoppingReminder>> GetAllAsync()
    {
        await PurgeLegacyRemindersAsync();
        var reminders = await store.GetAllAsync();
        return reminders
            .OrderBy(r => r.CreatedAt)
            .ToList();
    }

    public async Task<int> GetCountAsync()
    {
        await PurgeLegacyRemindersAsync();
        return (await store.GetAllAsync()).Count;
    }

    public async Task AddFromProductAsync(Guid productId, int quantity = 1)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        await catalog.InitializeAsync();
        var product = catalog.GetProduct(productId)
            ?? throw new InvalidOperationException("Produto não encontrado no catálogo.");

        var reminders = await store.GetAllAsync();
        var existing = reminders.FirstOrDefault(r => r.ProductId == productId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            await store.SaveAsync(existing);
            NotifyStateChanged();
            return;
        }

        var reminder = new ShoppingReminder
        {
            ProductId = productId,
            DisplayName = product.Name,
            Quantity = quantity,
            CreatedAt = clock.GetUtcNow(),
        };

        await store.SaveAsync(reminder);
        NotifyStateChanged();
    }

    public async Task UpdateQuantityAsync(Guid reminderId, int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        var reminders = await store.GetAllAsync();
        var reminder = reminders.FirstOrDefault(r => r.Id == reminderId)
            ?? throw new InvalidOperationException("Lembrete não encontrado.");

        reminder.Quantity = quantity;
        await store.SaveAsync(reminder);
        NotifyStateChanged();
    }

    public async Task RemoveAsync(Guid reminderId)
    {
        await store.DeleteAsync(reminderId);
        NotifyStateChanged();
    }

    public async Task<IReadOnlyList<ShoppingReminder>> ConsumeAllAsync()
    {
        await PurgeLegacyRemindersAsync();
        var reminders = await store.GetAllAsync();
        if (reminders.Count == 0)
        {
            return [];
        }

        await store.ClearAsync();
        NotifyStateChanged();
        return reminders.OrderBy(r => r.CreatedAt).ToList();
    }

    public async Task RestoreAllAsync(IEnumerable<ShoppingReminder> reminders)
    {
        foreach (var reminder in reminders)
        {
            await store.SaveAsync(reminder);
        }

        NotifyStateChanged();
    }

    public async Task<IReadOnlyList<ReminderLineView>> GetLinesAsync()
    {
        await catalog.InitializeAsync();
        var lines = new List<ReminderLineView>();

        foreach (var reminder in await GetAllAsync())
        {
            if (reminder.ProductId is not { } productId)
            {
                continue;
            }

            var product = catalog.GetProduct(productId);
            if (product is null)
            {
                continue;
            }

            var lastPrice = await catalog.GetLastKnownPriceAsync(productId);
            lines.Add(new ReminderLineView(reminder, product, lastPrice?.Price));
        }

        return lines;
    }

    private async Task PurgeLegacyRemindersAsync()
    {
        var reminders = await store.GetAllAsync();
        foreach (var reminder in reminders.Where(r => r.ProductId is null))
        {
            await store.DeleteAsync(reminder.Id);
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}

public sealed record ReminderLineView(ShoppingReminder Reminder, Product? Product, decimal? LastPrice);
