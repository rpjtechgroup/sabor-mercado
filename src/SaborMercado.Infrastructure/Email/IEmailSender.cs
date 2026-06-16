namespace SaborMercado.Infrastructure.Email;

public sealed class EmailMessage
{
    public required string ToAddress { get; init; }

    public string? ToName { get; init; }

    public string? ReplyToAddress { get; init; }

    public string? ReplyToName { get; init; }

    public required string Subject { get; init; }

    public required string TextBody { get; init; }

    public string? HtmlBody { get; init; }
}

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
