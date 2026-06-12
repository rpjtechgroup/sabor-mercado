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
    private string? _newPriceMarket;
    private ProductFormModel? _editForm;
    private bool _sharing;
    private string? _shareMarket;
    private string? _shareMessage;
    private string? _shareError;

    [Parameter]
    public Guid ProductId { get; set; }

    [Inject]
    public CatalogService Catalog { get; set; } = default!;

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
        Shopping.StateChanged += OnStateChanged;
        Account.StateChanged += OnStateChanged;
        await Catalog.InitializeAsync();
        await Shopping.InitializeAsync();
        await Account.InitializeAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await ReloadHistoryAsync();
        _loaded = true;
    }

    private async Task ReloadHistoryAsync()
    {
        _history = await Catalog.GetPriceHistoryAsync(ProductId);
        _lastPrice = _history.FirstOrDefault();
        _shareMarket ??= _lastPrice?.MarketName ?? Shopping.CurrentSession?.MarketName;
    }

    private async Task SharePriceAsync()
    {
        if (Product is null || _lastPrice is not { } priceRecord || string.IsNullOrWhiteSpace(_shareMarket))
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
                _shareMarket.Trim(),
                marketCity: null,
                marketState: null,
                observedOn);

            if (result.IsQueued)
            {
                _shareMessage = "Sem conexão com o servidor. Preço enfileirado e será enviado quando você estiver online.";
                await Account.FlushPendingSharesAsync();
                return;
            }

            var response = result.Response!;
            Account.ApplyCreditsFromShare(response.CreditsGranted);
            _shareMessage = response.IsNewProduct
                ? $"Compartilhado! +{response.CreditsGranted} créditos (produto novo no catálogo colaborativo)."
                : $"Compartilhado! +{response.CreditsGranted} crédito(s).";
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
        if (_newPrice is not { } price || Product is null)
        {
            return;
        }

        await Catalog.AddPriceRecordAsync(ProductId, price, _newPriceMarket);
        _newPrice = null;
        _newPriceMarket = null;
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

    private async Task SaveEditAsync(ProductFormModel model)
    {
        if (Product is null)
        {
            return;
        }

        model.ApplyTo(Product);
        await Catalog.UpdateProductAsync(Product);
        _editForm = null;
    }

    private async Task DeleteAsync()
    {
        await Catalog.DeleteProductAsync(ProductId);
        Navigation.NavigateTo("catalogo");
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Catalog.StateChanged -= OnStateChanged;
        Shopping.StateChanged -= OnStateChanged;
        Account.StateChanged -= OnStateChanged;
    }
}
