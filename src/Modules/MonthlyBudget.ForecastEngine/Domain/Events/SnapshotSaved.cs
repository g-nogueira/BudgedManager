using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Domain.Events;
public sealed class SnapshotSaved : DomainEventBase
{
    public Guid ForecastId { get; }
    public Guid BudgetId { get; }
    public SnapshotSaved(Guid forecastId, Guid budgetId) { ForecastId = forecastId; BudgetId = budgetId; }
}

