using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.IdentityHousehold.Domain.Events;
public sealed class MemberJoined : DomainEventBase
{
    public Guid HouseholdId { get; }
    public Guid UserId { get; }
    public string Role { get; }
    public MemberJoined(Guid householdId, Guid userId, string role)
    {
        HouseholdId = householdId; UserId = userId; Role = role;
    }
}

