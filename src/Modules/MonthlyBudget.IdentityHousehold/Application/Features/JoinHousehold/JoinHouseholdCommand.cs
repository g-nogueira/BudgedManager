using MediatR;
namespace MonthlyBudget.IdentityHousehold.Application.Features.JoinHousehold;
public sealed record JoinHouseholdCommand(Guid UserId, string Token) : IRequest<JoinHouseholdResult>;
public sealed record JoinHouseholdResult(Guid HouseholdId);
