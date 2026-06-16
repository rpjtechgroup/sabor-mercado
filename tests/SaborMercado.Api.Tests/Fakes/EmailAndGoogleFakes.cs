using SaborMercado.Infrastructure.Email;
using SaborMercado.Modules.Identity.Services;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Api.Tests.Fakes;

public sealed class CapturingEmailSender : IEmailSender
{
    public EmailMessage? LastMessage { get; private set; }

    public int SendCount { get; private set; }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        LastMessage = message;
        SendCount++;
        return Task.CompletedTask;
    }
}

public sealed class StubGoogleIdTokenValidator : IGoogleIdTokenValidator
{
    public Task<GoogleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (idToken.StartsWith("valid:", StringComparison.Ordinal))
        {
            var parts = idToken.Split(':', 3);
            if (parts.Length == 3)
            {
                return Task.FromResult(new GoogleTokenPayload(parts[2], parts[1], "Test User"));
            }
        }

        throw new AuthException(AuthErrorCodes.GoogleAuthFailed, "Token do Google inválido.");
    }
}
