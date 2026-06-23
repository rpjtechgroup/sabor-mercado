using Microsoft.JSInterop;

namespace SaborMercado.Web.Interop;

public interface IVoiceFieldExtractorInterop
{
    ValueTask<bool> IsModelSupportedAsync();

    ValueTask<string> ExtractProductFieldsAsync(string transcript);

    ValueTask DisposeModelAsync();
}

public sealed class VoiceFieldExtractorInterop(IJSRuntime jsRuntime) : IVoiceFieldExtractorInterop, IAsyncDisposable
{
    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/voiceFieldExtractor.js");
        return _module;
    }

    public async ValueTask<bool> IsModelSupportedAsync()
    {
        try
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<bool>("isModelSupported");
        }
        catch (JSException)
        {
            return false;
        }
    }

    public async ValueTask<string> ExtractProductFieldsAsync(string transcript)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("extractProductFields", transcript);
    }

    public async ValueTask DisposeModelAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("disposeModel");
        }
        catch (JSDisconnectedException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeModelAsync();

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
