namespace SaborMercado.Web.Storage;

public interface IPreferencesStore
{
    Task<decimal?> GetBudgetDefaultAsync();

    Task SetBudgetDefaultAsync(decimal? value);

    
    Task<string?> GetGeminiApiKeyAsync();

    Task SetGeminiApiKeyAsync(string? value);

    Task<string?> GetAccessTokenAsync();

    Task<DateTimeOffset?> GetAccessTokenExpiresAtAsync();

    Task<string?> GetAccountEmailAsync();

    Task<Guid?> GetPseudonymIdAsync();

    Task<string?> GetRefreshTokenAsync();

    Task SetAuthAsync(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        string email,
        Guid pseudonymId);

    Task ClearAuthAsync();

    
    Task<bool> GetShowIconLabelsAsync();

    Task SetShowIconLabelsAsync(bool value);
}
