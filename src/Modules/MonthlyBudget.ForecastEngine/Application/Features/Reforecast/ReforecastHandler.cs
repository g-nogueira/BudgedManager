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
        // AutoSnapshotOnReforecast policy: snapshot parent if not already snapshotted
        if (!parent.IsSnapshot)
        {
            parent.MarkAsSnapshot();
            await _repo.SaveAsync(parent, ct);
        }
        var snapshots = parent.ExpenseSnapshots.ToList();

        if (cmd.ExpenseAdjustments is { Count: > 0 })
        {
            var existingIds = snapshots.Select(s => s.OriginalExpenseId).ToHashSet();

            foreach (var adjustment in cmd.ExpenseAdjustments)
            {
                var isModify = string.Equals(adjustment.Action, "MODIFY", StringComparison.OrdinalIgnoreCase);
                var isRemove = string.Equals(adjustment.Action, "REMOVE", StringComparison.OrdinalIgnoreCase);
                if ((isModify || isRemove) && adjustment.OriginalExpenseId.HasValue && !existingIds.Contains(adjustment.OriginalExpenseId.Value))
                {
                    throw new InvalidReforecastException($"Expense '{adjustment.OriginalExpenseId}' not found in parent forecast snapshots.");
                }
            }

            var adjustedSnapshots = new List<ExpenseSnapshot>();
            var removedIds = cmd.ExpenseAdjustments
                .Where(a => string.Equals(a.Action, "REMOVE", StringComparison.OrdinalIgnoreCase) && a.OriginalExpenseId.HasValue)
                .Select(a => a.OriginalExpenseId!.Value)
                .ToHashSet();

            foreach (var snapshot in snapshots)
            {
                if (removedIds.Contains(snapshot.OriginalExpenseId))
                {
                    continue;
                }

                var modify = cmd.ExpenseAdjustments.FirstOrDefault(a =>
                    string.Equals(a.Action, "MODIFY", StringComparison.OrdinalIgnoreCase) &&
                    a.OriginalExpenseId == snapshot.OriginalExpenseId);

                if (modify is not null)
                {
                    adjustedSnapshots.Add(ExpenseSnapshot.CreateAdjusted(Guid.Empty, snapshot, modify.NewAmount));
                }
                else
                {
                    adjustedSnapshots.Add(snapshot);
                }
            }

            foreach (var add in cmd.ExpenseAdjustments.Where(a => string.Equals(a.Action, "ADD", StringComparison.OrdinalIgnoreCase)))
            {
                var category = Enum.Parse<SnapshotCategory>(add.Category!, true);
                adjustedSnapshots.Add(ExpenseSnapshot.Create(
                    Guid.Empty,
                    Guid.NewGuid(),
                    add.Name!,
                    category,
                    add.DayOfMonth,
                    add.IsSpread ?? false,
                    add.NewAmount ?? 0m,
                    false));
            }

            snapshots = adjustedSnapshots;
        }

        var forecast = ForecastCalculator.Reforecast(cmd.BudgetId, cmd.HouseholdId, cmd.ParentForecastId,
            cmd.StartDay, cmd.ActualBalance, monthDays, snapshots, cmd.VersionLabel);
        await _repo.SaveAsync(forecast, ct);
        return new ReforecastResult(forecast.ForecastId, forecast.GetEndOfMonthBalance());
    }
}
