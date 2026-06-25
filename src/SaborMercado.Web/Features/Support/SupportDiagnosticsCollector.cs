using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using SaborMercado.Web.Features.Account;
using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;
using SaborMercado.Web.Interop;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Support;

public sealed class SupportDiagnosticsCollector(
    NavigationManager navigation,
    AccountService account,
    ShoppingService shopping,
    ICatalogStore catalogStore,
    IStoreStore storeStore,
    ShoppingReminderService reminders,
    ShareService share,
    ISupportDiagnosticsInterop clientEnv)
{
    public async Task<JsonElement> CollectAsync()
    {
        var env = await clientEnv.GetClientEnvironmentAsync();
        var products = await catalogStore.GetAllProductsAsync();
        var storeList = await storeStore.GetAllStoresAsync();
        var reminderCount = await reminders.GetCountAsync();
        var pendingShares = await share.GetPendingCountAsync();

        var session = shopping.CurrentSession;
        var snapshot = new Dictionary<string, object?>
        {
            ["collectedAtUtc"] = DateTimeOffset.UtcNow.ToString("O"),
            ["appVersion"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
            ["route"] = navigation.Uri,
            ["client"] = new Dictionary<string, object?>
            {
                ["userAgent"] = env.UserAgent,
                ["language"] = env.Language,
                ["viewportWidth"] = env.ViewportWidth,
                ["viewportHeight"] = env.ViewportHeight,
                ["online"] = env.Online,
                ["serviceWorkerSupported"] = env.ServiceWorkerSupported,
                ["serviceWorkerActive"] = env.ServiceWorkerActive,
            },
            ["account"] = new Dictionary<string, object?>
            {
                ["isLoggedIn"] = account.IsLoggedIn,
                ["email"] = account.Email,
                ["pseudonymId"] = account.PseudonymId?.ToString(),
                ["pendingShareCount"] = pendingShares,
            },
            ["shopping"] = new Dictionary<string, object?>
            {
                ["hasActiveSession"] = session is not null,
                ["sessionKind"] = session?.Kind.ToString(),
                ["sessionStatus"] = session?.Status.ToString(),
                ["budgetAmount"] = session?.BudgetAmount,
                ["cartItemCount"] = shopping.Items.Count,
                ["cartTotal"] = shopping.Total,
                ["percentUsed"] = shopping.PercentUsed,
                ["lastAlertCode"] = shopping.LastMessage?.Code,
                ["storageUnavailable"] = shopping.StorageUnavailable,
            },
            ["catalog"] = new Dictionary<string, object?>
            {
                ["productCount"] = products.Count,
                ["storeCount"] = storeList.Count,
            },
            ["reminders"] = new Dictionary<string, object?>
            {
                ["pendingCount"] = reminderCount,
            },
            ["storage"] = new Dictionary<string, object?>
            {
                ["indexedDbVersion"] = StorageSchema.DatabaseVersion,
                ["schemaVersion"] = StorageSchema.CurrentSchemaVersion,
            },
        };

        return JsonSerializer.SerializeToElement(snapshot);
    }
}
