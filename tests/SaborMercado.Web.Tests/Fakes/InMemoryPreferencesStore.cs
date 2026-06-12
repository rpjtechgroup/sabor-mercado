using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Tests.Fakes;

public sealed class InMemoryPreferencesStore : IPreferencesStore
{
    public decimal? BudgetDefault { get; set; }

    public Task<decimal?> GetBudgetDefaultAsync() => Task.FromResult(BudgetDefault);

    public Task SetBudgetDefaultAsync(decimal? value)
    {
        BudgetDefault = value;
        return Task.CompletedTask;
    }

    public string? GeminiApiKey { get; set; }

    public Task<string?> GetGeminiApiKeyAsync() => Task.FromResult(GeminiApiKey);

    public Task SetGeminiApiKeyAsync(string? value)
    {
        GeminiApiKey = value;
        return Task.CompletedTask;
    }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTimeOffset? AccessTokenExpiresAt { get; set; }

    public string? AccountEmail { get; set; }

    public Guid? PseudonymId { get; set; }

    public Task<string?> GetAccessTokenAsync() => Task.FromResult(AccessToken);

    public Task<DateTimeOffset?> GetAccessTokenExpiresAtAsync() => Task.FromResult(AccessTokenExpiresAt);

    public Task<string?> GetAccountEmailAsync() => Task.FromResult(AccountEmail);

    public Task<Guid?> GetPseudonymIdAsync() => Task.FromResult(PseudonymId);

    public Task<string?> GetRefreshTokenAsync() => Task.FromResult(RefreshToken);

    public Task SetAuthAsync(
        string accessToken,
        string refreshToken,
        DateTimeOffset expiresAt,
        string email,
        Guid pseudonymId)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpiresAt = expiresAt;
        AccountEmail = email;
        PseudonymId = pseudonymId;
        return Task.CompletedTask;
    }

    public Task ClearAuthAsync()
    {
        AccessToken = null;
        RefreshToken = null;
        AccessTokenExpiresAt = null;
        AccountEmail = null;
        PseudonymId = null;
        return Task.CompletedTask;
    }

    public bool ShowIconLabels { get; set; } = true;

    public Task<bool> GetShowIconLabelsAsync() => Task.FromResult(ShowIconLabels);

    public Task SetShowIconLabelsAsync(bool value)
    {
        ShowIconLabels = value;
        return Task.CompletedTask;
    }
}
