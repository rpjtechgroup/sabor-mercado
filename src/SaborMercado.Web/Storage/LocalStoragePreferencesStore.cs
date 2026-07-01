using System.Globalization;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class LocalStoragePreferencesStore(LocalStorageInterop localStorage) : IPreferencesStore
{
    private const string BudgetDefaultKey = "saborMercado.preferences.budgetDefault";
    private const string GeminiApiKeyKey = "saborMercado.preferences.geminiApiKey";
    private const string AccessTokenKey = "saborMercado.preferences.accessToken";
    private const string RefreshTokenKey = "saborMercado.preferences.refreshToken";
    private const string AccessTokenExpiresKey = "saborMercado.preferences.accessTokenExpires";
    private const string AccountEmailKey = "saborMercado.preferences.accountEmail";
    private const string PseudonymIdKey = "saborMercado.preferences.pseudonymId";
    private const string ShowIconLabelsKey = "saborMercado.preferences.showIconLabels";
    private const string ComparatorColumnOrderKey = "saborMercado.preferences.comparatorColumnOrder";

    public async Task<decimal?> GetBudgetDefaultAsync()
    {
        var raw = await localStorage.GetItemAsync(BudgetDefaultKey);
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    public async Task SetBudgetDefaultAsync(decimal? value)
    {
        if (value is null)
        {
            await localStorage.RemoveItemAsync(BudgetDefaultKey);
            return;
        }

        await localStorage.SetItemAsync(
            BudgetDefaultKey,
            value.Value.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<string?> GetGeminiApiKeyAsync() =>
        await localStorage.GetItemAsync(GeminiApiKeyKey);

    public async Task SetGeminiApiKeyAsync(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            await localStorage.RemoveItemAsync(GeminiApiKeyKey);
            return;
        }

        await localStorage.SetItemAsync(GeminiApiKeyKey, value.Trim());
    }

    public async Task<string?> GetAccessTokenAsync() =>
        await localStorage.GetItemAsync(AccessTokenKey);

    public async Task<DateTimeOffset?> GetAccessTokenExpiresAtAsync()
    {
        var raw = await localStorage.GetItemAsync(AccessTokenExpiresKey);
        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value)
            ? value
            : null;
    }

    public async Task<string?> GetAccountEmailAsync() =>
        await localStorage.GetItemAsync(AccountEmailKey);

    public async Task<Guid?> GetPseudonymIdAsync()
    {
        var raw = await localStorage.GetItemAsync(PseudonymIdKey);
        return Guid.TryParse(raw, out var value) ? value : null;
    }

    public async Task<string?> GetRefreshTokenAsync() =>
        await localStorage.GetItemAsync(RefreshTokenKey);

    public async Task SetAuthAsync(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        string email,
        Guid pseudonymId)
    {
        await localStorage.SetItemAsync(AccessTokenKey, accessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, refreshToken);
        await localStorage.SetItemAsync(AccessTokenExpiresKey, expiresAt.ToString("O", CultureInfo.InvariantCulture));
        await localStorage.SetItemAsync(AccountEmailKey, email);
        await localStorage.SetItemAsync(PseudonymIdKey, pseudonymId.ToString());
    }

    public async Task ClearAuthAsync()
    {
        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        await localStorage.RemoveItemAsync(AccessTokenExpiresKey);
        await localStorage.RemoveItemAsync(AccountEmailKey);
        await localStorage.RemoveItemAsync(PseudonymIdKey);
    }

    public async Task<bool> GetShowIconLabelsAsync()
    {
        var raw = await localStorage.GetItemAsync(ShowIconLabelsKey);
        if (string.IsNullOrEmpty(raw))
        {
            return true;
        }

        return string.Equals(raw, "1", StringComparison.Ordinal);
    }

    public async Task SetShowIconLabelsAsync(bool value) =>
        await localStorage.SetItemAsync(ShowIconLabelsKey, value ? "1" : "0");

    public async Task<IReadOnlyList<ComparatorColumnId>> GetComparatorColumnOrderAsync()
    {
        var raw = await localStorage.GetItemAsync(ComparatorColumnOrderKey);
        return ComparatorColumnOrder.TryParse(raw, out var order)
            ? order
            : ComparatorColumnOrder.DefaultOrder;
    }

    public async Task SetComparatorColumnOrderAsync(IReadOnlyList<ComparatorColumnId> order)
    {
        var normalized = ComparatorColumnOrder.Normalize(order);
        await localStorage.SetItemAsync(
            ComparatorColumnOrderKey,
            ComparatorColumnOrder.ToStorageString(normalized));
    }
}
