using MonthlyBudget.IdentityHousehold.Domain.Entities;

namespace MonthlyBudget.IdentityHousehold.Tests.Domain;

public sealed class RefreshTokenEntryTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsEntry()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(30);

        var entry = RefreshTokenEntry.Create(userId, "abc123", expiresAt);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(userId, entry.UserId);
        Assert.Equal("abc123", entry.TokenHash);
        Assert.Equal(expiresAt, entry.ExpiresAt);
    }

    [Fact]
    public void Create_SetsCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entry = RefreshTokenEntry.Create(Guid.NewGuid(), "abc123", DateTime.UtcNow.AddDays(30));
        var after = DateTime.UtcNow;

        Assert.InRange(entry.CreatedAt, before, after);
    }

    [Fact]
    public void IsExpired_PastDate_ReturnsTrue()
    {
        var entry = RefreshTokenEntry.Create(Guid.NewGuid(), "abc123", DateTime.UtcNow.AddMinutes(-1));

        Assert.True(entry.IsExpired());
    }

    [Fact]
    public void IsExpired_FutureDate_ReturnsFalse()
    {
        var entry = RefreshTokenEntry.Create(Guid.NewGuid(), "abc123", DateTime.UtcNow.AddMinutes(1));

        Assert.False(entry.IsExpired());
    }

    [Fact]
    public void Create_EmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            RefreshTokenEntry.Create(Guid.Empty, "abc123", DateTime.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Create_EmptyTokenHash_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            RefreshTokenEntry.Create(Guid.NewGuid(), string.Empty, DateTime.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Create_WhitespaceOnlyTokenHash_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            RefreshTokenEntry.Create(Guid.NewGuid(), "   ", DateTime.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Create_EachCallProducesUniqueId()
    {
        var entry1 = RefreshTokenEntry.Create(Guid.NewGuid(), "hash1", DateTime.UtcNow.AddDays(30));
        var entry2 = RefreshTokenEntry.Create(Guid.NewGuid(), "hash2", DateTime.UtcNow.AddDays(30));

        Assert.NotEqual(entry1.Id, entry2.Id);
    }
}