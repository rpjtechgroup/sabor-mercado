using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaborMercado.Modules.Identity.Data;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Modules.Identity.Services;

public sealed class GoogleAuthService(
    IdentityDbContext db,
    AuthService auth,
    IGoogleIdTokenValidator validator,
    IOptions<GoogleAuthOptions> googleOptions,
    TimeProvider clock)
{
    public async Task<AuthResponse> LoginWithGoogleAsync(
        GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!googleOptions.Value.IsConfigured)
        {
            throw new AuthException(AuthErrorCodes.GoogleAuthNotConfigured, "Login com Google não configurado.");
        }

        var payload = await validator.ValidateAsync(request.IdToken, cancellationToken);
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.GoogleSubjectId == payload.SubjectId,
            cancellationToken);

        if (user is null)
        {
            user = await db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email, cancellationToken);
            if (user is not null)
            {
                if (!string.IsNullOrWhiteSpace(user.GoogleSubjectId) &&
                    !string.Equals(user.GoogleSubjectId, payload.SubjectId, StringComparison.Ordinal))
                {
                    throw new AuthException(AuthErrorCodes.GoogleAuthFailed, "Conta já vinculada a outro Google.");
                }

                user.GoogleSubjectId = payload.SubjectId;
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                user = new UserAccount
                {
                    Id = Guid.NewGuid(),
                    Email = payload.Email,
                    GoogleSubjectId = payload.SubjectId,
                    PseudonymId = Guid.NewGuid(),
                    PasswordHash = string.Empty,
                    CreatedAt = clock.GetUtcNow(),
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return await auth.SignInAsync(user, cancellationToken);
    }
}
