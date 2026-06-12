using SaborMercado.Web.Domain;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Domain.Status;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Shopping;

/// <summary>
/// Estado e casos de uso da sessão de compra (F2–F4). Toda mutação:
/// 1) atualiza o estado em memória (recálculo instantâneo);
/// 2) avalia o catálogo de mensagens (avaliador puro);
/// 3) persiste imediatamente (usuário pode fechar o app a qualquer momento).
/// Falha de persistência não interrompe o fluxo da compra (spec, edge case).
/// </summary>
public sealed class ShoppingService(
    IShoppingStore store,
    IPreferencesStore preferences,
    CatalogService catalog,
    StoreService stores,
    ShoppingPatternService patterns,
    TimeProvider clock)
{
    private readonly List<CartItem> _items = [];
    private bool _initialized;
    private TimeSpan? _averageSessionDuration;

    public event Action? StateChanged;

    public ShoppingSession? CurrentSession { get; private set; }

    public ShoppingSession? LastFinishedSession { get; private set; }

    public IReadOnlyList<CartItem> Items => _items;

    public StatusMessage? LastMessage { get; private set; }

    /// <summary>Incrementa a cada nova mensagem (permite re-exibir o banner).</summary>
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
            Evaluate(CartMutation.SessionStarted);
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
                CartItemSource.Catalog,
                recordPriceInCatalog: unitPrice > 0m);

            _items.Add(item);
            await PersistItemAsync(item);
        }
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

    public async Task UpdateItemAsync(Guid itemId, ProductSnapshot snapshot, decimal unitPrice, int quantity)
    {
        RequireActiveSession();
        ValidateItem(snapshot, unitPrice, quantity);

        var item = RequireItem(itemId);
        var session = RequireActiveSession();
        var product = await catalog.EnsureProductAsync(snapshot, session.StoreId);
        item.ProductSnapshot = snapshot;
        item.ProductId = product.Id;
        item.UnitPrice = unitPrice;
        item.Quantity = quantity;

        await catalog.TouchPriceFromPurchaseAsync(
            product.Id,
            unitPrice,
            session.StoreId,
            clock.GetUtcNow());

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
        NotifyStateChanged();
    }

    public async Task AbandonSessionAsync()
    {
        var session = RequireActiveSession();

        // Sem mensagem: o catálogo não define código para abandono.
        session.Status = SessionStatus.Abandoned;
        session.FinishedAt = clock.GetUtcNow();

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
            // Preferência é leve; falha não afeta o fluxo.
        }
    }

    private async Task<CartItem> CreateCartItemAsync(
        ShoppingSession session,
        ProductSnapshot snapshot,
        decimal unitPrice,
        int quantity,
        CartItemSource source,
        bool recordPriceInCatalog = true)
    {
        var product = await catalog.EnsureProductAsync(snapshot, session.StoreId);
        if (recordPriceInCatalog)
        {
            await catalog.TouchPriceFromPurchaseAsync(
                product.Id,
                unitPrice,
                session.StoreId,
                clock.GetUtcNow());
        }

        return new CartItem
        {
            Id = Ids.NewId(),
            SessionId = session.Id,
            ProductSnapshot = snapshot,
            ProductId = product.Id,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Source = source,
            AddedAt = clock.GetUtcNow(),
        };
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
