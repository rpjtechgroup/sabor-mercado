using SaborMercado.Web.Contracts.SharedCatalog;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Sharing;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Account;

public sealed class ShareService(
    SaborMercadoApiClient api,
    IPendingShareStore pendingShares,
    TimeProvider clock)
{
    public async Task<ShareResult> SharePriceAsync(
        Product product,
        decimal price,
        string marketName,
        string? marketCity,
        string? marketState,
        DateOnly observedOn,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(product, price, marketName, marketCity, marketState, observedOn);
        var idempotencyKey = Guid.NewGuid().ToString();

        try
        {
            var result = await SendAsync(request, idempotencyKey, cancellationToken);
            return ShareResult.Sent(result);
        }
        catch (HttpRequestException)
        {
            await EnqueueAsync(product, price, marketName, marketCity, marketState, observedOn);
            return ShareResult.Enqueued();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            await EnqueueAsync(product, price, marketName, marketCity, marketState, observedOn);
            return ShareResult.Enqueued();
        }
    }

    public async Task<int> FlushPendingAsync(CancellationToken cancellationToken = default)
    {
        var pending = await pendingShares.GetAllAsync();
        var sent = 0;

        foreach (var item in pending.OrderBy(p => p.QueuedAt))
        {
            try
            {
                var request = new SubmitPriceObservationRequest(
                    item.ProductName,
                    item.Brand,
                    item.QuantityValue,
                    item.QuantityUnit,
                    item.Ean,
                    item.Price,
                    item.MarketName,
                    item.MarketCity,
                    item.MarketState,
                    item.ObservedOn);

                await SendAsync(request, item.Id.ToString(), cancellationToken);
                await pendingShares.RemoveAsync(item.Id);
                sent++;
            }
            catch (HttpRequestException)
            {
                break;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return sent;
    }

    public async Task<int> GetPendingCountAsync() =>
        (await pendingShares.GetAllAsync()).Count;

    private async Task<PriceObservationResponse> SendAsync(
        SubmitPriceObservationRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var response = await api.SendAsync(
            HttpMethod.Post,
            "/api/v1/price-observations",
            request,
            requireAuth: true,
            idempotencyKey: idempotencyKey,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
            throw new ShareException(detail ?? "Não foi possível compartilhar o preço.");
        }

        return await api.ReadJsonAsync<PriceObservationResponse>(response, cancellationToken)
            ?? throw new InvalidOperationException("Resposta de compartilhamento inválida.");
    }

    private async Task EnqueueAsync(
        Product product,
        decimal price,
        string marketName,
        string? marketCity,
        string? marketState,
        DateOnly observedOn)
    {
        await pendingShares.EnqueueAsync(new PendingShare
        {
            ProductName = product.Name,
            Brand = product.Brand,
            QuantityValue = product.QuantityValue,
            QuantityUnit = product.QuantityUnit?.Label(),
            Ean = product.Ean,
            Price = price,
            MarketName = marketName,
            MarketCity = marketCity,
            MarketState = marketState,
            ObservedOn = observedOn,
            QueuedAt = clock.GetUtcNow(),
        });
    }

    private static SubmitPriceObservationRequest BuildRequest(
        Product product,
        decimal price,
        string marketName,
        string? marketCity,
        string? marketState,
        DateOnly observedOn) =>
        new(
            product.Name,
            product.Brand,
            product.QuantityValue,
            product.QuantityUnit?.Label(),
            product.Ean,
            price,
            marketName,
            marketCity,
            marketState,
            observedOn);
}

public sealed record ShareResult(bool IsQueued, PriceObservationResponse? Response)
{
    public static ShareResult Sent(PriceObservationResponse response) => new(false, response);

    public static ShareResult Enqueued() => new(true, null);
}

public sealed class ShareException(string message) : Exception(message);
