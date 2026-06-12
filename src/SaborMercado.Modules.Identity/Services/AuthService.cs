using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaborMercado.Modules.Identity.Data;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Modules.Identity.Services;

public sealed class AuthService(
    IdentityDbContext db,
    JwtTokenService tokens,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider clock)
{
    private readonly PasswordHasher<UserAccount> _hasher = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (email.Length < 5 || !email.Contains('@'))
        {
            throw new AuthException(AuthErrorCodes.WeakPassword, "E-mail inválido.");
        }

        if (request.Password.Length < 8)
        {
            throw new AuthException(AuthErrorCodes.WeakPassword, "Senha deve ter ao menos 8 caracteres.");
        }

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new AuthException(AuthErrorCodes.EmailAlreadyRegistered, "E-mail já cadastrado.");
        }

        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            PseudonymId = Guid.NewGuid(),
            CreatedAt = clock.GetUtcNow(),
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new AuthException(AuthErrorCodes.InvalidCredentials, "E-mail ou senha incorretos.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new AuthException(AuthErrorCodes.InvalidCredentials, "Refresh token inválido.");
        }

        var hash = HashToken(request.RefreshToken);
        var now = clock.GetUtcNow();
        var candidates = await db.RefreshTokens
            .Where(t => t.TokenHash == hash)
            .ToListAsync(cancellationToken);
        var stored = candidates.FirstOrDefault(t => t.ExpiresAt > now);

        if (stored is null)
        {
            throw new AuthException(AuthErrorCodes.InvalidCredentials, "Refresh token inválido ou expirado.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken);
        if (user is null)
        {
            throw new AuthException(AuthErrorCodes.InvalidCredentials, "Conta não encontrada.");
        }

        db.RefreshTokens.Remove(stored);
        await db.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        await db.RefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        var deleted = await db.Users.Where(u => u.Id == userId).ExecuteDeleteAsync(cancellationToken);
        if (deleted == 0)
        {
            throw new AuthException(AuthErrorCodes.InvalidCredentials, "Conta não encontrada.");
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(UserAccount user, CancellationToken cancellationToken)
    {
        var response = tokens.CreateTokens(user);
        await StoreRefreshTokenAsync(user.Id, response.RefreshToken, cancellationToken);
        return response;
    }

    private async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        var settings = jwtOptions.Value;
        var now = clock.GetUtcNow();
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = now.AddDays(settings.RefreshTokenDays),
            CreatedAt = now,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}

public sealed class AuthException(string code, string detail) : Exception(detail)
{
    public string Code { get; } = code;
}
