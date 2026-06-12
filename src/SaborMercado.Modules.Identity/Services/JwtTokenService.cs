using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaborMercado.Modules.Identity.Data;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Modules.Identity.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock)
{
    public AuthResponse CreateTokens(UserAccount user)
    {
        var settings = options.Value;
        var expiresAt = clock.GetUtcNow().AddMinutes(settings.AccessTokenMinutes);
        var accessToken = CreateJwt(user, expiresAt, settings);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

        return new AuthResponse(accessToken, refreshToken, expiresAt, user.PseudonymId);
    }

    private string CreateJwt(UserAccount user, DateTimeOffset expiresAt, JwtOptions settings)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("pseudonym_id", user.PseudonymId.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
