using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Events;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.InviteMember;
public sealed class InviteMemberHandler : IRequestHandler<InviteMemberCommand, InviteMemberResult>
{
    private readonly IHouseholdRepository _households;
    private readonly IInvitationRepository _invitations;
    private readonly IEmailService _email;
    private readonly IHouseholdEventPublisher _events;
    public InviteMemberHandler(IHouseholdRepository households, IInvitationRepository invitations, IEmailService email, IHouseholdEventPublisher events)
    { _households = households; _invitations = invitations; _email = email; _events = events; }
    public async Task<InviteMemberResult> Handle(InviteMemberCommand cmd, CancellationToken ct)
    {
        var household = await _households.FindByIdAsync(cmd.HouseholdId, ct) ?? throw new HouseholdNotFoundException(cmd.HouseholdId);
        // INV-H2: Only the OWNER may invite — enforced in domain
        household.AuthorizeInvite(cmd.InvitingUserId);
        // INV-H1: prevent inviting if already full
        if (household.Members.Count >= 2) throw new HouseholdFullException();
        // INV-H4: only one pending invitation per household at a time (domain-enforced)
        var pending = await _invitations.FindPendingByHouseholdAsync(cmd.HouseholdId, ct);
        household.GuardPendingInvitation(pending != null);
        var invitation = Invitation.Create(cmd.HouseholdId, cmd.PartnerEmail);
        await _invitations.SaveAsync(invitation, ct);
        await _email.SendInvitationAsync(cmd.PartnerEmail, household.Name, invitation.Token, ct);
        await _events.PublishAsync(new MemberInvited(cmd.HouseholdId, invitation.InvitationId, cmd.PartnerEmail), ct);
        return new InviteMemberResult(invitation.InvitationId);
    }
}
