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
        // Find household membership (if any)
        // For MVP: scan all households would be expensive; householdId stored on user claim
        // Here we return null and let the front-end redirect to household creation
        var access = _tokens.GenerateAccessToken(user.UserId, user.Email, user.DisplayName, null);
        var refresh = _tokens.GenerateRefreshToken();
        return new AuthenticateUserResult(access, refresh, user.UserId, null);
    }
}
