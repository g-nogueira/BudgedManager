using MediatR;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
using MonthlyBudget.ForecastEngine.Domain.Services;
namespace MonthlyBudget.ForecastEngine.Application.Features.GenerateForecast;
public sealed class GenerateForecastHandler : IRequestHandler<GenerateForecastCommand, GenerateForecastResult>
{
    private readonly IForecastRepository _repo;
    private readonly IBudgetDataPort _budgetPort;
    public GenerateForecastHandler(IForecastRepository repo, IBudgetDataPort budgetPort) { _repo = repo; _budgetPort = budgetPort; }
    public async Task<GenerateForecastResult> Handle(GenerateForecastCommand cmd, CancellationToken ct)
    {
        var data = await _budgetPort.GetBudgetDataAsync(cmd.BudgetId, cmd.HouseholdId, ct)
            ?? throw new ForecastNotFoundException(cmd.BudgetId);
        var yearMonth = System.DateTime.ParseExact(data.YearMonth + "-01", "yyyy-MM-dd", null);
        int monthDays = System.DateTime.DaysInMonth(yearMonth.Year, yearMonth.Month);
        var snapshots = data.Expenses.Select(e =>
            ExpenseSnapshot.Create(Guid.NewGuid(), e.ExpenseId, e.Name, e.Category, e.DayOfMonth, e.IsSpread, e.Amount, e.IsExcluded)
        ).ToList();
        var forecast = ForecastCalculator.Generate(cmd.BudgetId, cmd.HouseholdId, data.TotalIncome, monthDays, snapshots);
        await _repo.SaveAsync(forecast, ct);
        return new GenerateForecastResult(forecast.ForecastId, forecast.VersionLabel, forecast.GetEndOfMonthBalance(), forecast.DailyEntries.Count);
    }
}
