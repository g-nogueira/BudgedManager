using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;
public sealed class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly IUserRepository _users;
    private readonly IHouseholdRepository _households;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    public AuthenticateUserHandler(IUserRepository users, IHouseholdRepository households, IPasswordHasher hasher, ITokenService tokens)
    { _users = users; _households = households; _hasher = hasher; _tokens = tokens; }
    public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(cmd.Email, ct) ?? throw new InvalidCredentialsException();
        if (!_hasher.Verify(cmd.Password, user.PasswordHash)) throw new InvalidCredentialsException();
        // Find household membership for this user (null = not yet in a household)
        var household = await _households.FindByMemberIdAsync(user.UserId, ct);
        var access = _tokens.GenerateAccessToken(user.UserId, user.Email, user.DisplayName, household?.HouseholdId);
        var refresh = _tokens.GenerateRefreshToken();
        return new AuthenticateUserResult(access, refresh);
    }
}
