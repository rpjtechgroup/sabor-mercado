using Microsoft.JSInterop;

namespace SaborMercado.Web.Interop;

public sealed class GeolocationInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/geolocation.js");
        }

        return _module;
    }

    public async ValueTask<GeoCoordinates> GetCurrentPositionAsync()
    {
        var module = await GetModuleAsync();
        var result = await module.InvokeAsync<GeoPositionDto>("getCurrentPosition");
        return new GeoCoordinates((decimal)result.Latitude, (decimal)result.Longitude);
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

    private sealed class GeoPositionDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
