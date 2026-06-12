using System.Globalization;

using Microsoft.AspNetCore.Components.Web;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using SaborMercado.Web;

using SaborMercado.Web.Features.Account;
using SaborMercado.Web.Features.Premium;

using SaborMercado.Web.Features.Catalog;

using SaborMercado.Web.Features.Recognition;

using SaborMercado.Web.Features.Shopping;

using SaborMercado.Web.Infrastructure;

using SaborMercado.Web.Interop;

using SaborMercado.Web.Shared;

using SaborMercado.Web.Storage;



CultureInfo.DefaultThreadCurrentCulture = MoneyFormat.Culture;

CultureInfo.DefaultThreadCurrentUICulture = MoneyFormat.Culture;



var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");



builder.Services.AddLocalization();

builder.Services.AddSingleton(TimeProvider.System);



builder.Services.AddScoped<IndexedDbInterop>();

builder.Services.AddScoped<LocalStorageInterop>();

builder.Services.AddScoped<ImageCompressInterop>();

builder.Services.AddScoped<DownloadInterop>();



var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5280";

builder.Services.AddScoped(sp => new SaborMercadoApiClient(
    new HttpClient { BaseAddress = new Uri(apiBaseUrl), Timeout = TimeSpan.FromSeconds(15) },
    sp.GetRequiredService<IPreferencesStore>()));

builder.Services.AddScoped(sp => new GeminiShelfLabelClient(
    new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com/"), Timeout = TimeSpan.FromSeconds(15) },
    sp.GetRequiredService<IConfiguration>()));



builder.Services.AddScoped<RecognitionService>();

builder.Services.AddScoped<IShoppingStore, IndexedDbShoppingStore>();

builder.Services.AddScoped<ICatalogStore, IndexedDbCatalogStore>();

builder.Services.AddScoped<IPreferencesStore, LocalStoragePreferencesStore>();
builder.Services.AddScoped<IPendingShareStore, IndexedDbPendingShareStore>();



builder.Services.AddScoped<StatusMessageLocalizer>();

builder.Services.AddScoped<ToastService>();

builder.Services.AddScoped<CatalogService>();

builder.Services.AddScoped<IShoppingPatternStore, IndexedDbShoppingPatternStore>();

builder.Services.AddScoped<ShoppingPatternService>();

builder.Services.AddScoped<ShoppingService>();

builder.Services.AddScoped<PurchaseHistoryService>();

builder.Services.AddScoped<MarketPriceMatrixService>();

builder.Services.AddScoped<AccountService>();

builder.Services.AddScoped<ShareService>();
builder.Services.AddScoped<CollaborativeCatalogService>();
builder.Services.AddScoped<MarketComparisonClient>();
builder.Services.AddScoped<PremiumStatsService>();
builder.Services.AddScoped<SmartListService>();



await builder.Build().RunAsync();

