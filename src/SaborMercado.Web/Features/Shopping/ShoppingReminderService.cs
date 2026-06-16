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
        var reminders = await store.GetAllAsync();
        return reminders
            .OrderBy(r => r.CreatedAt)
            .ToList();
    }

    public async Task<int> GetCountAsync() => (await store.GetAllAsync()).Count;

    public async Task AddFromProductAsync(Guid productId, int quantity = 1, string? note = null)
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
            if (!string.IsNullOrWhiteSpace(note))
            {
                existing.Note = note.Trim();
            }

            await store.SaveAsync(existing);
            NotifyStateChanged();
            return;
        }

        var reminder = new ShoppingReminder
        {
            ProductId = productId,
            DisplayName = product.Name,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Quantity = quantity,
            CreatedAt = clock.GetUtcNow(),
        };

        await store.SaveAsync(reminder);
        NotifyStateChanged();
    }

    public async Task AddFromNoteAsync(string displayName, int quantity = 1, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Informe o nome do item.", nameof(displayName));
        }

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        var normalized = displayName.Trim();
        var reminders = await store.GetAllAsync();
        var existing = reminders.FirstOrDefault(r =>
            r.ProductId is null &&
            string.Equals(r.DisplayName, normalized, StringComparison.CurrentCultureIgnoreCase));

        if (existing is not null)
        {
            existing.Quantity += quantity;
            if (!string.IsNullOrWhiteSpace(note))
            {
                existing.Note = note.Trim();
            }

            await store.SaveAsync(existing);
            NotifyStateChanged();
            return;
        }

        var reminder = new ShoppingReminder
        {
            DisplayName = normalized,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
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
            if (reminder.ProductId is { } productId)
            {
                var product = catalog.GetProduct(productId);
                if (product is null)
                {
                    continue;
                }

                var lastPrice = await catalog.GetLastKnownPriceAsync(productId);
                lines.Add(new ReminderLineView(reminder, product, lastPrice?.Price));
                continue;
            }

            lines.Add(new ReminderLineView(reminder, null, null));
        }

        return lines;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}

public sealed record ReminderLineView(ShoppingReminder Reminder, Product? Product, decimal? LastPrice);
