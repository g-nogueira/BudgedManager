using MediatR;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
using MonthlyBudget.ForecastEngine.Domain.Services;
namespace MonthlyBudget.ForecastEngine.Application.Features.Reforecast;
public sealed class ReforecastHandler : IRequestHandler<ReforecastCommand, ReforecastResult>
{
    private readonly IForecastRepository _repo;
    private readonly IBudgetDataPort _budgetPort;
    public ReforecastHandler(IForecastRepository repo, IBudgetDataPort budgetPort) { _repo = repo; _budgetPort = budgetPort; }
    public async Task<ReforecastResult> Handle(ReforecastCommand cmd, CancellationToken ct)
    {
        var parent = await _repo.FindByIdAsync(cmd.ParentForecastId, ct) ?? throw new ForecastNotFoundException(cmd.ParentForecastId);
        var data = await _budgetPort.GetBudgetDataAsync(cmd.BudgetId, cmd.HouseholdId, ct) ?? throw new ForecastNotFoundException(cmd.BudgetId);
        var yearMonth = System.DateTime.ParseExact(data.YearMonth + "-01", "yyyy-MM-dd", null);
        int monthDays = System.DateTime.DaysInMonth(yearMonth.Year, yearMonth.Month);
        // Use parent snapshots as base — allow future adjustment via explicit snapshot updates
        var snapshots = parent.ExpenseSnapshots.ToList();
        var forecast = ForecastCalculator.Reforecast(cmd.BudgetId, cmd.HouseholdId, cmd.ParentForecastId,
            cmd.StartDay, cmd.ActualBalance, monthDays, snapshots, cmd.VersionLabel);
        await _repo.SaveAsync(forecast, ct);
        return new ReforecastResult(forecast.ForecastId, forecast.GetEndOfMonthBalance());
    }
}
