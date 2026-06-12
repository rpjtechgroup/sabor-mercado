namespace SaborMercado.Modules.Recognition.Data;

public sealed class RecognitionLog
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool Succeeded { get; set; }

    public string? FailureReason { get; set; }

    public int LatencyMs { get; set; }

    public string? ClientKey { get; set; }
}
