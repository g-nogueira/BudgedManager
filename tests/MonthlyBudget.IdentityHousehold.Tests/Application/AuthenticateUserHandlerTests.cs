using Microsoft.Extensions.Configuration;
using MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;

namespace MonthlyBudget.IdentityHousehold.Tests.Application;

public sealed class AuthenticateUserHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed");
        var sut = BuildSut(user: user, passwordValid: true);

        var result = await sut.Handle(new AuthenticateUserCommand("alice@example.com", "secret"), CancellationToken.None);

        Assert.StartsWith("access-", result.AccessToken);
        Assert.Equal("refresh-1", result.RefreshToken);
    }

    [Fact]
    public async Task Handle_ValidCredentials_SavesRefreshTokenToRepository()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed");
        var refreshRepo = new FakeRefreshTokenRepository();
        var sut = BuildSut(user: user, passwordValid: true, refreshRepo: refreshRepo);

        await sut.Handle(new AuthenticateUserCommand("alice@example.com", "secret"), CancellationToken.None);

        Assert.Single(refreshRepo.SavedEntries);
        Assert.Equal(user.UserId, refreshRepo.SavedEntries[0].UserId);
    }

    [Fact]
    public async Task Handle_ValidCredentials_StoresHashedRefreshToken()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed");
        var refreshRepo = new FakeRefreshTokenRepository();
        var tokens = new FakeTokenService();
        var sut = BuildSut(user: user, passwordValid: true, refreshRepo: refreshRepo, tokenService: tokens);

        await sut.Handle(new AuthenticateUserCommand("alice@example.com", "secret"), CancellationToken.None);

        // FakeTokenService hashes as "hash:<token>", so the stored hash should be "hash:refresh-1"
        Assert.Equal("hash:refresh-1", refreshRepo.SavedEntries[0].TokenHash);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsInvalidCredentialsException()
    {
        var sut = BuildSut(user: null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.Handle(new AuthenticateUserCommand("unknown@example.com", "secret"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsInvalidCredentialsException()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed");
        var sut = BuildSut(user: user, passwordValid: false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.Handle(new AuthenticateUserCommand("alice@example.com", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserWithoutHousehold_ReturnsTokensSuccessfully()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed");
        var sut = BuildSut(user: user, passwordValid: true, household: null);

        var result = await sut.Handle(new AuthenticateUserCommand("alice@example.com", "secret"), CancellationToken.None);

        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }

    [Fact]
    public async Task Handle_MissingRefreshTokenExpiryConfig_DefaultsTo30Days()
    {
        var user = User.Create("alice@example.com", "Alice", "hashed");
        var refreshRepo = new FakeRefreshTokenRepository();
        var config = new ConfigurationBuilder().Build(); // empty config
        var sut = new AuthenticateUserHandler(
            new FakeUserRepository(user),
            new FakeHouseholdRepository(null),
            refreshRepo,
            new FakePasswordHasher(valid: true),
            new FakeTokenService(),
            config);

        var before = DateTime.UtcNow;
        await sut.Handle(new AuthenticateUserCommand("alice@example.com", "secret"), CancellationToken.None);
        var after = DateTime.UtcNow;

        var expiresAt = refreshRepo.SavedEntries[0].ExpiresAt;
        Assert.InRange(expiresAt, before.AddDays(29), after.AddDays(31));
    }

    private static AuthenticateUserHandler BuildSut(
        User? user = null,
        bool passwordValid = true,
        Household? household = null,
        FakeRefreshTokenRepository? refreshRepo = null,
        FakeTokenService? tokenService = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:RefreshTokenExpiryDays"] = "30" })
            .Build();

        return new AuthenticateUserHandler(
            new FakeUserRepository(user),
            new FakeHouseholdRepository(household),
            refreshRepo ?? new FakeRefreshTokenRepository(),
            new FakePasswordHasher(valid: passwordValid),
            tokenService ?? new FakeTokenService(),
            config);
    }

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

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        private readonly bool _valid;
        public FakePasswordHasher(bool valid) => _valid = valid;
        public string Hash(string password) => "hashed";
        public bool Verify(string password, string hash) => _valid;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? _user;
        public FakeUserRepository(User? user) => _user = user;

        public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(_user?.UserId == userId ? _user : null);

        public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(_user?.Email == email.ToLowerInvariant() ? _user : null);

        public Task SaveAsync(User user, CancellationToken ct = default) => Task.CompletedTask;

        public Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(_user != null);
    }

    private sealed class FakeHouseholdRepository : IHouseholdRepository
    {
        private readonly Household? _household;
        public FakeHouseholdRepository(Household? household) => _household = household;

        public Task<Household?> FindByIdAsync(Guid householdId, CancellationToken ct = default)
            => Task.FromResult(_household?.HouseholdId == householdId ? _household : null);

        public Task<Household?> FindByMemberIdAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(_household?.Members.Any(m => m.UserId == userId) == true ? _household : null);

        public Task SaveAsync(Household household, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public List<RefreshTokenEntry> SavedEntries { get; } = new();

        public Task<RefreshTokenEntry?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default)
            => Task.FromResult<RefreshTokenEntry?>(null);

        public Task SaveAsync(RefreshTokenEntry refreshToken, CancellationToken ct = default)
        {
            SavedEntries.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(RefreshTokenEntry refreshToken, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAllByUserIdAsync(Guid userId, CancellationToken ct = default) => Task.CompletedTask;
        public Task ReplaceAsync(RefreshTokenEntry oldToken, RefreshTokenEntry newToken, CancellationToken ct = default) => Task.CompletedTask;
    }
}