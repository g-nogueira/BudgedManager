using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.RegisterUser;
public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _users; private readonly IPasswordHasher _hasher;
    public RegisterUserHandler(IUserRepository users, IPasswordHasher hasher) { _users = users; _hasher = hasher; }
    public async Task<RegisterUserResult> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        if (await _users.ExistsWithEmailAsync(cmd.Email, ct)) throw new DuplicateEmailException(cmd.Email);
        var user = User.Create(cmd.Email, cmd.DisplayName, _hasher.Hash(cmd.Password));
        await _users.SaveAsync(user, ct);
        return new RegisterUserResult(user.UserId, user.Email, user.DisplayName);
    }
}
