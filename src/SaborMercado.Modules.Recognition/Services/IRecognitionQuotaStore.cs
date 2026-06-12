namespace SaborMercado.Modules.Recognition.Services;

public interface IRecognitionQuotaStore
{
    bool TryConsumeGlobal(DateOnly day, int limit);

    bool TryConsumeClient(DateOnly day, string clientKey, int limit);
}
