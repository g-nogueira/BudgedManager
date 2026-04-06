namespace MonthlyBudget.IdentityHousehold.Domain.Entities;

public sealed class RefreshTokenEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RefreshTokenEntry() { }

    public static RefreshTokenEntry Create(Guid userId, string tokenHash, DateTime expiresAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("TokenHash is required.", nameof(tokenHash));

        return new RefreshTokenEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsExpired() => ExpiresAt <= DateTime.UtcNow;
}