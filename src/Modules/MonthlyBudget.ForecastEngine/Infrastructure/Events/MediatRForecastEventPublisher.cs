using MediatR;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Infrastructure.Events;
public sealed class MediatRForecastEventPublisher : IForecastEventPublisher
{
    private readonly IPublisher _publisher;
    public MediatRForecastEventPublisher(IPublisher publisher) { _publisher = publisher; }
    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        if (domainEvent is INotification notification)
            await _publisher.Publish(notification, ct);
    }
}

