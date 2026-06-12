using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Domain.Status;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Tests.Domain;

/// <summary>
/// Cobertura do catálogo docs/domain/status-messages.md: gatilhos,
/// prioridades, emissão única, rearme, cooldown e regra "sem meta".
/// </summary>
public class StatusMessageEvaluatorTests
{
    private static readonly DateTimeOffset SessionStart = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private static CartSnapshot Snapshot(
        decimal total,
        int itemCount = 1,
        decimal? budget = 100m,
        int? plannedListSize = null,
        TimeSpan? averageDuration = null) =>
        new(total, itemCount, budget, SessionStart, plannedListSize, averageDuration);

    private static EvaluationResult Evaluate(
        CartSnapshot cart,
        CartMutation mutation,
        BudgetAlertState? before = null,
        DateTimeOffset? now = null) =>
        StatusMessageEvaluator.Evaluate(new EvaluationInput(
            before ?? BudgetAlertState.Initial,
            cart,
            mutation,
            now ?? SessionStart.AddMinutes(10)));

    private static BudgetAlertState StateAt(decimal percent, params string[] emitted) =>
        new() { EmittedCodes = [.. emitted], LastPercentUsed = percent };

    // ----- BUDGET_SET -----

    [Fact]
    public void BudgetSet_OnSessionStartWithBudget_EmitsBudgetSet()
    {
        var result = Evaluate(Snapshot(0m, 0, budget: 300m), CartMutation.SessionStarted);

        Assert.Equal(StatusCodes.BudgetSet, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(300m), result.Message!.Args["B"]);
    }

    [Fact]
    public void SessionStart_WithoutBudget_EmitsNothing()
    {
        var result = Evaluate(Snapshot(0m, 0, budget: null), CartMutation.SessionStarted);

        Assert.Null(result.Message);
    }

    // ----- Cruzamentos de limiar -----

    [Fact]
    public void BudgetAlert_Crosses50Percent_EmitsBudgetHalf()
    {
        var result = Evaluate(Snapshot(55m), CartMutation.ItemAdded, StateAt(40m));

        Assert.Equal(StatusCodes.BudgetHalf, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(45m), result.Message!.Args["R"]);
    }

    [Fact]
    public void BudgetAlert_Crosses75Percent_EmitsBudgetWarn75()
    {
        var result = Evaluate(
            Snapshot(77m), CartMutation.ItemAdded, StateAt(70m, StatusCodes.BudgetHalf));

        Assert.Equal(StatusCodes.BudgetWarn75, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(23m), result.Message!.Args["R"]);
    }

    [Fact]
    public void BudgetAlert_Crosses90Percent_EmitsBudgetHigh90()
    {
        var result = Evaluate(
            Snapshot(92m), CartMutation.ItemAdded,
            StateAt(80m, StatusCodes.BudgetHalf, StatusCodes.BudgetWarn75));

        Assert.Equal(StatusCodes.BudgetHigh90, result.Message?.Code);
    }

    [Fact]
    public void BudgetAlert_Reaches100PercentExactly_EmitsBudgetReached()
    {
        var result = Evaluate(Snapshot(100m), CartMutation.ItemAdded, StateAt(95m));

        Assert.Equal(StatusCodes.BudgetReached, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(100m), result.Message!.Args["B"]);
    }

    [Fact]
    public void BudgetAlert_CrossingOnItemUpdated_AlsoEmits()
    {
        var result = Evaluate(Snapshot(55m), CartMutation.ItemUpdated, StateAt(45m));

        Assert.Equal(StatusCodes.BudgetHalf, result.Message?.Code);
    }

    // ----- BUDGET_EXCEEDED -----

    [Fact]
    public void BudgetExceeded_OnItemAddedWhileOver_EmitsWithExcess()
    {
        var result = Evaluate(Snapshot(127m), CartMutation.ItemAdded, StateAt(110m));

        Assert.Equal(StatusCodes.BudgetExceeded, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(27m), result.Message!.Args["excess"]);
    }

    [Fact]
    public void BudgetExceeded_EmitsOnEveryNewItemWhileOver()
    {
        var first = Evaluate(Snapshot(110m), CartMutation.ItemAdded, StateAt(90m));
        var second = Evaluate(Snapshot(120m), CartMutation.ItemAdded, first.After);

        Assert.Equal(StatusCodes.BudgetExceeded, first.Message?.Code);
        Assert.Equal(StatusCodes.BudgetExceeded, second.Message?.Code);
    }

    [Fact]
    public void BudgetExceeded_NotEmittedOnUpdateWithoutCrossing()
    {
        // Gatilho é "a cada NOVO item enquanto estourado"; update sem cruzar não emite.
        var result = Evaluate(Snapshot(130m), CartMutation.ItemUpdated, StateAt(120m));

        Assert.Null(result.Message);
    }

    // ----- Prioridade -----

    [Fact]
    public void Priority_CrossingMultipleThresholds_EmitsOnlyHighestPriority()
    {
        var result = Evaluate(Snapshot(95m), CartMutation.ItemAdded, StateAt(10m));

        Assert.Equal(StatusCodes.BudgetHigh90, result.Message?.Code);
    }

