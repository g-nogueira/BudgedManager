using MediatR;
namespace MonthlyBudget.IdentityHousehold.Application.Features.RegisterUser;
public sealed record RegisterUserCommand(string Email, string DisplayName, string Password) : IRequest<RegisterUserResult>;
public sealed record RegisterUserResult(Guid UserId);
