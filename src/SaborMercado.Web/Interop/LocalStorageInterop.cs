using Microsoft.JSInterop;

namespace SaborMercado.Web.Interop;

public sealed class LocalStorageInterop(IJSRuntime jsRuntime)
{
    public ValueTask SetItemAsync(string key, string value) =>
        jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);

    public ValueTask<string?> GetItemAsync(string key) =>
        jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

    public ValueTask RemoveItemAsync(string key) =>
        jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
}
