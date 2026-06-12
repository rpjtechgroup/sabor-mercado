namespace SaborMercado.Web.Storage;

/// <summary>
/// Preferências leves (localStorage). Proibido dado de domínio aqui
/// (docs/standards/data-standards.md).
/// </summary>
public interface IPreferencesStore
{
    Task<decimal?> GetBudgetDefaultAsync();

    Task SetBudgetDefaultAsync(decimal? value);

    /// <summary>Chave Gemini do usuário (localStorage). Opcional — OCR só funciona com chave.</summary>
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
}
