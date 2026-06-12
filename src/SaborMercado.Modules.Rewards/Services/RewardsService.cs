using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Modules.Rewards.Domain;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Services;

public sealed class RewardsService(RewardsDbContext db, TimeProvider clock)
{
    public async Task<CreditsResponse> GetCreditsAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var entries = await db.CreditLedgerEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .ToListAsync(cancellationToken);

        var balance = entries.Sum(e => e.Amount);
        var recentEntries = entries
            .OrderByDescending(e => e.CreatedAt)
            .Take(20)
            .Select(e => new CreditLedgerEntryDto(e.Amount, e.Reason.ToString(), e.CreatedAt))
            .ToList();

        var now = clock.GetUtcNow();
        var unlocks = (await db.FeatureUnlocks
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .ToListAsync(cancellationToken))
            .Where(u => u.ExpiresAt is null || u.ExpiresAt > now)
            .OrderByDescending(u => u.UnlockedAt)
            .Select(u => new FeatureUnlockDto(u.FeatureCode, u.UnlockedAt, u.ExpiresAt))
            .ToList();

        return new CreditsResponse(balance, recentEntries, unlocks);
    }

    public async Task<UnlockResponse> UnlockAsync(
        ClaimsPrincipal user,
        UnlockRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!UnlockCatalog.Features.TryGetValue(request.FeatureCode, out var definition))
        {
            throw new RewardsException(RewardsErrorCodes.UnknownFeature, "Funcionalidade desconhecida.");
        }

        if (!definition.IsAvailable)
        {
            throw new RewardsException(RewardsErrorCodes.FeatureNotAvailable, "Funcionalidade ainda não disponível.");
        }

        var now = clock.GetUtcNow();
        var active = (await db.FeatureUnlocks
                .AsNoTracking()
                .Where(u => u.UserId == userId && u.FeatureCode == request.FeatureCode)
                .ToListAsync(cancellationToken))
            .Any(u => u.ExpiresAt is null || u.ExpiresAt > now);

        if (active)
        {
            throw new RewardsException(RewardsErrorCodes.FeatureAlreadyUnlocked, "Funcionalidade já desbloqueada.");
        }

        var balance = await GetBalanceAsync(userId, cancellationToken);
        if (balance < definition.Cost)
        {
            throw new RewardsException(
                RewardsErrorCodes.InsufficientCredits,
                $"Créditos insuficientes. Necessário: {definition.Cost}, saldo: {balance}.");
        }

        var expiresAt = definition.Duration is { } duration ? now.Add(duration) : (DateTimeOffset?)null;

        db.CreditLedgerEntries.Add(new CreditLedgerEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = -definition.Cost,
            Reason = CreditReason.UnlockSpend,
            CreatedAt = now,
        });

        db.FeatureUnlocks.Add(new FeatureUnlock
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FeatureCode = request.FeatureCode,
            UnlockedAt = now,
            ExpiresAt = expiresAt,
        });

        await db.SaveChangesAsync(cancellationToken);

        var newBalance = balance - definition.Cost;
        return new UnlockResponse(request.FeatureCode, newBalance, expiresAt);
    }

    public async Task<bool> HasActiveUnlockAsync(Guid userId, string featureCode, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var unlocks = await db.FeatureUnlocks
            .AsNoTracking()
            .Where(u => u.UserId == userId && u.FeatureCode == featureCode)
            .ToListAsync(cancellationToken);

        return unlocks.Any(u => u.ExpiresAt is null || u.ExpiresAt > now);
    }

    private async Task<int> GetBalanceAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.CreditLedgerEntries
            .Where(e => e.UserId == userId)
            .SumAsync(e => e.Amount, cancellationToken);

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id)
            ? id
            : throw new UnauthorizedAccessException();
    }
}

public sealed class RewardsException(string code, string detail) : Exception(detail)
{
    public string Code { get; } = code;
}
