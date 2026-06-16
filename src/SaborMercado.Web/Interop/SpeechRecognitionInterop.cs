using Microsoft.JSInterop;

namespace SaborMercado.Web.Interop;

public interface ISpeechRecognitionService
{
    ValueTask<bool> IsSupportedAsync();

    ValueTask StartListeningAsync(ISpeechRecognitionListener listener, string lang = "pt-BR");

    ValueTask StopListeningAsync();
}

public interface ISpeechRecognitionListener
{
    Task OnSpeechResult(string transcript, bool isFinal);

    Task OnSpeechError(string message);

    Task OnSpeechEnd();
}

public sealed class SpeechRecognitionInterop(IJSRuntime jsRuntime) : ISpeechRecognitionService, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<ISpeechRecognitionListener>? _dotNetRef;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/speechRecognition.js");
        }

        return _module;
    }

    public async ValueTask<bool> IsSupportedAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("isSupported");
    }

    public async ValueTask StartListeningAsync(ISpeechRecognitionListener listener, string lang = "pt-BR")
    {
        await StopListeningAsync();
        _dotNetRef = DotNetObjectReference.Create(listener);
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("start", _dotNetRef, new { lang, continuous = false, interimResults = true });
    }

    public async ValueTask StopListeningAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("stop");
            }
            catch (JSDisconnectedException)
            {
                
            }
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopListeningAsync();

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
