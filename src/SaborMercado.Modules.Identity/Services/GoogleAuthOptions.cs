namespace SaborMercado.Modules.Identity.Services;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    public string ClientId { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId);
}

public sealed record GoogleTokenPayload(string SubjectId, string Email, string? Name);

public interface IGoogleIdTokenValidator
{
    Task<GoogleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
