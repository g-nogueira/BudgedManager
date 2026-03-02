using MediatR;
namespace MonthlyBudget.IdentityHousehold.Application.Features.InviteMember;
public sealed record InviteMemberCommand(Guid HouseholdId, Guid InvitingUserId, string PartnerEmail) : IRequest<InviteMemberResult>;
public sealed record InviteMemberResult(Guid InvitationId, string Token);
