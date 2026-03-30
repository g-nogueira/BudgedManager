using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;

namespace MonthlyBudget.IdentityHousehold.Application.Features.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthenticateUserResult>;