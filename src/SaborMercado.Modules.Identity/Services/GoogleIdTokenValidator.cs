using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Modules.Identity.Services;

public sealed class GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options) : IGoogleIdTokenValidator
{
    public async Task<GoogleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new AuthException(AuthErrorCodes.GoogleAuthNotConfigured, "Login com Google não configurado.");
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new AuthException(AuthErrorCodes.GoogleAuthFailed, "Token do Google inválido.");
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [clientId],
                });

            if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
            {
                throw new AuthException(AuthErrorCodes.GoogleAuthFailed, "Token do Google incompleto.");
            }

            return new GoogleTokenPayload(payload.Subject, payload.Email.Trim().ToLowerInvariant(), payload.Name);
        }
        catch (AuthException)
        {
            throw;
        }
        catch (InvalidJwtException)
        {
            throw new AuthException(AuthErrorCodes.GoogleAuthFailed, "Token do Google inválido ou expirado.");
        }
        catch (Exception)
        {
            throw new AuthException(AuthErrorCodes.GoogleAuthFailed, "Token do Google inválido ou expirado.");
        }
    }
}
