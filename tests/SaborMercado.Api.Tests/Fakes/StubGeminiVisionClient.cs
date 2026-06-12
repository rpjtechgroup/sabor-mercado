using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Recognition;

namespace SaborMercado.Api.Tests.Fakes;

public sealed class StubGeminiVisionClient : IGeminiVisionClient
{
    public Task<RecognitionResultDto> RecognizeAsync(
        ReadOnlyMemory<byte> imageBytes,
        string contentType,
        CancellationToken cancellationToken) =>
        Task.FromResult(new RecognitionResultDto(
            "Óleo De Soja Liza",
            "Liza",
            900m,
            "ml",
            8.99m,
            null,
            0.93m,
            "ÓLEO DE SOJA LIZA 900ML R$ 8,99"));
}
