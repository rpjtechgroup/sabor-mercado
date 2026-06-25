using Microsoft.AspNetCore.Components;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Account;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Features.Catalog;

public partial class ProductDetail : IDisposable
{
    private bool _loaded;
    private bool _confirmingDelete;
    private bool _addedToCart;
    private List<PriceRecord> _history = [];
    private PriceRecord? _lastPrice;
    private decimal? _newPrice;
    private Guid _newPriceStoreId;
    private ProductFormModel? _editForm;
    private bool _sharing;
    private Guid _shareStoreId;
    private string? _shareMessage;
    private string? _shareError;
    private string? _storeLabel;

    [Parameter]
    public Guid ProductId { get; set; }

    [Inject]
    public CatalogService Catalog { get; set; } = default!;

    [Inject]
    public StoreService StoreService { get; set; } = default!;

    [Inject]
    public ShoppingService Shopping { get; set; } = default!;

    [Inject]
    public AccountService Account { get; set; } = default!;

    [Inject]
    public ShareService Share { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    private Product? Product => Catalog.GetProduct(ProductId);

    private string DetailsLine
    {
        get
        {
            if (Product is null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Product.Brand))
            {
                parts.Add(Product.Brand);
            }

            if (Product.QuantityValue is { } value)
            {
                parts.Add($"{value.ToString("0.##", MoneyFormat.Culture)}{Product.QuantityUnit?.Label()}");
            }

            if (!string.IsNullOrWhiteSpace(Product.Category))
            {
                parts.Add(Product.Category);
            }

            if (!string.IsNullOrWhiteSpace(Product.Ean))
            {
                parts.Add($"EAN {Product.Ean}");
            }

            return string.Join(" · ", parts);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Catalog.StateChanged += OnStateChanged;
        StoreService.StateChanged += OnStateChanged;
        Shopping.StateChanged += OnStateChanged;
        Account.StateChanged += OnStateChanged;
        await StoreService.InitializeAsync();
        await Catalog.InitializeAsync();
        await Shopping.InitializeAsync();
        await Account.InitializeAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await ReloadHistoryAsync();
        UpdateStoreLabel();
        _loaded = true;
    }

    private void UpdateStoreLabel()
    {
        if (Product is null || Product.StoreId == Guid.Empty)
        {
            _storeLabel = null;
            return;
        }

        _storeLabel = StoreService.GetStore(Product.StoreId)?.DisplayLabel;
    }

    private async Task ReloadHistoryAsync()
    {
        _history = await Catalog.GetPriceHistoryAsync(ProductId);
        _lastPrice = _history.FirstOrDefault();

        var defaultStore = Product?.StoreId
            ?? _lastPrice?.StoreId
            ?? Shopping.CurrentSession?.StoreId
            ?? Guid.Empty;

        if (_newPriceStoreId == Guid.Empty && defaultStore != Guid.Empty)
        {
            _newPriceStoreId = defaultStore;
        }

        if (_shareStoreId == Guid.Empty && defaultStore != Guid.Empty)
        {
            _shareStoreId = defaultStore;
        }
    }

    private async Task SharePriceAsync()
    {
        if (Product is null || _lastPrice is not { } priceRecord || _shareStoreId == Guid.Empty)
        {
            return;
        }

        var store = StoreService.GetStore(_shareStoreId);
        if (store is null)
        {
            return;
        }

        _sharing = true;
        _shareError = null;
        _shareMessage = null;
        try
        {
            var observedOn = DateOnly.FromDateTime(priceRecord.ObservedAt.UtcDateTime);
            var result = await Share.SharePriceAsync(
                Product,
                priceRecord.Price,
                store.Name,
                marketCity: store.City,
                marketState: store.State,
                observedOn);

            if (result.IsQueued)
            {
                _shareMessage = "Sem conexão com o servidor. Preço enfileirado e será enviado quando você estiver online.";
                await Account.FlushPendingSharesAsync();
                return;
            }

            var response = result.Response!;
            _shareMessage = response.IsNewProduct
                ? "Compartilhado! Produto novo no catálogo colaborativo."
                : "Preço compartilhado com sucesso.";
        }
        catch (ShareException ex)
        {
            _shareError = ex.Message;
        }
        finally
        {
            _sharing = false;
        }
    }

    private async Task AddPriceAsync()
    {
        if (_newPrice is not { } price || Product is null || _newPriceStoreId == Guid.Empty)
        {
            return;
        }

        await Catalog.AddPriceRecordAsync(ProductId, price, _newPriceStoreId);
        _newPrice = null;
        await ReloadHistoryAsync();
    }

    private async Task AddToCartAsync()
    {
        if (Product is null || _lastPrice is not { } lastPrice || Shopping.CurrentSession is null)
        {
            return;
        }

        await Shopping.AddItemAsync(
            ProductSnapshot.FromProduct(Product),
            lastPrice.Price,
            quantity: 1,
            CartItemSource.Catalog);

        _addedToCart = true;
    }

    private void OpenEdit()
    {
        if (Product is not null)
        {
            _editForm = ProductFormModel.FromProduct(Product);
        }
    }

    private void AskDelete() => _confirmingDelete = true;

    private async Task SaveEditAsync(ProductFormModel model)
    {
        if (Product is null)
        {
            return;
        }

        model.ApplyTo(Product);
        await Catalog.UpdateProductAsync(Product);
        _editForm = null;
        UpdateStoreLabel();
    }

    private async Task DeleteAsync()
    {
        await Catalog.DeleteProductAsync(ProductId);
        Navigation.NavigateTo("catalogo");
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        UpdateStoreLabel();
        await ReloadHistoryAsync();
        StateHasChanged();
    });

    public void Dispose()
    {
        Catalog.StateChanged -= OnStateChanged;
        StoreService.StateChanged -= OnStateChanged;
        Shopping.StateChanged -= OnStateChanged;
        Account.StateChanged -= OnStateChanged;
    }
}
