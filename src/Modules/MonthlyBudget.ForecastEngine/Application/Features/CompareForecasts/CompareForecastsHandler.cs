using MediatR;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
namespace MonthlyBudget.ForecastEngine.Application.Features.CompareForecasts;
public sealed class CompareForecastsHandler : IRequestHandler<CompareForecastsQuery, ComparisonResult>
{
    private readonly IForecastRepository _repo;
    public CompareForecastsHandler(IForecastRepository repo) { _repo = repo; }
    public async Task<ComparisonResult> Handle(CompareForecastsQuery q, CancellationToken ct)
    {
        var a = await _repo.FindByIdAsync(q.ForecastAId, ct);
        if (a == null || a.HouseholdId != q.HouseholdId)
            throw new ForecastNotFoundException(q.ForecastAId);
        var b = await _repo.FindByIdAsync(q.ForecastBId, ct);
        if (b == null || b.HouseholdId != q.HouseholdId)
            throw new ForecastNotFoundException(q.ForecastBId);

        // Day variances — align by DayNumber
        var daysA = a.DailyEntries.ToDictionary(e => e.DayNumber, e => e.RemainingBalance);
        var daysB = b.DailyEntries.ToDictionary(e => e.DayNumber, e => e.RemainingBalance);
        var allDays = daysA.Keys.Union(daysB.Keys).OrderBy(d => d);
        var dayVariances = allDays.Select(day =>
        {
            var balA = daysA.GetValueOrDefault(day, 0m);
            var balB = daysB.GetValueOrDefault(day, 0m);
            return new DayVariance(day, balA, balB, balA - balB);
        }).ToList();

        // Expense changes — diff by OriginalExpenseId
        var snapshotsA = a.ExpenseSnapshots.ToDictionary(s => s.OriginalExpenseId);
        var snapshotsB = b.ExpenseSnapshots.ToDictionary(s => s.OriginalExpenseId);
        var allIds = snapshotsA.Keys.Union(snapshotsB.Keys);
        var expenseChanges = new List<ExpenseChange>();
        foreach (var id in allIds)
        {
            var inA = snapshotsA.TryGetValue(id, out var sA);
            var inB = snapshotsB.TryGetValue(id, out var sB);
            if (inA && !inB)
                expenseChanges.Add(new ExpenseChange(sA!.Name, "REMOVED", sA.Amount, null));
            else if (!inA && inB)
                expenseChanges.Add(new ExpenseChange(sB!.Name, "ADDED", null, sB.Amount));
            else if (inA && inB && sA!.Amount != sB!.Amount)
                expenseChanges.Add(new ExpenseChange(sA.Name, "MODIFIED", sA.Amount, sB.Amount));
        }

        var endA = a.GetEndOfMonthBalance();
        var endB = b.GetEndOfMonthBalance();
        return new ComparisonResult(a.ForecastId, b.ForecastId, a.VersionLabel, b.VersionLabel,
            endA, endB, endA - endB, dayVariances, expenseChanges);
    }
}

