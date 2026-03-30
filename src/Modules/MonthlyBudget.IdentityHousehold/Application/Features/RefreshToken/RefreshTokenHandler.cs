using MediatR;
using Microsoft.Extensions.Configuration;
using MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;

namespace MonthlyBudget.IdentityHousehold.Application.Features.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthenticateUserResult>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly IHouseholdRepository _households;
    private readonly ITokenService _tokens;
    private readonly int _refreshTokenExpiryDays;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IHouseholdRepository households,
        ITokenService tokens,
        IConfiguration config)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _households = households;
        _tokens = tokens;
        _refreshTokenExpiryDays = int.TryParse(config["Jwt:RefreshTokenExpiryDays"], out var days) ? days : 30;
    }

    public async Task<AuthenticateUserResult> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var tokenHash = _tokens.HashToken(cmd.RefreshToken);
        var existing = await _refreshTokens.FindByTokenHashAsync(tokenHash, ct);
        if (existing is null || existing.IsExpired())
            throw new InvalidRefreshTokenException();

        await _refreshTokens.DeleteAsync(existing, ct);

        var user = await _users.FindByIdAsync(existing.UserId, ct);
        if (user is null)
            throw new InvalidRefreshTokenException();

        var household = await _households.FindByMemberIdAsync(user.UserId, ct);
        var accessToken = _tokens.GenerateAccessToken(user.UserId, user.Email, user.DisplayName, household?.HouseholdId);
        var refreshToken = _tokens.GenerateRefreshToken();
        var newHash = _tokens.HashToken(refreshToken);
        var newEntry = RefreshTokenEntry.Create(user.UserId, newHash, DateTime.UtcNow.AddDays(_refreshTokenExpiryDays));

        await _refreshTokens.SaveAsync(newEntry, ct);

        return new AuthenticateUserResult(accessToken, refreshToken);
    }
}