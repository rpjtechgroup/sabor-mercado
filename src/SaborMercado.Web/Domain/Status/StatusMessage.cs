namespace SaborMercado.Web.Domain.Status;

/// <summary>
/// Mensagem determinística emitida pelo avaliador. O código é o contrato
/// (catálogo); os argumentos chegam formatados em pt-BR, prontos para o
/// texto localizado.
/// </summary>
public sealed record StatusMessage(
    string Code,
    IReadOnlyDictionary<string, string> Args)
{
    public static StatusMessage Create(string code, params (string Key, string Value)[] args) =>
        new(code, args.ToDictionary(a => a.Key, a => a.Value));
}
