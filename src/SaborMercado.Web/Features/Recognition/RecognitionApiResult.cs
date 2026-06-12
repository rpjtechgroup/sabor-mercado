using SaborMercado.Web.Contracts.Recognition;

namespace SaborMercado.Web.Features.Recognition;

public sealed record RecognitionApiResult(
    bool IsSuccess,
    bool IsUnavailable,
    bool IsMissingApiKey,
    RecognitionResultDto? Result,
    RecognitionResultDto? PartialResult)
{
    public static RecognitionApiResult Success(RecognitionResultDto result) =>
        new(true, false, false, result, null);

    public static RecognitionApiResult Unavailable(RecognitionResultDto? partial = null) =>
        new(false, true, false, null, partial);

    public static RecognitionApiResult MissingApiKey() =>
        new(false, false, true, null, null);
}
