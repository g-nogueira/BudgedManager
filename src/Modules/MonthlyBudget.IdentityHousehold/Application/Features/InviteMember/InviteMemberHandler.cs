using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.InviteMember;
public sealed class InviteMemberHandler : IRequestHandler<InviteMemberCommand, InviteMemberResult>
{
    private readonly IHouseholdRepository _households;
    private readonly IInvitationRepository _invitations;
    private readonly IEmailService _email;
    public InviteMemberHandler(IHouseholdRepository households, IInvitationRepository invitations, IEmailService email)
    { _households = households; _invitations = invitations; _email = email; }
    public async Task<InviteMemberResult> Handle(InviteMemberCommand cmd, CancellationToken ct)
    {
        var household = await _households.FindByIdAsync(cmd.HouseholdId, ct) ?? throw new HouseholdNotFoundException(cmd.HouseholdId);
        // INV-H1: prevent inviting if already full
        if (household.Members.Count >= 2) throw new HouseholdFullException();
        var invitation = Invitation.Create(cmd.HouseholdId, cmd.PartnerEmail);
        await _invitations.SaveAsync(invitation, ct);
        await _email.SendInvitationAsync(cmd.PartnerEmail, household.Name, invitation.Token, ct);
        return new InviteMemberResult(invitation.InvitationId, invitation.Token);
    }
}
