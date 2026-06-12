using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaborMercado.Modules.Recognition.Data;
using SaborMercado.Shared.Recognition;

namespace SaborMercado.Modules.Recognition.Services;

public sealed class ShelfLabelRecognitionService(
    IGeminiVisionClient gemini,
    RecognitionDbContext db,
    IRecognitionQuotaStore quota,
    IOptions<RecognitionOptions> options,
    TimeProvider clock,
    ILogger<ShelfLabelRecognitionService> logger)
{
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    public async Task<RecognitionResultDto> RecognizeAsync(
        Stream imageStream,
        string contentType,
        string clientKey,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new InvalidDataException("Unsupported image content type.");
        }

        using var memory = new MemoryStream();
        await imageStream.CopyToAsync(memory, cancellationToken);
        if (memory.Length > settings.MaxUploadBytes)
        {
            throw new InvalidDataException("Image exceeds maximum size.");
        }

        if (memory.Length == 0)
        {
            throw new InvalidDataException("Empty image payload.");
        }

        var day = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        if (!quota.TryConsumeGlobal(day, settings.DailyGlobalQuota) ||
            !quota.TryConsumeClient(day, clientKey, settings.PerClientDailyQuota))
        {
            throw new OcrUnavailableException("Daily OCR quota exhausted");
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await gemini.RecognizeAsync(memory.ToArray(), contentType, cancellationToken);
            await LogAsync(true, null, (int)sw.ElapsedMilliseconds, clientKey, cancellationToken);
            return result;
        }
        catch (OcrUnavailableException ex)
        {
            await LogAsync(false, ex.Reason, (int)sw.ElapsedMilliseconds, clientKey, cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Recognition failed for client {ClientKey}", clientKey);
            await LogAsync(false, ex.GetType().Name, (int)sw.ElapsedMilliseconds, clientKey, cancellationToken);
            throw new OcrUnavailableException("Recognition service error");
        }
    }

    private async Task LogAsync(
        bool succeeded,
        string? failureReason,
        int latencyMs,
        string clientKey,
        CancellationToken cancellationToken)
    {
        db.RecognitionLogs.Add(new RecognitionLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = clock.GetUtcNow(),
            Succeeded = succeeded,
            FailureReason = failureReason,
            LatencyMs = latencyMs,
            ClientKey = clientKey,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
