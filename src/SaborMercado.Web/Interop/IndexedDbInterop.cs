using Microsoft.JSInterop;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Interop;

public sealed class IndexedDbInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/indexedDb.js");
            await _module.InvokeVoidAsync("open", StorageSchema.DatabaseName, StorageSchema.DatabaseVersion);
        }

        return _module;
    }

    public async ValueTask PutAsync<T>(string storeName, T item)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("put", storeName, item);
    }

    public async ValueTask<T?> GetAsync<T>(string storeName, Guid key)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<T?>("get", storeName, key);
    }

    public async ValueTask<List<T>> GetAllAsync<T>(string storeName)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<List<T>>("getAll", storeName);
    }

    public async ValueTask<List<T>> GetAllByIndexAsync<T>(string storeName, string indexName, Guid value)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<List<T>>("getAllByIndex", storeName, indexName, value);
    }

    public async ValueTask DeleteAsync(string storeName, Guid key)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("remove", storeName, key);
    }

    public async ValueTask DeleteAllByIndexAsync(string storeName, string indexName, Guid value)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("removeAllByIndex", storeName, indexName, value);
    }

    public async ValueTask ClearAsync(string storeName)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("clear", storeName);
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
        }
    }
}
