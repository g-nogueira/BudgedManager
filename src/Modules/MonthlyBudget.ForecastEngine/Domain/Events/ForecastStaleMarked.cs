using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Domain.Events;
public sealed class ForecastStaleMarked : DomainEventBase
{
    public Guid ForecastId { get; }
    public string Reason { get; }
    public ForecastStaleMarked(Guid forecastId, string reason) { ForecastId = forecastId; Reason = reason; }
}

