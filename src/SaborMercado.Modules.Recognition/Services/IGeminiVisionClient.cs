using SaborMercado.Shared.Recognition;

namespace SaborMercado.Modules.Recognition.Services;

public interface IGeminiVisionClient
{
    Task<RecognitionResultDto> RecognizeAsync(
        ReadOnlyMemory<byte> imageBytes,
        string contentType,
        CancellationToken cancellationToken);
}
