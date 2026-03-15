using MediatR;
namespace MonthlyBudget.IdentityHousehold.Application.Features.CreateHousehold;
public sealed record CreateHouseholdCommand(Guid UserId, string Name) : IRequest<CreateHouseholdResult>;
public sealed record CreateHouseholdResult(Guid HouseholdId);
