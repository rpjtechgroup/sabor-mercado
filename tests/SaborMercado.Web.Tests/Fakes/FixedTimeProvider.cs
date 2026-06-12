namespace SaborMercado.Web.Tests.Fakes;

/// <summary>Relógio controlável para testes determinísticos.</summary>
public sealed class FixedTimeProvider(DateTimeOffset start) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = start;

    public override DateTimeOffset GetUtcNow() => Now;

    public void Advance(TimeSpan delta) => Now += delta;
}
