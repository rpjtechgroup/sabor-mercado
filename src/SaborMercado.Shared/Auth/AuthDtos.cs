namespace SaborMercado.Shared.Auth;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    Guid PseudonymId);

public sealed record RefreshRequest(string RefreshToken);

public sealed record GoogleLoginRequest(string IdToken);
