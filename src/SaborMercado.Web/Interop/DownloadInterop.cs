using Microsoft.JSInterop;

namespace SaborMercado.Web.Interop;

public sealed class DownloadInterop(IJSRuntime jsRuntime)
{
    public ValueTask DownloadTextAsync(string fileName, string content, string mimeType = "text/csv;charset=utf-8") =>
        jsRuntime.InvokeVoidAsync("saborMercado.downloadText", fileName, content, mimeType);
}
