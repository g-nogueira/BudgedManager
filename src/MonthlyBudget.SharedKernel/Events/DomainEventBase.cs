using MediatR;

namespace MonthlyBudget.SharedKernel.Events;

public abstract class DomainEventBase : IDomainEvent, INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
