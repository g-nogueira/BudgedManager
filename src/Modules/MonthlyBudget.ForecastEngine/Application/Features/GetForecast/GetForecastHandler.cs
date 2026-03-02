using MediatR;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
namespace MonthlyBudget.ForecastEngine.Application.Features.GetForecast;
public sealed class GetForecastHandler : IRequestHandler<GetForecastQuery, ForecastDto?>
{
    private readonly IForecastRepository _repo;
    public GetForecastHandler(IForecastRepository repo) { _repo = repo; }
    public async Task<ForecastDto?> Handle(GetForecastQuery q, CancellationToken ct)
    {
        var forecast = await _repo.FindByIdAsync(q.ForecastId, ct);
        if (forecast == null || forecast.HouseholdId != q.HouseholdId) return null;
        return new ForecastDto(forecast.ForecastId, forecast.BudgetId, forecast.VersionLabel, forecast.ForecastType.ToString(),
            forecast.StartDay, forecast.StartBalance, forecast.GetEndOfMonthBalance(), forecast.IsSnapshot,
            forecast.DailyEntries.Select(e => new DailyEntryDto(e.DayNumber, e.RemainingBalance, e.DailyExpenseTotal,
                e.ExpenseBreakdown.Select(i => new ExpenseItemDto(i.ExpenseName, i.Amount)).ToList())).ToList());
    }
}
public sealed class GetForecastsByBudgetHandler : IRequestHandler<GetForecastsByBudgetQuery, IReadOnlyList<ForecastSummaryDto>>
{
    private readonly IForecastRepository _repo;
    public GetForecastsByBudgetHandler(IForecastRepository repo) { _repo = repo; }
    public async Task<IReadOnlyList<ForecastSummaryDto>> Handle(GetForecastsByBudgetQuery q, CancellationToken ct)
    {
        var forecasts = await _repo.FindAllByBudgetAsync(q.BudgetId, ct);
        return forecasts.Where(f => f.HouseholdId == q.HouseholdId)
            .Select(f => new ForecastSummaryDto(f.ForecastId, f.VersionLabel, f.ForecastType.ToString(), f.GetEndOfMonthBalance(), f.IsSnapshot, f.CreatedAt))
            .ToList();
    }
}
