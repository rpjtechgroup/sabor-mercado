using Microsoft.JSInterop;

namespace SaborMercado.Web.Interop;

public interface IGoogleSignInService
{
    ValueTask<bool> RenderButtonAsync(string elementId, string clientId, IGoogleSignInListener listener);
}

public interface IGoogleSignInListener
{
    Task OnGoogleCredential(string credential);
}

public sealed class GoogleSignInInterop(IJSRuntime jsRuntime) : IGoogleSignInService, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<IGoogleSignInListener>? _dotNetRef;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/googleSignIn.js");
        return _module;
    }

    public async ValueTask<bool> RenderButtonAsync(string elementId, string clientId, IGoogleSignInListener listener)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return false;
        }

        _dotNetRef?.Dispose();
        _dotNetRef = DotNetObjectReference.Create(listener);
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("renderButton", elementId, clientId, _dotNetRef);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        _dotNetRef = null;

        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }

            _module = null;
        }
    }
}

public interface ISupportDiagnosticsInterop
{
    ValueTask<bool> IsOnlineAsync();

    ValueTask<ClientEnvironmentInfo> GetClientEnvironmentAsync();
}

public sealed record ClientEnvironmentInfo(
    string UserAgent,
    string Language,
    int ViewportWidth,
    int ViewportHeight,
    bool Online,
    bool ServiceWorkerSupported,
    bool ServiceWorkerActive);

public sealed class SupportDiagnosticsInterop(IJSRuntime jsRuntime) : ISupportDiagnosticsInterop, IAsyncDisposable
{
    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/supportDiagnostics.js");
        return _module;
    }

    public async ValueTask<bool> IsOnlineAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("isOnline");
    }

    public async ValueTask<ClientEnvironmentInfo> GetClientEnvironmentAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<ClientEnvironmentInfo>("getClientEnvironment");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }

            _module = null;
        }
    }
}
