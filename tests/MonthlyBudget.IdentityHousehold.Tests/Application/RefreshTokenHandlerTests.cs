using Microsoft.Extensions.Configuration;
using MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;
using MonthlyBudget.IdentityHousehold.Application.Features.RefreshToken;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;

namespace MonthlyBudget.IdentityHousehold.Tests.Application;

public sealed class RefreshTokenHandlerTests
{
    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokenPair()
    {
        var user = User.Create("test@example.com", "Alice", "hash");
        var oldRaw = "old-token";
        var oldHash = "hash:old-token";
        var existing = RefreshTokenEntry.Create(user.UserId, oldHash, DateTime.UtcNow.AddMinutes(30));

        var refreshRepo = new FakeRefreshTokenRepository(existing);
        var users = new FakeUserRepository(user);
        var households = new FakeHouseholdRepository(Household.Create("Home", user.UserId));
        var tokens = new FakeTokenService();
        var config = BuildConfig();
        var sut = new RefreshTokenHandler(refreshRepo, users, households, tokens, config);

        var result = await sut.Handle(new RefreshTokenCommand(oldRaw), CancellationToken.None);

        Assert.StartsWith("access-", result.AccessToken);
        Assert.Equal("refresh-1", result.RefreshToken);
    }

    [Fact]
    public async Task Handle_ValidToken_DeletesOldToken()
    {
        var user = User.Create("test@example.com", "Alice", "hash");
        var existing = RefreshTokenEntry.Create(user.UserId, "hash:old-token", DateTime.UtcNow.AddMinutes(30));

        var refreshRepo = new FakeRefreshTokenRepository(existing);
        var sut = new RefreshTokenHandler(
            refreshRepo,
            new FakeUserRepository(user),
            new FakeHouseholdRepository(Household.Create("Home", user.UserId)),
            new FakeTokenService(),
            BuildConfig());

        await sut.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        Assert.Contains(existing.Id, refreshRepo.DeletedIds);
    }

    [Fact]
    public async Task Handle_ValidToken_StoresNewRefreshToken()
    {
        var user = User.Create("test@example.com", "Alice", "hash");
        var existing = RefreshTokenEntry.Create(user.UserId, "hash:old-token", DateTime.UtcNow.AddMinutes(30));

        var refreshRepo = new FakeRefreshTokenRepository(existing);
        var sut = new RefreshTokenHandler(
            refreshRepo,
            new FakeUserRepository(user),
            new FakeHouseholdRepository(Household.Create("Home", user.UserId)),
            new FakeTokenService(),
            BuildConfig());

        await sut.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        Assert.Contains(refreshRepo.SavedEntries, e => e.TokenHash == "hash:refresh-1");
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidRefreshTokenException()
    {
        var user = User.Create("test@example.com", "Alice", "hash");
        var sut = new RefreshTokenHandler(
            new FakeRefreshTokenRepository(),
            new FakeUserRepository(user),
            new FakeHouseholdRepository(Household.Create("Home", user.UserId)),
            new FakeTokenService(),
            BuildConfig());

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            sut.Handle(new RefreshTokenCommand("unknown-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidRefreshTokenException()
    {
        var user = User.Create("test@example.com", "Alice", "hash");
        var expired = RefreshTokenEntry.Create(user.UserId, "hash:old-token", DateTime.UtcNow.AddMinutes(-1));

        var sut = new RefreshTokenHandler(
            new FakeRefreshTokenRepository(expired),
            new FakeUserRepository(user),
            new FakeHouseholdRepository(Household.Create("Home", user.UserId)),
            new FakeTokenService(),
            BuildConfig());

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            sut.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidRefreshTokenException()
    {
        var existing = RefreshTokenEntry.Create(Guid.NewGuid(), "hash:old-token", DateTime.UtcNow.AddMinutes(30));

        var sut = new RefreshTokenHandler(
            new FakeRefreshTokenRepository(existing),
            new FakeUserRepository(null),
            new FakeHouseholdRepository(null),
            new FakeTokenService(),
            BuildConfig());

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            sut.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None));
    }

    private static IConfiguration BuildConfig()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:RefreshTokenExpiryDays"] = "30"
            })
            .Build();

    private sealed class FakeTokenService : ITokenService
    {
        private int _counter;

        public string GenerateAccessToken(Guid userId, string email, string displayName, Guid? householdId)
            => $"access-{userId:N}";

        public string GenerateRefreshToken()
        {
            _counter++;
            return $"refresh-{_counter}";
        }

        public string HashToken(string token) => $"hash:{token}";
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;

        public FakeUserRepository(User? user)
        {
            _user = user;
        }

        public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(_user?.UserId == userId ? _user : null);

        public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult<User?>(null);

        public Task SaveAsync(User user, CancellationToken ct = default) => Task.CompletedTask;

        public Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(false);
    }

    private sealed class FakeHouseholdRepository : IHouseholdRepository
    {
        private readonly Household? _household;

        public FakeHouseholdRepository(Household? household)
        {
            _household = household;
        }

        public Task<Household?> FindByIdAsync(Guid householdId, CancellationToken ct = default)
            => Task.FromResult(_household?.HouseholdId == householdId ? _household : null);

        public Task<Household?> FindByMemberIdAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(_household?.Members.Any(m => m.UserId == userId) == true ? _household : null);

        public Task SaveAsync(Household household, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly List<RefreshTokenEntry> _entries;

        public FakeRefreshTokenRepository(params RefreshTokenEntry[] entries)
        {
            _entries = entries.ToList();
        }

        public List<Guid> DeletedIds { get; } = new();
        public List<RefreshTokenEntry> SavedEntries { get; } = new();

        public Task<RefreshTokenEntry?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default)
            => Task.FromResult(_entries.FirstOrDefault(e => e.TokenHash == tokenHash));

        public Task SaveAsync(RefreshTokenEntry refreshToken, CancellationToken ct = default)
        {
            SavedEntries.Add(refreshToken);
            _entries.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(RefreshTokenEntry refreshToken, CancellationToken ct = default)
        {
            DeletedIds.Add(refreshToken.Id);
            _entries.RemoveAll(e => e.Id == refreshToken.Id);
            return Task.CompletedTask;
        }

        public Task DeleteAllByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            _entries.RemoveAll(e => e.UserId == userId);
            return Task.CompletedTask;
        }
    }
}