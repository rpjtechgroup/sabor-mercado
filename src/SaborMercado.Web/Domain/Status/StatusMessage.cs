namespace SaborMercado.Web.Domain.Status;

public sealed record StatusMessage(
    string Code,
    IReadOnlyDictionary<string, string> Args)
{
    public static StatusMessage Create(string code, params (string Key, string Value)[] args) =>
        new(code, args.ToDictionary(a => a.Key, a => a.Value));
}
