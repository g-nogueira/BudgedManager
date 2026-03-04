using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.IdentityHousehold.Domain.Events;
public sealed class MemberInvited : DomainEventBase
{
    public Guid HouseholdId { get; }
    public Guid InvitationId { get; }
    public string InvitedEmail { get; }
    public MemberInvited(Guid householdId, Guid invitationId, string invitedEmail)
    {
        HouseholdId = householdId; InvitationId = invitationId; InvitedEmail = invitedEmail;
    }
}