    [Fact]
    public void Priority_ExceededBeatsReached_OnSameMutation()
    {
        var result = Evaluate(Snapshot(110m), CartMutation.ItemAdded, StateAt(90m));

        Assert.Equal(StatusCodes.BudgetExceeded, result.Message?.Code);
    }

    // ----- Emissão única + rearme -----

    [Fact]
    public void CrossingCode_DoesNotRepeatWhileAboveThreshold()
    {
        var first = Evaluate(Snapshot(55m), CartMutation.ItemAdded, StateAt(40m));
        var second = Evaluate(Snapshot(58m), CartMutation.ItemAdded, first.After);

        Assert.Equal(StatusCodes.BudgetHalf, first.Message?.Code);
        Assert.Null(second.Message);
    }

    [Fact]
    public void Rearm_DroppingBelowThresholdAndCrossingAgain_EmitsAgain()
    {
        var crossed = Evaluate(Snapshot(55m), CartMutation.ItemAdded, StateAt(40m));
        var dropped = Evaluate(Snapshot(40m, itemCount: 1), CartMutation.ItemRemoved, crossed.After);
        var crossedAgain = Evaluate(Snapshot(55m), CartMutation.ItemAdded, dropped.After);

        Assert.Equal(StatusCodes.BudgetHalf, crossed.Message?.Code);
        Assert.Null(dropped.Message);
        Assert.Equal(StatusCodes.BudgetHalf, crossedAgain.Message?.Code);
    }

    [Fact]
    public void After_AlwaysTracksLastPercentUsed()
    {
        var result = Evaluate(Snapshot(42m), CartMutation.ItemAdded, StateAt(30m));

        Assert.Equal(42m, result.After.LastPercentUsed);
    }

    // ----- Sem meta (regra de emissão nº 4) -----

    [Fact]
    public void NoBudget_CartMutations_EmitNothing()
    {
        var added = Evaluate(Snapshot(500m, 10, budget: null), CartMutation.ItemAdded);
        var removed = Evaluate(Snapshot(500m, 10, budget: null), CartMutation.ItemRemoved);

        Assert.Null(added.Message);
        Assert.Null(removed.Message);
    }

    // ----- SESSION_FINISHED -----

    [Fact]
    public void SessionFinished_WithoutBudget_UsesExactVariant()
    {
        var result = Evaluate(Snapshot(80m, 7, budget: null), CartMutation.SessionFinished);

        Assert.Equal(StatusCodes.SessionFinished, result.Message?.Code);
        Assert.Equal("exact", result.Message!.Args["variant"]);
        Assert.Equal("7", result.Message.Args["n"]);
        Assert.Equal(MoneyFormat.Format(80m), result.Message.Args["T"]);
    }

    [Fact]
    public void SessionFinished_UnderBudget_IncludesSaving()
    {
        var result = Evaluate(Snapshot(80m, 7), CartMutation.SessionFinished);

        Assert.Equal(StatusCodes.SessionFinished, result.Message?.Code);
        Assert.Equal("under", result.Message!.Args["variant"]);
        Assert.Equal(MoneyFormat.Format(20m), result.Message.Args["saving"]);
    }

    [Fact]
    public void SessionFinished_OverBudget_IncludesExcess()
    {
        var result = Evaluate(Snapshot(127m, 9), CartMutation.SessionFinished);

        Assert.Equal("over", result.Message!.Args["variant"]);
        Assert.Equal(MoneyFormat.Format(27m), result.Message.Args["excess"]);
    }

    // ----- Projeções PACE_* -----

    [Fact]
    public void PaceProjectionOver_ByTemporalPace_EmitsWithProjection()
    {
        // n=5, sem lista; 10 min decorridos de média 40 → E = T×4, teto 3×T.
        // T=40 → E=120 (teto), B=100: E > 105 e P=40 < 100 → OVER.
        var result = Evaluate(
            Snapshot(40m, 5), CartMutation.ItemAdded, StateAt(35m),
            now: SessionStart.AddMinutes(10));

        Assert.Equal(StatusCodes.PaceProjectionOver, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(120m), result.Message!.Args["E"]);
    }

    [Fact]
    public void PaceProjectionOk_WhenProjectionWithinBudgetAndAbove60Percent()
    {
        // 40 min decorridos de média 40 → E = T = 70 ≤ 100 e P=70 ≥ 60 → OK.
        var result = Evaluate(
            Snapshot(70m, 6), CartMutation.ItemAdded,
            StateAt(65m, StatusCodes.BudgetHalf),
            now: SessionStart.AddMinutes(40));

        Assert.Equal(StatusCodes.PaceProjectionOk, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(70m), result.Message!.Args["E"]);
    }

    [Fact]
    public void PaceProjection_WithPlannedList_UsesAverageTicketFormula()
    {
        // Regra 1: n=3, N=10, T=33 → E = (33/3)×10 = 110 > 105 → OVER.
        var result = Evaluate(
            Snapshot(33m, 3, plannedListSize: 10), CartMutation.ItemAdded, StateAt(30m));

        Assert.Equal(StatusCodes.PaceProjectionOver, result.Message?.Code);
        Assert.Equal(MoneyFormat.Format(110m), result.Message!.Args["E"]);
    }

