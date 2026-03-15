using MediatR;
namespace MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;
public sealed record AuthenticateUserCommand(string Email, string Password) : IRequest<AuthenticateUserResult>;
public sealed record AuthenticateUserResult(string AccessToken, string RefreshToken);
