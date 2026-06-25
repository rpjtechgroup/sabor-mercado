using Microsoft.AspNetCore.Components;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Account;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Features.Shopping;

public partial class ShoppingPage : IDisposable
{
    private bool _loaded;
    private bool _showSummary;
    private bool _confirmingAbandon;
    private bool _canExportCsv = true;
    private int _reminderCount;
    private CartItemFormModel? _itemForm;
    private Guid? _editingItemId;
    private Guid _sessionStoreId;

    [Inject]
    public ShoppingService Shopping { get; set; } = default!;

    [Inject]
    public AccountService Account { get; set; } = default!;

    [Inject]
    public DownloadInterop Download { get; set; } = default!;

    [Inject]
    public ToastService Toast { get; set; } = default!;

    [Inject]
    public ShoppingReminderService Reminders { get; set; } = default!;

    private string ReminderLinkLabel =>
        _reminderCount > 0 ? $"Lembretes ({_reminderCount})" : "Lembretes";

    protected override async Task OnInitializedAsync()
    {
        Shopping.StateChanged += OnStateChanged;
        Account.StateChanged += OnStateChanged;
        Reminders.StateChanged += OnStateChanged;
        await Shopping.InitializeAsync();
        await Account.InitializeAsync();
        _sessionStoreId = Shopping.CurrentSession?.StoreId ?? Guid.Empty;
        _reminderCount = await Reminders.GetCountAsync();
        _loaded = true;
    }

    private void OnStateChanged()
    {
        _sessionStoreId = Shopping.CurrentSession?.StoreId ?? Guid.Empty;
        _ = RefreshReminderCountAsync();
        InvokeAsync(StateHasChanged);
    }

    private async Task StartSessionAsync(SessionStartRequest input)
    {
        await Shopping.StartSessionAsync(input.Kind, input.Budget, input.StoreId);
        _sessionStoreId = input.StoreId ?? Guid.Empty;
        _showSummary = false;
        _confirmingAbandon = false;
    }

    private async Task OnSessionStoreChangedAsync(Guid storeId)
    {
        _sessionStoreId = storeId;
        await Shopping.SetSessionStoreAsync(storeId == Guid.Empty ? null : storeId);
    }

    private void OpenAddForm()
    {
        _itemForm = new CartItemFormModel();
        _editingItemId = null;
    }

    private void OpenEditForm(CartItem item)
    {
        _itemForm = CartItemFormModel.FromItem(item);
        _editingItemId = item.Id;
    }

    private void CloseItemForm()
    {
        _itemForm = null;
        _editingItemId = null;
    }

    private async Task SaveItemAsync(CartItemFormModel model)
    {
        try
        {
            if (_editingItemId is { } itemId)
            {
                await Shopping.UpdateItemAsync(itemId, model.ToSnapshot(), model.UnitPrice!.Value, model.Quantity);
            }
            else
            {
                await Shopping.AddItemAsync(model.ToSnapshot(), model.UnitPrice!.Value, model.Quantity, CartItemSource.Manual);
            }

            CloseItemForm();
        }
        catch (InvalidOperationException ex)
        {
            Toast.Show(ex.Message, ToastSeverity.Danger);
        }
    }

    private Task IncrementAsync(Guid itemId, int delta) => Shopping.IncrementQuantityAsync(itemId, delta);

    private Task SetQuantityAsync(Guid itemId, int quantity) => Shopping.SetQuantityAsync(itemId, quantity);

    private Task RemoveAsync(Guid itemId) => Shopping.RemoveItemAsync(itemId);

    private async Task FinishAsync()
    {
        await Shopping.FinishSessionAsync();
        _showSummary = true;
        CloseItemForm();
        _confirmingAbandon = false;
    }

    private void AskAbandon() => _confirmingAbandon = true;

    private async Task AbandonAsync()
    {
        await Shopping.AbandonSessionAsync();
        _showSummary = false;
        CloseItemForm();
        _confirmingAbandon = false;
        _reminderCount = await Reminders.GetCountAsync();
    }

    private void StartNew() => _showSummary = false;

    private async Task ExportCsvAsync()
    {
        if (Shopping.CurrentSession is null || Shopping.Items.Count == 0)
        {
            return;
        }

        var csv = CartCsvExporter.Build(
            Shopping.Items,
            Shopping.CurrentSession.MarketName,
            Shopping.Total);

        var fileName = $"compra-{DateTime.Now:yyyyMMdd-HHmm}.csv";
        await Download.DownloadTextAsync(fileName, csv);
    }

    private async Task RefreshReminderCountAsync() =>
        _reminderCount = await Reminders.GetCountAsync();

    public void Dispose()
    {
        Shopping.StateChanged -= OnStateChanged;
        Account.StateChanged -= OnStateChanged;
        Reminders.StateChanged -= OnStateChanged;
    }
}