    [Fact]
    public void PaceProjection_InsufficientData_EmitsNothing()
    {
        // Sem lista e n=4 < 5 → projeção indisponível.
        var result = Evaluate(
            Snapshot(40m, 4), CartMutation.ItemAdded, StateAt(38m),
            now: SessionStart.AddMinutes(10));

        Assert.Null(result.Message);
    }

    [Fact]
    public void PaceCooldown_Within5MinutesAndLessThan5Items_Blocks()
    {
        var before = StateAt(35m) with
        {
            LastPaceEmissionAt = SessionStart.AddMinutes(8),
            ItemCountAtLastPaceEmission = 4,
        };

        var result = Evaluate(
            Snapshot(40m, 5), CartMutation.ItemAdded, before,
            now: SessionStart.AddMinutes(10));

        Assert.Null(result.Message);
    }

    [Fact]
    public void PaceCooldown_After5Minutes_AllowsEmission()
    {
        var before = StateAt(35m) with
        {
            LastPaceEmissionAt = SessionStart.AddMinutes(5),
            ItemCountAtLastPaceEmission = 5,
        };

        var result = Evaluate(
            Snapshot(40m, 5), CartMutation.ItemAdded, before,
            now: SessionStart.AddMinutes(10));

        Assert.Equal(StatusCodes.PaceProjectionOver, result.Message?.Code);
    }

    [Fact]
    public void PaceCooldown_After5MoreItems_AllowsEmission()
    {
        var before = StateAt(35m) with
        {
            LastPaceEmissionAt = SessionStart.AddMinutes(9),
            ItemCountAtLastPaceEmission = 5,
        };

        var result = Evaluate(
            Snapshot(40m, 10), CartMutation.ItemAdded, before,
            now: SessionStart.AddMinutes(10));

        Assert.Equal(StatusCodes.PaceProjectionOver, result.Message?.Code);
    }

    [Fact]
    public void PaceEmission_RecordsCooldownStateInAfter()
    {
        var now = SessionStart.AddMinutes(10);
        var result = Evaluate(Snapshot(40m, 5), CartMutation.ItemAdded, StateAt(35m), now);

        Assert.Equal(now, result.After.LastPaceEmissionAt);
        Assert.Equal(5, result.After.ItemCountAtLastPaceEmission);
    }

    [Fact]
    public void PaceProjection_CrossingHasPriorityOverPace()
    {
        // Mesma mutação cruza 50% e tem projeção disponível → cruzamento vence.
        var result = Evaluate(
            Snapshot(55m, 5), CartMutation.ItemAdded, StateAt(40m),
            now: SessionStart.AddMinutes(10));

        Assert.Equal(StatusCodes.BudgetHalf, result.Message?.Code);
    }

    // ----- OCR (F1) -----

    [Fact]
    public void ItemAddedOcr_HighConfidence_EmitsItemAddedOcr()
    {
        var result = StatusMessageEvaluator.Evaluate(new EvaluationInput(
            BudgetAlertState.Initial,
            Snapshot(8.99m),
            CartMutation.ItemAddedOcr,
            SessionStart.AddMinutes(1),
            OcrConfidence: 0.9m,
            OcrProductName: "Óleo De Soja Liza"));

        Assert.Equal(StatusCodes.ItemAddedOcr, result.Message?.Code);
    }

    [Fact]
    public void ItemAddedOcr_LowConfidence_EmitsReviewMessage()
    {
        var result = StatusMessageEvaluator.Evaluate(new EvaluationInput(
            BudgetAlertState.Initial,
            Snapshot(8.99m),
            CartMutation.ItemAddedOcr,
            SessionStart.AddMinutes(1),
            OcrConfidence: 0.7m,
            OcrProductName: "Óleo De Soja Liza"));

        Assert.Equal(StatusCodes.ItemAddedOcrReview, result.Message?.Code);
    }

    [Fact]
    public void ItemAddedOcr_WithoutBudget_StillEmitsOcrMessage()
    {
        var result = StatusMessageEvaluator.Evaluate(new EvaluationInput(
            BudgetAlertState.Initial,
            Snapshot(8.99m, budget: null),
            CartMutation.ItemAddedOcr,
            SessionStart.AddMinutes(1),
            OcrConfidence: 0.95m,
            OcrProductName: "Arroz"));

        Assert.Equal(StatusCodes.ItemAddedOcr, result.Message?.Code);
    }

    [Fact]
    public void ItemAddedOcr_WhenBudgetExceeded_PrefersExceededMessage()
    {
        var result = StatusMessageEvaluator.Evaluate(new EvaluationInput(
            BudgetAlertState.Initial,
            Snapshot(110m, budget: 100m),
            CartMutation.ItemAddedOcr,
            SessionStart.AddMinutes(1),
            OcrConfidence: 0.95m,
            OcrProductName: "Arroz"));

        Assert.Equal(StatusCodes.BudgetExceeded, result.Message?.Code);
    }
}
