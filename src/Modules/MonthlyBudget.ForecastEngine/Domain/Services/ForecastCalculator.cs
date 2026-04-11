using MonthlyBudget.ForecastEngine.Domain.Entities;
namespace MonthlyBudget.ForecastEngine.Domain.Services;
/// <summary>
/// Core domain service: computes daily cashflow projection (INV-F1, INV-F2, INV-F3, INV-F5).
/// Pure functional - no external dependencies, easily unit-testable.
/// </summary>
public static class ForecastCalculator
{
    /// <summary>
    /// Creates an original forecast version spanning day 1 through the specified month length.
    /// </summary>
    /// <param name="budgetId">Identifier of the budget the forecast belongs to.</param>
    /// <param name="householdId">Identifier of the household the forecast belongs to.</param>
    /// <param name="startBalance">Starting end-of-day balance used on day 1.</param>
    /// <param name="monthDays">Number of days in the month to project (forecast covers days 1..monthDays).</param>
    /// <param name="snapshots">Expense snapshots to include in the forecast; these are cloned for the returned version.</param>
    /// <returns>A ForecastVersion marked as the original forecast containing the provided metadata, cloned snapshots, and computed daily entries for each day from 1 to <paramref name="monthDays"/>.</returns>
    public static ForecastVersion Generate(
        Guid budgetId,
        Guid householdId,
        decimal startBalance,
        int monthDays,
        IReadOnlyList<ExpenseSnapshot> snapshots)
    {
        var entries = BuildDailyEntries(startBalance, monthDays, snapshots, startDay: 1);
        var clonedSnapshots = snapshots.ToList();
        return ForecastVersion.CreateOriginal(budgetId, householdId, startBalance, clonedSnapshots, entries);
    }
    /// <summary>
    /// Creates a reforecast ForecastVersion that projects end-of-day balances and expense items from the specified start day through the end of the month using the provided actual balance and adjusted snapshots.
    /// </summary>
    /// <param name="budgetId">The budget identifier associated with the forecast.</param>
    /// <param name="householdId">The household identifier associated with the forecast.</param>
    /// <param name="parentForecastId">The identifier of the parent forecast from which this reforecast is derived.</param>
    /// <param name="startDay">The first day (inclusive) of the month for which the reforecast is produced.</param>
    /// <param name="actualBalance">The starting balance to use on the start day.</param>
    /// <param name="monthDays">The total number of days in the month used to calculate daily spreads and iterate days.</param>
    /// <param name="adjustedSnapshots">Expense snapshots to apply for the reforecast; these will be cloned for inclusion in the returned version.</param>
    /// <param name="versionLabel">A human-readable label for this reforecast version.</param>
    /// <returns>A ForecastVersion representing the reforecast covering days from <paramref name="startDay"/> through <paramref name="monthDays"/>, containing cloned snapshots and computed daily entries.</returns>
    public static ForecastVersion Reforecast(
        Guid budgetId,
        Guid householdId,
        Guid parentForecastId,
        int startDay,
        decimal actualBalance,
        int monthDays,
        IReadOnlyList<ExpenseSnapshot> adjustedSnapshots,
        string versionLabel)
    {
        var entries = BuildDailyEntries(actualBalance, monthDays, adjustedSnapshots, startDay);
        var clonedSnapshots = adjustedSnapshots.ToList();
        return ForecastVersion.CreateReforecast(budgetId, householdId, parentForecastId,
            startDay, actualBalance, versionLabel, clonedSnapshots, entries);
    }
    // --- Core Algorithm ----------------------------------------------------------
    private static List<DailyEntry> BuildDailyEntries(
        decimal startBalance,
        int monthDays,
        IReadOnlyList<ExpenseSnapshot> snapshots,
        int startDay)
    {
        // INV-F5: only non-excluded snapshots affect balance
        var activeSnapshots = snapshots.Where(s => !s.IsExcluded).ToList();
        // Pre-compute daily spread amounts (spread = distributed evenly over all month days)
        var spreadDailyAmount = activeSnapshots
            .Where(s => s.IsSpread)
            .Sum(s => s.Amount / monthDays);
        // Build lookup: day ? list of fixed/subscription items on that day
        var fixedByDay = activeSnapshots
            .Where(s => !s.IsSpread && s.DayOfMonth.HasValue)
            .GroupBy(s => s.DayOfMonth!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        var entries = new List<DailyEntry>(monthDays - startDay + 1);
        decimal balance = startBalance;
        for (int day = startDay; day <= monthDays; day++)
        {
            var items = new List<DailyExpenseItem>();
            // Add fixed/subscription expenses for this day
            if (fixedByDay.TryGetValue(day, out var daySnapshots))
            {
                foreach (var snap in daySnapshots)
                {
                    items.Add(DailyExpenseItem.Create(Guid.NewGuid(), snap.SnapshotId, snap.Name, snap.Amount));
                    balance -= snap.Amount;
                }
            }
            // Add daily portion of spread expenses
            if (spreadDailyAmount > 0)
            {
                // Round to 2 decimal places to avoid floating-point drift
                var spreadToday = Math.Round(spreadDailyAmount, 2, MidpointRounding.AwayFromZero);
                // Group all spread expenses into one item for display
                items.Add(DailyExpenseItem.Create(Guid.NewGuid(), Guid.Empty, "Spread (daily portion)", spreadToday));
                balance -= spreadToday;
            }
            balance = Math.Round(balance, 2, MidpointRounding.AwayFromZero);
            entries.Add(DailyEntry.Create(Guid.Empty, day, balance, items));
        }
        return entries;
    }
}
