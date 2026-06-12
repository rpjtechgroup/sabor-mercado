using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Domain.Status;

public static class StatusMessageEvaluator
{
    private static readonly TimeSpan PaceCooldownDuration = TimeSpan.FromMinutes(5);
    private const int PaceCooldownItemCount = 5;

    
    private static readonly (decimal Threshold, string Code)[] CrossingThresholds =
    [
        (100m, StatusCodes.BudgetReached),
        (90m, StatusCodes.BudgetHigh90),
        (75m, StatusCodes.BudgetWarn75),
        (50m, StatusCodes.BudgetHalf),
    ];

    public static EvaluationResult Evaluate(EvaluationInput input)
    {
        var cart = input.Cart;
        var percentUsed = cart.PercentUsed;
        var hasBudget = cart.BudgetAmount is { } budgetValue && budgetValue > 0m;

        
        var emitted = new HashSet<string>(input.Before.EmittedCodes);
        foreach (var (threshold, code) in CrossingThresholds)
        {
            if (percentUsed < threshold)
            {
                emitted.Remove(code);
            }
        }

        var after = input.Before with { EmittedCodes = emitted, LastPercentUsed = percentUsed };

        switch (input.Mutation)
        {
            case CartMutation.SessionStarted:
                return hasBudget
                    ? new EvaluationResult(
                        StatusMessage.Create(StatusCodes.BudgetSet, ("B", MoneyFormat.Format(cart.BudgetAmount!.Value))),
                        after)
                    : new EvaluationResult(null, after);

            case CartMutation.SessionFinished:
                return new EvaluationResult(BuildSessionFinished(cart), after);
        }

        
        if (!hasBudget)
        {
            return TryBuildOcrItemMessage(input, after)
                ?? new EvaluationResult(null, after);
        }

        var budget = cart.BudgetAmount!.Value;
        var remaining = budget - cart.Total;
        var isNewItem = input.Mutation is CartMutation.ItemAdded or CartMutation.ItemAddedOcr;

        
        if (isNewItem && cart.Total > budget)
        {
            return new EvaluationResult(
                StatusMessage.Create(StatusCodes.BudgetExceeded, ("excess", MoneyFormat.Format(cart.Total - budget))),
                after);
        }

        
        
        var percentBefore = input.Before.LastPercentUsed;
        foreach (var (threshold, code) in CrossingThresholds)
        {
            if (percentBefore < threshold && percentUsed >= threshold && !emitted.Contains(code))
            {
                emitted.Add(code);
                var message = code == StatusCodes.BudgetReached
                    ? StatusMessage.Create(code, ("B", MoneyFormat.Format(budget)))
                    : StatusMessage.Create(code, ("R", MoneyFormat.Format(remaining)));
                return new EvaluationResult(message, after);
            }
        }

        
        if (TryComputeProjection(cart, input.Now, out var projection) &&
            IsPaceCooldownElapsed(input.Before, cart, input.Now))
        {
            if (projection > budget * 1.05m && percentUsed < 100m)
            {
                after = after with
                {
                    LastPaceEmissionAt = input.Now,
                    ItemCountAtLastPaceEmission = cart.DistinctItemCount,
                };
                return new EvaluationResult(
                    StatusMessage.Create(StatusCodes.PaceProjectionOver, ("E", MoneyFormat.Format(projection))),
                    after);
            }

            if (projection <= budget && percentUsed >= 60m)
            {
                after = after with
                {
                    LastPaceEmissionAt = input.Now,
                    ItemCountAtLastPaceEmission = cart.DistinctItemCount,
                };
                return new EvaluationResult(
                    StatusMessage.Create(StatusCodes.PaceProjectionOk, ("E", MoneyFormat.Format(projection))),
                    after);
            }
        }

        return TryBuildOcrItemMessage(input, after) ?? new EvaluationResult(null, after);
    }

    private static EvaluationResult? TryBuildOcrItemMessage(EvaluationInput input, BudgetAlertState after)
    {
        if (input.Mutation != CartMutation.ItemAddedOcr ||
            input.OcrProductName is null ||
            input.OcrConfidence is null)
        {
            return null;
        }

        var code = input.OcrConfidence >= 0.8m
            ? StatusCodes.ItemAddedOcr
            : StatusCodes.ItemAddedOcrReview;

        return new EvaluationResult(
            StatusMessage.Create(
                code,
                ("produto", input.OcrProductName),
                ("T", MoneyFormat.Format(input.Cart.Total))),
            after);
    }

    
    
    
    
    
    private static bool TryComputeProjection(CartSnapshot cart, DateTimeOffset now, out decimal projection)
    {
        projection = 0m;

        if (cart.PlannedListSize is { } planned && planned > 0 && cart.DistinctItemCount >= 3)
        {
            projection = cart.Total / cart.DistinctItemCount * planned;
            return true;
        }

        if (cart.DistinctItemCount >= 5)
        {
            var elapsed = now - cart.SessionStartedAt;
            if (elapsed <= TimeSpan.Zero)
            {
                return false;
            }

            var ratio = (decimal)cart.EffectiveAverageSessionDuration.Ticks / elapsed.Ticks;
            projection = Math.Min(cart.Total * ratio, cart.Total * 3m);
            return true;
        }

        return false;
    }

    private static bool IsPaceCooldownElapsed(BudgetAlertState before, CartSnapshot cart, DateTimeOffset now)
    {
        if (before.LastPaceEmissionAt is not { } lastEmission)
        {
            return true;
        }

        if (now - lastEmission >= PaceCooldownDuration)
        {
            return true;
        }

        return before.ItemCountAtLastPaceEmission is { } lastCount &&
               cart.DistinctItemCount - lastCount >= PaceCooldownItemCount;
    }

    private static StatusMessage BuildSessionFinished(CartSnapshot cart)
    {
        var args = new List<(string, string)>
        {
            ("n", cart.DistinctItemCount.ToString(MoneyFormat.Culture)),
            ("T", MoneyFormat.Format(cart.Total)),
        };

        
        
        
        if (cart.BudgetAmount is { } budget && budget > 0m && cart.Total < budget)
        {
            args.Add(("variant", "under"));
            args.Add(("saving", MoneyFormat.Format(budget - cart.Total)));
        }
        else if (cart.BudgetAmount is { } budgetOver && budgetOver > 0m && cart.Total > budgetOver)
        {
            args.Add(("variant", "over"));
            args.Add(("excess", MoneyFormat.Format(cart.Total - budgetOver)));
        }
        else
        {
            args.Add(("variant", "exact"));
        }

        return StatusMessage.Create(StatusCodes.SessionFinished, args.ToArray());
    }
}
