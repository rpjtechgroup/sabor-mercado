namespace SaborMercado.Modules.Identity.Data;

public sealed class UserAccount
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Guid PseudonymId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
