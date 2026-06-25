namespace SaborMercado.Shared.Rewards;

public sealed class RewardsException(string code, string detail) : Exception(detail)
{
    public string Code { get; } = code;
}
