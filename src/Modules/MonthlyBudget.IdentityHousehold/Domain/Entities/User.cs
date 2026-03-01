namespace MonthlyBudget.IdentityHousehold.Domain.Entities;
public class User
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    private User() { }
    public static User Create(string email, string displayName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("DisplayName required.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash required.", nameof(passwordHash));
        return new User { UserId = Guid.NewGuid(), Email = email.Trim().ToLowerInvariant(), DisplayName = displayName.Trim(), PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow };
    }
}
