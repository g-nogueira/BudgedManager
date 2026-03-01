using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Infrastructure.Events;
public sealed class MediatRBudgetEventPublisher : IBudgetEventPublisher
{
    private readonly IPublisher _publisher;
    public MediatRBudgetEventPublisher(IPublisher publisher) { _publisher = publisher; }
    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        if (domainEvent is INotification notification)
            await _publisher.Publish(notification, ct);
    }
}
