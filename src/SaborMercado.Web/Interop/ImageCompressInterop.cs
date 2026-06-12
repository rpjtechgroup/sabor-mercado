using Microsoft.JSInterop;

namespace SaborMercado.Web.Interop;

public sealed class ImageCompressInterop(IJSRuntime js) : IAsyncDisposable
{
    private const string ModulePath = "./js/imageCompress.js";
    private IJSObjectReference? _module;

    public async Task<byte[]> CompressAsync(byte[] imageBytes)
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
        var result = await _module.InvokeAsync<int[]>("compressImageBytes", imageBytes);
        return result.Select(static b => (byte)b).ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
