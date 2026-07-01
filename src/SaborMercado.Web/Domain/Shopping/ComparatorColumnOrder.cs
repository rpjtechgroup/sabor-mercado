namespace SaborMercado.Web.Domain.Shopping;

public static class ComparatorColumnOrder
{
    public static IReadOnlyList<ComparatorColumnId> DefaultOrder { get; } =
    [
        ComparatorColumnId.Product,
        ComparatorColumnId.Store,
        ComparatorColumnId.BestPrice,
        ComparatorColumnId.WorstPrice,
        ComparatorColumnId.When
    ];

    public static IReadOnlyList<ComparatorColumnId> Normalize(IReadOnlyList<ComparatorColumnId>? saved)
    {
        if (saved is null || saved.Count == 0)
        {
            return DefaultOrder;
        }

        var result = new List<ComparatorColumnId>(DefaultOrder.Count);
        var seen = new HashSet<ComparatorColumnId>();

        foreach (var column in saved)
        {
            if (seen.Add(column))
            {
                result.Add(column);
            }
        }

        foreach (var column in DefaultOrder)
        {
            if (seen.Add(column))
            {
                result.Add(column);
            }
        }

        return result;
    }

    public static string ToStorageString(IReadOnlyList<ComparatorColumnId> order) =>
        string.Join(',', order);

    public static bool TryParse(string? raw, out IReadOnlyList<ComparatorColumnId> order)
    {
        order = DefaultOrder;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var parsed = new List<ComparatorColumnId>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<ComparatorColumnId>(part, ignoreCase: true, out var column))
            {
                return false;
            }

            parsed.Add(column);
        }

        if (parsed.Count == 0)
        {
            return false;
        }

        order = Normalize(parsed);
        return true;
    }
}
