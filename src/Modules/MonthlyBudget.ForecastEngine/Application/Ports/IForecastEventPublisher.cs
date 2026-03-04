using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Application.Ports;
public interface IForecastEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}

