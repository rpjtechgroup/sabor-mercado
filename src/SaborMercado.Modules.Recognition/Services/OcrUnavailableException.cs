namespace SaborMercado.Modules.Recognition.Services;

public sealed class OcrUnavailableException(string reason) : Exception(reason)
{
    public string Reason { get; } = reason;
}
