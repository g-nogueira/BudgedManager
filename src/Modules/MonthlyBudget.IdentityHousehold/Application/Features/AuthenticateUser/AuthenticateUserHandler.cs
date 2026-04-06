using MediatR;
using Microsoft.Extensions.Configuration;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;
public sealed class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly IUserRepository _users;
    private readonly IHouseholdRepository _households;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly int _refreshTokenExpiryDays;
    public AuthenticateUserHandler(
        IUserRepository users,
        IHouseholdRepository households,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher,
        ITokenService tokens,
        IConfiguration config)
    {
        _users = users;
        _households = households;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
        _tokens = tokens;
        _refreshTokenExpiryDays = int.TryParse(config["Jwt:RefreshTokenExpiryDays"], out var days) ? days : 30;
    }
    public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(cmd.Email, ct) ?? throw new InvalidCredentialsException();
        if (!_hasher.Verify(cmd.Password, user.PasswordHash)) throw new InvalidCredentialsException();
        // Find household membership for this user (null = not yet in a household)
        var household = await _households.FindByMemberIdAsync(user.UserId, ct);
        var access = _tokens.GenerateAccessToken(user.UserId, user.Email, user.DisplayName, household?.HouseholdId);
        var refresh = _tokens.GenerateRefreshToken();

        var hash = _tokens.HashToken(refresh);
        var entry = RefreshTokenEntry.Create(user.UserId, hash, DateTime.UtcNow.AddDays(_refreshTokenExpiryDays));
        await _refreshTokens.SaveAsync(entry, ct);

        return new AuthenticateUserResult(access, refresh);
    }
}
