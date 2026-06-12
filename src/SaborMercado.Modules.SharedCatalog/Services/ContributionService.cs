using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Modules.SharedCatalog.Domain;
using SaborMercado.Shared.Rewards;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Modules.SharedCatalog.Services;

public sealed class ContributionService(
    SharedCatalogDbContext db,
    IContributionRewardService rewards,
    TimeProvider clock)
{
    private const int DailySubmissionLimit = 50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<PriceObservationResponse> SubmitAsync(
        ClaimsPrincipal user,
        SubmitPriceObservationRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId(user);
        var pseudonymId = GetPseudonymId(user);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var cached = await TryGetIdempotentResponseAsync(accountId, idempotencyKey, request, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        if (request.ObservedOn > today)
        {
            throw new ContributionException(ContributionErrorCodes.InvalidDate, "Data da observação não pode ser futura.");
        }

        if (request.Price <= 0m)
        {
            throw new ContributionException(ContributionErrorCodes.InvalidPrice, "Preço deve ser maior que zero.");
        }

        var submissionsToday = await db.PriceObservations.CountAsync(
            o => o.ContributorPseudonymId == pseudonymId && o.ObservedOn == today,
            cancellationToken);

        if (submissionsToday >= DailySubmissionLimit)
        {
            throw new ContributionException(ContributionErrorCodes.DailyLimit, "Limite diário de compartilhamentos atingido.");
        }

        var ean = ProductNormalizer.NormalizeEan(request.Ean);
        var normalizedName = ProductNormalizer.NormalizeName(request.ProductName);
        var market = await FindOrCreateMarketAsync(request, cancellationToken);
        var (product, isNew) = await FindOrCreateProductAsync(request, normalizedName, ean, cancellationToken);

        if (!isNew)
        {
            var historicalPrices = await db.PriceObservations
                .AsNoTracking()
                .Where(o => o.SharedProductId == product.Id && o.Status == ObservationStatus.Accepted)
                .OrderByDescending(o => o.ObservedOn)
                .Take(100)
                .Select(o => o.Price)
                .ToListAsync(cancellationToken);

            if (!PriceOutlierValidator.IsPlausible(request.Price, historicalPrices))
            {
                throw new ContributionException(
                    ContributionErrorCodes.PriceOutlier,
                    "Preço fora da faixa plausível para este produto.");
            }
        }

        var duplicate = await db.PriceObservations.AnyAsync(
            o => o.ContributorPseudonymId == pseudonymId &&
                 o.SharedProductId == product.Id &&
                 o.MarketId == market.Id &&
                 o.ObservedOn == request.ObservedOn,
            cancellationToken);

        if (duplicate)
        {
            throw new ContributionException(ContributionErrorCodes.Duplicate, "Você já compartilhou este produto neste mercado hoje.");
        }

        var observation = new PriceObservation
        {
            Id = Guid.NewGuid(),
            SharedProductId = product.Id,
            MarketId = market.Id,
            Price = Math.Round(request.Price, 2, MidpointRounding.AwayFromZero),
            ObservedOn = request.ObservedOn,
            ContributorPseudonymId = pseudonymId,
            Status = ObservationStatus.Accepted,
            CreatedAt = clock.GetUtcNow(),
        };

        db.PriceObservations.Add(observation);
        await db.SaveChangesAsync(cancellationToken);

        var credits = await rewards.GrantForAcceptedObservationAsync(
            accountId,
            new ContributionRewardContext(observation.Id, isNew, ean is not null),
            cancellationToken);

        var response = new PriceObservationResponse(observation.Id, "Accepted", credits, isNew);
        await StoreIdempotentResponseAsync(accountId, idempotencyKey, request, response, cancellationToken);
        return response;
    }

    private async Task<PriceObservationResponse?> TryGetIdempotentResponseAsync(
        Guid accountId,
        string idempotencyKey,
        SubmitPriceObservationRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.ContributionIdempotencies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.AccountId == accountId && x.IdempotencyKey == idempotencyKey,
                cancellationToken);

        if (existing is null)
        {
            return null;
        }

        var requestHash = HashRequest(request);
        if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
        {
            throw new ContributionException(
                ContributionErrorCodes.IdempotencyConflict,
                "Chave de idempotência reutilizada com payload diferente.");
        }

        return JsonSerializer.Deserialize<PriceObservationResponse>(existing.ResponseJson, JsonOptions);
    }

    private async Task StoreIdempotentResponseAsync(
        Guid accountId,
        string? idempotencyKey,
        SubmitPriceObservationRequest request,
        PriceObservationResponse response,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return;
        }

        db.ContributionIdempotencies.Add(new ContributionIdempotency
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            IdempotencyKey = idempotencyKey.Trim(),
            RequestHash = HashRequest(request),
            ResponseJson = JsonSerializer.Serialize(response, JsonOptions),
            CreatedAt = clock.GetUtcNow(),
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string HashRequest(SubmitPriceObservationRequest request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }

    private static Guid GetAccountId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id)
            ? id
            : throw new UnauthorizedAccessException();
    }

    private static Guid GetPseudonymId(ClaimsPrincipal user)
    {
        var pseudonym = user.FindFirstValue("pseudonym_id");
        return Guid.TryParse(pseudonym, out var id)
            ? id
            : throw new UnauthorizedAccessException("Pseudônimo ausente no token.");
    }

    private async Task<Market> FindOrCreateMarketAsync(
        SubmitPriceObservationRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.MarketName.Trim();
        var city = request.MarketCity?.Trim();
        var state = request.MarketState?.Trim();

        var existing = await db.Markets.FirstOrDefaultAsync(
            m => m.Name == name && m.City == city && m.State == state,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var market = new Market
        {
            Id = Guid.NewGuid(),
            Name = name,
            City = city,
            State = state,
        };
        db.Markets.Add(market);
        await db.SaveChangesAsync(cancellationToken);
        return market;
    }

    private async Task<(SharedProduct Product, bool IsNew)> FindOrCreateProductAsync(
        SubmitPriceObservationRequest request,
        string normalizedName,
        string? ean,
        CancellationToken cancellationToken)
    {
        SharedProduct? existing = null;
        if (ean is not null)
        {
            existing = await db.SharedProducts.FirstOrDefaultAsync(p => p.Ean == ean, cancellationToken);
        }

        existing ??= await db.SharedProducts.FirstOrDefaultAsync(
            p => p.NormalizedName == normalizedName && p.Brand == request.Brand,
            cancellationToken);

        if (existing is not null)
        {
            return (existing, false);
        }

        var product = new SharedProduct
        {
            Id = Guid.NewGuid(),
            NormalizedName = normalizedName,
            Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
            Ean = ean,
            QuantityValue = request.QuantityValue,
            QuantityUnit = request.QuantityUnit,
        };
        db.SharedProducts.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return (product, true);
    }
}

public sealed class ContributionException(string code, string detail) : Exception(detail)
{
    public string Code { get; } = code;
}
