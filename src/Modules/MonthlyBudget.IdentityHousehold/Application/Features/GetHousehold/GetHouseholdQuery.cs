using MediatR;
namespace MonthlyBudget.IdentityHousehold.Application.Features.GetHousehold;
public sealed record GetHouseholdQuery(Guid HouseholdId, Guid RequestingUserId) : IRequest<HouseholdDto?>;
public sealed record HouseholdDto(Guid HouseholdId, string Name, IReadOnlyList<MemberDto> Members);
public sealed record MemberDto(Guid UserId, string Role);

