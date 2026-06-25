using System.Net;
using SaborMercado.Web.Contracts.Auth;
using SaborMercado.Web.Features.Gamification;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Account;

public sealed class AccountService(
    SaborMercadoApiClient api,
    IPreferencesStore preferences,
    ShareService share,
    GamificationMetricsService gamificationMetrics,
    ClientGamificationSyncService gamificationSync,
    TimeProvider clock)
{
    public event Action? StateChanged;

    public bool IsLoggedIn { get; private set; }

    public string? Email { get; private set; }

    public Guid? PseudonymId { get; private set; }

    public int PendingShareCount { get; private set; }

    public async Task InitializeAsync()
    {
        await gamificationMetrics.InitializeAsync();

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
        await gamificationMetrics.UpdateLoginStreakAsync();
        await FlushPendingSharesAsync();
        PendingShareCount = await share.GetPendingCountAsync();
        await gamificationSync.TrySyncAsync(force: true);
        Notify();
    }

    public async Task RegisterAsync(string email, string password)
    {
        var response = await api.SendAsync(HttpMethod.Post, "/api/v1/auth/register", new RegisterRequest(email, password));
        await EnsureSuccessAsync(response);
        var auth = await api.ReadJsonAsync<AuthResponse>(response, CancellationToken.None)
            ?? throw new InvalidOperationException("Resposta de autenticação inválida.");
        await StoreSessionAsync(email, auth);
        await gamificationMetrics.UpdateLoginStreakAsync();
        await FlushPendingSharesAsync();
        await gamificationSync.TrySyncAsync(force: true);
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
        await gamificationMetrics.UpdateLoginStreakAsync();
        await FlushPendingSharesAsync();
        await gamificationSync.TrySyncAsync(force: true);
    }

    public async Task LoginWithGoogleAsync(string idToken)
    {
        var response = await api.SendAsync(HttpMethod.Post, "/api/v1/auth/google", new GoogleLoginRequest(idToken));
        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, CancellationToken.None);
            throw new AccountException(detail ?? "Não foi possível entrar com Google.");
        }

        var auth = await api.ReadJsonAsync<AuthResponse>(response, CancellationToken.None)
            ?? throw new InvalidOperationException("Resposta de autenticação inválida.");

        await RefreshProfileAfterGoogleAsync(auth);
        await gamificationMetrics.UpdateLoginStreakAsync();
        await FlushPendingSharesAsync();
        await gamificationSync.TrySyncAsync(force: true);
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

    private async Task RefreshProfileAfterGoogleAsync(AuthResponse auth)
    {
        await preferences.SetAuthAsync(
            auth.AccessToken,
            auth.RefreshToken,
            auth.ExpiresAt,
            string.Empty,
            auth.PseudonymId);

        IsLoggedIn = true;
        PseudonymId = auth.PseudonymId;
        await RefreshProfileAsync();
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
            if (!string.IsNullOrWhiteSpace(me.Email))
            {
                var token = await preferences.GetAccessTokenAsync();
                var refresh = await preferences.GetRefreshTokenAsync();
                var expires = await preferences.GetAccessTokenExpiresAtAsync();
                if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(refresh) && expires is not null)
                {
                    await preferences.SetAuthAsync(token, refresh, expires.Value, me.Email, PseudonymId ?? Guid.Empty);
                }
            }
        }
    }

    private void ResetState()
    {
        IsLoggedIn = false;
        Email = null;
        PseudonymId = null;
        PendingShareCount = 0;
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
