using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Domain.Events;
public sealed class ForecastGenerated : DomainEventBase
{
    public Guid ForecastId { get; }
    public Guid BudgetId { get; }
    public Guid HouseholdId { get; }
    public string ForecastType { get; }
    public ForecastGenerated(Guid forecastId, Guid budgetId, Guid householdId, string forecastType)
    { ForecastId = forecastId; BudgetId = budgetId; HouseholdId = householdId; ForecastType = forecastType; }
}

