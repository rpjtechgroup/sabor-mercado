using System.Net;
using SaborMercado.Web.Contracts.Auth;
using SaborMercado.Web.Contracts.Rewards;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Account;

public sealed class AccountService(
    SaborMercadoApiClient api,
    IPreferencesStore preferences,
    ShareService share,
    TimeProvider clock)
{
    public event Action? StateChanged;

    public bool IsLoggedIn { get; private set; }

    public string? Email { get; private set; }

    public Guid? PseudonymId { get; private set; }

    public int CreditBalance { get; private set; }

    public int PendingShareCount { get; private set; }

    public IReadOnlyList<FeatureUnlockDto> ActiveUnlocks { get; private set; } = [];

    public async Task InitializeAsync()
    {
        var token = await preferences.GetAccessTokenAsync();
        IsLoggedIn = !string.IsNullOrWhiteSpace(token);
        if (!IsLoggedIn)
        {
            ResetState();
            return;
        }

        var expiresAt = await preferences.GetAccessTokenExpiresAtAsync();
        if (expiresAt is not null && expiresAt <= clock.GetUtcNow())
        {
            var refreshed = await TryRefreshSessionAsync();
            if (!refreshed)
            {
                ResetState();
                return;
            }
        }

        await RefreshProfileAsync();
        await RefreshCreditsAsync();
        await FlushPendingSharesAsync();
        PendingShareCount = await share.GetPendingCountAsync();
        Notify();
    }

    public bool HasUnlock(string featureCode) =>
        ActiveUnlocks.Any(u =>
            u.FeatureCode.Equals(featureCode, StringComparison.Ordinal) &&
            (u.ExpiresAt is null || u.ExpiresAt > DateTimeOffset.UtcNow));

    public async Task RegisterAsync(string email, string password)
    {
        var response = await api.SendAsync(HttpMethod.Post, "/api/v1/auth/register", new RegisterRequest(email, password));
        await EnsureSuccessAsync(response);
        var auth = await api.ReadJsonAsync<AuthResponse>(response, CancellationToken.None)
            ?? throw new InvalidOperationException("Resposta de autenticação inválida.");
        await StoreSessionAsync(email, auth);
        await FlushPendingSharesAsync();
    }

    public async Task LoginAsync(string email, string password)
    {
        var response = await api.SendAsync(HttpMethod.Post, "/api/v1/auth/login", new LoginRequest(email, password));
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var detail = await api.ReadErrorDetailAsync(response, CancellationToken.None);
            throw new AccountException(detail ?? "E-mail ou senha inválidos.");
        }

        await EnsureSuccessAsync(response);
        var auth = await api.ReadJsonAsync<AuthResponse>(response, CancellationToken.None)
            ?? throw new InvalidOperationException("Resposta de autenticação inválida.");
        await StoreSessionAsync(email, auth);
        await FlushPendingSharesAsync();
    }

    public async Task DeleteAccountAsync()
    {
        var response = await api.SendAsync(HttpMethod.Delete, "/api/v1/auth/me", requireAuth: true);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
        {
            var detail = await api.ReadErrorDetailAsync(response, CancellationToken.None);
            throw new AccountException(detail ?? "Não foi possível excluir a conta.");
        }

        await LogoutAsync();
    }

    public async Task LogoutAsync()
    {
        await preferences.ClearAuthAsync();
        ResetState();
        Notify();
    }

    public async Task RefreshCreditsAsync()
    {
        if (!IsLoggedIn)
        {
            return;
        }

        var response = await api.SendAsync(HttpMethod.Get, "/api/v1/credits", requireAuth: true);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (!await TryRefreshSessionAsync())
            {
                await LogoutAsync();
            }

            return;
        }

        await EnsureSuccessAsync(response);
        var credits = await api.ReadJsonAsync<CreditsResponse>(response, CancellationToken.None);
        if (credits is null)
        {
            return;
        }

        CreditBalance = credits.Balance;
        ActiveUnlocks = credits.ActiveUnlocks;
        Notify();
    }

    public async Task<UnlockResponse> UnlockAsync(string featureCode)
    {
        var response = await api.SendAsync(
            HttpMethod.Post,
            "/api/v1/unlocks",
            new UnlockRequest(featureCode),
            requireAuth: true);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, CancellationToken.None);
            throw new AccountException(detail ?? "Não foi possível desbloquear.");
        }

        var result = await api.ReadJsonAsync<UnlockResponse>(response, CancellationToken.None)
            ?? throw new InvalidOperationException("Resposta de desbloqueio inválida.");

        CreditBalance = result.NewBalance;
        await RefreshCreditsAsync();
        return result;
    }

    public void ApplyCreditsFromShare(int creditsGranted, int newBalanceHint = 0)
    {
        if (creditsGranted > 0)
        {
            CreditBalance = Math.Max(CreditBalance + creditsGranted, newBalanceHint);
            Notify();
        }
    }

    public async Task FlushPendingSharesAsync()
    {
        if (!IsLoggedIn)
        {
            return;
        }

        await share.FlushPendingAsync();
        PendingShareCount = await share.GetPendingCountAsync();
        Notify();
    }

    private async Task StoreSessionAsync(string email, AuthResponse auth)
    {
        await preferences.SetAuthAsync(
            auth.AccessToken,
            auth.RefreshToken,
            auth.ExpiresAt,
            email,
            auth.PseudonymId);
        IsLoggedIn = true;
        Email = email;
        PseudonymId = auth.PseudonymId;
        await RefreshCreditsAsync();
        Notify();
    }

    private async Task<bool> TryRefreshSessionAsync()
    {
        var refreshToken = await preferences.GetRefreshTokenAsync();
        var email = await preferences.GetAccountEmailAsync();
        if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var response = await api.SendAsync(
            HttpMethod.Post,
            "/api/v1/auth/refresh",
            new RefreshRequest(refreshToken));

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var auth = await api.ReadJsonAsync<AuthResponse>(response, CancellationToken.None);
        if (auth is null)
        {
            return false;
        }

        await StoreSessionAsync(email, auth);
        return true;
    }

    private async Task RefreshProfileAsync()
    {
        var response = await api.SendAsync(HttpMethod.Get, "/api/v1/auth/me", requireAuth: true);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (!await TryRefreshSessionAsync())
            {
                await LogoutAsync();
            }

            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var me = await api.ReadJsonAsync<MeResponse>(response, CancellationToken.None);
        if (me is not null)
        {
            Email = me.Email;
            PseudonymId = Guid.TryParse(me.PseudonymId, out var pseudonym) ? pseudonym : PseudonymId;
        }
    }

    private void ResetState()
    {
        IsLoggedIn = false;
        Email = null;
        PseudonymId = null;
        CreditBalance = 0;
        PendingShareCount = 0;
        ActiveUnlocks = [];
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new AccountException(string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase ?? "Erro na API." : body);
    }

    private void Notify() => StateChanged?.Invoke();
}

public sealed class AccountException(string message) : Exception(message);
