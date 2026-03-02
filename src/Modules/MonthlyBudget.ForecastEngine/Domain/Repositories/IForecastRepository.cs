using MonthlyBudget.ForecastEngine.Domain.Entities;
namespace MonthlyBudget.ForecastEngine.Domain.Repositories;
public interface IForecastRepository
{
    Task<ForecastVersion?> FindByIdAsync(Guid forecastId, CancellationToken ct = default);
    Task<IReadOnlyList<ForecastVersion>> FindAllByBudgetAsync(Guid budgetId, CancellationToken ct = default);
    Task<ForecastVersion?> FindLatestByBudgetAsync(Guid budgetId, CancellationToken ct = default);
    Task SaveAsync(ForecastVersion forecast, CancellationToken ct = default);
}
