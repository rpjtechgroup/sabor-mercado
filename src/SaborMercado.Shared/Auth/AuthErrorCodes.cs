namespace SaborMercado.Shared.Auth;

public static class AuthErrorCodes
{
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
    public const string WeakPassword = "WEAK_PASSWORD";
    public const string GoogleAuthFailed = "GOOGLE_AUTH_FAILED";
    public const string GoogleAuthNotConfigured = "GOOGLE_AUTH_NOT_CONFIGURED";
}
