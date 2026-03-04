using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.IdentityHousehold.Infrastructure.Events;
public sealed class MediatRHouseholdEventPublisher : IHouseholdEventPublisher
{
    private readonly IPublisher _publisher;
    public MediatRHouseholdEventPublisher(IPublisher publisher) { _publisher = publisher; }
    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        if (domainEvent is INotification notification)
            await _publisher.Publish(notification, ct);
    }
}

