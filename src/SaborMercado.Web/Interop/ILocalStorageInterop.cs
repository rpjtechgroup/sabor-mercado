namespace SaborMercado.Web.Interop;

public interface ILocalStorageInterop
{
    ValueTask SetItemAsync(string key, string value);

    ValueTask<string?> GetItemAsync(string key);

    ValueTask RemoveItemAsync(string key);
}
