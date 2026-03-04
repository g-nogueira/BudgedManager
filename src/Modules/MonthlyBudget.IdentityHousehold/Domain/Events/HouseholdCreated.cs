using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.IdentityHousehold.Domain.Events;
public sealed class HouseholdCreated : DomainEventBase
{
    public Guid HouseholdId { get; }
    public Guid OwnerId { get; }
    public HouseholdCreated(Guid householdId, Guid ownerId) { HouseholdId = householdId; OwnerId = ownerId; }
}

