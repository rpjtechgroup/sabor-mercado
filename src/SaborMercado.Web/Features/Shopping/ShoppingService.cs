using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Domain.Status;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Shared;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Shopping;

public sealed class ShoppingService(
    IShoppingStore store,
    IPreferencesStore preferences,
    CatalogService catalog,
    StoreService stores,
    ShoppingPatternService patterns,
    ShoppingReminderService reminders,
    ToastService toast,
    TimeProvider clock)
{
    private readonly List<CartItem> _items = [];
    private readonly List<ShoppingReminder> _lastConsumedReminders = [];
    private bool _initialized;
    private TimeSpan? _averageSessionDuration;

    public event Action? StateChanged;

    public ShoppingSession? CurrentSession { get; private set; }

    public ShoppingSession? LastFinishedSession { get; private set; }

    public IReadOnlyList<CartItem> Items => _items;

    public StatusMessage? LastMessage { get; private set; }

    
    public int MessageVersion { get; private set; }

    public bool StorageUnavailable { get; private set; }

    public decimal? SuggestedBudget { get; private set; }

    public decimal Total => _items.Sum(i => i.Subtotal);

    public decimal? Remaining => CurrentSession?.BudgetAmount is { } budget ? budget - Total : null;

    public decimal PercentUsed =>
        CurrentSession?.BudgetAmount is { } budget && budget > 0m ? Total / budget * 100m : 0m;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            var sessions = await store.GetAllSessionsAsync();
            CurrentSession = sessions.FirstOrDefault(s => s.Status == SessionStatus.Active);

            var finished = sessions
                .Where(s => s.Status == SessionStatus.Finished && s.FinishedAt is not null)
                .ToList();
            if (finished.Count > 0)
            {
                var averageTicks = finished.Average(s => (s.FinishedAt!.Value - s.StartedAt).Ticks);
                _averageSessionDuration = TimeSpan.FromTicks((long)averageTicks);
            }

            if (CurrentSession is not null)
            {
                await ResolveSessionStoreIdAsync(CurrentSession);
                var items = await store.GetItemsAsync(CurrentSession.Id);
                _items.Clear();
                _items.AddRange(items.OrderBy(i => i.AddedAt));
            }

            SuggestedBudget = await preferences.GetBudgetDefaultAsync();
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        _initialized = true;
        NotifyStateChanged();
    }

    public Task StartSessionAsync(decimal? budgetAmount, Guid? storeId) =>
        StartSessionAsync(SessionKind.Sporadic, budgetAmount, storeId);

    public async Task StartSessionAsync(SessionKind kind, decimal? budgetAmount, Guid? storeId, Guid? patternId = null)
    {
        if (CurrentSession is not null)
        {
            throw new InvalidOperationException(
                "Já existe uma sessão ativa; encerre ou abandone antes de iniciar outra.");
        }

        if (kind == SessionKind.Sporadic && budgetAmount is < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(budgetAmount));
        }

        await stores.InitializeAsync();
        string? marketName = null;
        if (storeId is { } id && id != Guid.Empty)
        {
            stores.RequireStore(id);
            marketName = stores.GetStoreName(id);
        }

        var session = new ShoppingSession
        {
            Id = Ids.NewId(),
            StoreId = storeId is { } store && store != Guid.Empty ? store : null,
            MarketName = marketName,
            BudgetAmount = kind == SessionKind.Sporadic && budgetAmount is > 0m ? budgetAmount : null,
            Kind = kind,
            PatternId = kind == SessionKind.Monthly ? patternId ?? StorageSchema.DefaultPatternId : null,
            StartedAt = clock.GetUtcNow(),
            Status = SessionStatus.Active,
        };

        CurrentSession = session;
        LastFinishedSession = null;
        _items.Clear();

        if (kind == SessionKind.Monthly)
        {
            await PrefillFromMonthlyPatternAsync(session);
        }
        else
        {
            var added = await PrefillFromRemindersAsync(session);
            Evaluate(CartMutation.SessionStarted);
            if (added > 0)
            {
                toast.Show(
                    $"{added} item(ns) dos seus lembretes foram adicionados ao carrinho.",
                    ToastSeverity.Success);
            }
        }

        await PersistSessionAsync();

        if (session.BudgetAmount is { } budget)
        {
            SuggestedBudget = budget;
            await TryPersistPreferenceAsync(budget);
        }

        NotifyStateChanged();
    }

    private async Task PrefillFromMonthlyPatternAsync(ShoppingSession session)
    {
        await catalog.InitializeAsync();
        var pattern = await patterns.GetOrCreateAsync();

        foreach (var patternItem in pattern.Items)
        {
            var product = catalog.GetProduct(patternItem.ProductId);
            if (product is null)
            {
                continue;
            }

            var snapshot = ProductSnapshot.FromProduct(product);
            var lastPrice = await catalog.GetLastKnownPriceAsync(product.Id);
            var unitPrice = lastPrice?.Price ?? 0m;
            var item = await CreateCartItemAsync(
                session,
                snapshot,
                unitPrice,
                patternItem.DefaultQuantity,
                CartItemSource.Catalog);

            _items.Add(item);
            await PersistItemAsync(item);
        }
    }

    private async Task<int> PrefillFromRemindersAsync(ShoppingSession session)
    {
        await catalog.InitializeAsync();
        var consumed = await reminders.ConsumeAllAsync();
        _lastConsumedReminders.Clear();
        _lastConsumedReminders.AddRange(consumed);

        var added = 0;
        foreach (var reminder in consumed)
        {
            ProductSnapshot snapshot;
            decimal unitPrice;
            CartItemSource source;

            if (reminder.ProductId is { } productId)
            {
                var product = catalog.GetProduct(productId);
                if (product is null)
                {
                    continue;
                }

                snapshot = ProductSnapshot.FromProduct(product);
                var lastPrice = await catalog.GetLastKnownPriceAsync(productId);
                unitPrice = lastPrice?.Price ?? 0m;
                source = CartItemSource.Catalog;
            }
            else
            {
                snapshot = new ProductSnapshot(reminder.DisplayName, null, null, null);
                unitPrice = 0m;
                source = CartItemSource.Manual;
            }

            var item = await CreateCartItemAsync(
                session,
                snapshot,
                unitPrice,
                reminder.Quantity,
                source);

            _items.Add(item);
            await PersistItemAsync(item);
            added++;
        }

        return added;
    }

    public async Task AddItemAsync(
        ProductSnapshot snapshot,
        decimal unitPrice,
        int quantity,
        CartItemSource source)
    {
        var session = RequireActiveSession();
        ValidateItem(snapshot, unitPrice, quantity);

        var item = await CreateCartItemAsync(session, snapshot, unitPrice, quantity, source);

        _items.Add(item);
        Evaluate(CartMutation.ItemAdded);
        await PersistItemAsync(item);
        await PersistSessionAsync();
        NotifyStateChanged();
    }

    public async Task AddItemFromOcrAsync(
        ProductSnapshot snapshot,
        decimal unitPrice,
        int quantity,
        decimal confidence)
    {
        var session = RequireActiveSession();
        ValidateItem(snapshot, unitPrice, quantity);

        var item = await CreateCartItemAsync(session, snapshot, unitPrice, quantity, CartItemSource.Ocr);

        _items.Add(item);
        Evaluate(CartMutation.ItemAddedOcr, confidence, snapshot.Name);
        await PersistItemAsync(item);
        await PersistSessionAsync();
        NotifyStateChanged();
    }

    public void EmitOcrUnavailable()
    {
        LastMessage = StatusMessage.Create(StatusCodes.OcrUnavailable);
        MessageVersion++;
        NotifyStateChanged();
    }

    public async Task SetSessionStoreAsync(Guid? storeId)
    {
        var session = RequireActiveSession();
        await stores.InitializeAsync();

        if (storeId is null || storeId == Guid.Empty)
        {
            session.StoreId = null;
            session.MarketName = null;
        }
        else
        {
            stores.RequireStore(storeId.Value);
            session.StoreId = storeId.Value;
            session.MarketName = stores.GetStoreName(storeId.Value);
        }

        await PersistSessionAsync();
        NotifyStateChanged();
    }

    public async Task UpdateItemAsync(Guid itemId, ProductSnapshot snapshot, decimal unitPrice, int quantity)
    {
        RequireActiveSession();
        ValidateItem(snapshot, unitPrice, quantity);

        var item = RequireItem(itemId);
        var product = await catalog.EnsureProductAsync(snapshot);
        item.ProductSnapshot = snapshot;
        item.ProductId = product.Id;
        item.UnitPrice = unitPrice;
        item.Quantity = quantity;

        Evaluate(CartMutation.ItemUpdated);
        await PersistItemAsync(item);
        await PersistSessionAsync();
        NotifyStateChanged();
    }

    public Task IncrementQuantityAsync(Guid itemId, int delta)
    {
        var item = RequireItem(itemId);
        return SetQuantityAsync(itemId, item.Quantity + delta);
    }

    public async Task SetQuantityAsync(Guid itemId, int quantity)
    {
        RequireActiveSession();
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity), "Quantidade deve ser maior que zero; para zerar, remova o item.");
        }

        var item = RequireItem(itemId);
        item.Quantity = quantity;

        Evaluate(CartMutation.ItemUpdated);
        await PersistItemAsync(item);
        await PersistSessionAsync();
        NotifyStateChanged();
    }

    public async Task RemoveItemAsync(Guid itemId)
    {
        RequireActiveSession();
        var item = RequireItem(itemId);
        _items.Remove(item);

        Evaluate(CartMutation.ItemRemoved);
        try
        {
            await store.DeleteItemAsync(item.Id);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }

        await PersistSessionAsync();
        NotifyStateChanged();
    }

    public async Task FinishSessionAsync()
    {
        var session = RequireActiveSession();

        Evaluate(CartMutation.SessionFinished);
        session.Status = SessionStatus.Finished;
        session.FinishedAt = clock.GetUtcNow();

        await PersistSessionAsync();
        LastFinishedSession = session;
        CurrentSession = null;
        _items.Clear();
        _lastConsumedReminders.Clear();
        NotifyStateChanged();
    }

    public async Task AbandonSessionAsync()
    {
        var session = RequireActiveSession();

        
        session.Status = SessionStatus.Abandoned;
        session.FinishedAt = clock.GetUtcNow();

        if (_lastConsumedReminders.Count > 0)
        {
            await reminders.RestoreAllAsync(_lastConsumedReminders);
            _lastConsumedReminders.Clear();
        }

        await PersistSessionAsync();
        CurrentSession = null;
        _items.Clear();
        NotifyStateChanged();
    }

    public void DismissMessage()
    {
        LastMessage = null;
        NotifyStateChanged();
    }

    private void Evaluate(CartMutation mutation, decimal? ocrConfidence = null, string? ocrProductName = null)
    {
        var session = CurrentSession!;
        var snapshot = new CartSnapshot(
            Total,
            _items.Count,
            session.BudgetAmount,
            session.StartedAt,
            PlannedListSize: null,
            AverageSessionDuration: _averageSessionDuration);

        var result = StatusMessageEvaluator.Evaluate(
            new EvaluationInput(
                session.AlertState,
                snapshot,
                mutation,
                clock.GetUtcNow(),
                ocrConfidence,
                ocrProductName));

        session.AlertState = result.After;
        if (result.Message is not null)
        {
            LastMessage = result.Message;
            MessageVersion++;
        }
    }

    private ShoppingSession RequireActiveSession() =>
        CurrentSession ?? throw new InvalidOperationException("Nenhuma sessão de compra ativa.");

    private CartItem RequireItem(Guid itemId) =>
        _items.FirstOrDefault(i => i.Id == itemId)
        ?? throw new InvalidOperationException("Item não encontrado no carrinho.");

    private static void ValidateItem(ProductSnapshot snapshot, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(snapshot.Name))
        {
            throw new ArgumentException("Nome do produto é obrigatório.", nameof(snapshot));
        }

        if (unitPrice < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Preço não pode ser negativo.");
        }

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantidade deve ser maior que zero.");
        }
    }

    private async Task PersistSessionAsync()
    {
        if (CurrentSession is null)
        {
            return;
        }

        try
        {
            await store.SaveSessionAsync(CurrentSession);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }
    }

    private async Task PersistItemAsync(CartItem item)
    {
        try
        {
            await store.SaveItemAsync(item);
            StorageUnavailable = false;
        }
        catch
        {
            StorageUnavailable = true;
        }
    }

    private async Task TryPersistPreferenceAsync(decimal budget)
    {
        try
        {
            await preferences.SetBudgetDefaultAsync(budget);
        }
        catch
        {
            
        }
    }

    private async Task<Guid?> ResolveSessionStoreIdAsync(ShoppingSession session)
    {
        if (session.StoreId is { } existing && existing != Guid.Empty)
        {
            return existing;
        }

        await stores.InitializeAsync();

        if (string.IsNullOrWhiteSpace(session.MarketName))
        {
            return null;
        }

        var matched = stores.Stores.FirstOrDefault(store =>
            string.Equals(store.Name, session.MarketName, StringComparison.OrdinalIgnoreCase));

        if (matched is null)
        {
            return null;
        }

        session.StoreId = matched.Id;
        await PersistSessionAsync();
        return matched.Id;
    }

    private async Task<CartItem> CreateCartItemAsync(
        ShoppingSession session,
        ProductSnapshot snapshot,
        decimal unitPrice,
        int quantity,
        CartItemSource source)
    {
        var (storeId, storeName) = await ResolveStoreStampAsync(session);
        var product = await catalog.EnsureProductAsync(snapshot);

        return new CartItem
        {
            Id = Ids.NewId(),
            SessionId = session.Id,
            ProductSnapshot = snapshot,
            ProductId = product.Id,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Source = source,
            StoreId = storeId,
            StoreName = storeName,
            AddedAt = clock.GetUtcNow(),
        };
    }

    private async Task<(Guid? StoreId, string? StoreName)> ResolveStoreStampAsync(ShoppingSession session)
    {
        var storeId = await ResolveSessionStoreIdAsync(session);
        if (storeId is null || storeId == Guid.Empty)
        {
            return (null, null);
        }

        await stores.InitializeAsync();
        return (storeId, stores.GetStoreName(storeId));
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
