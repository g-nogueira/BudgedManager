using MonthlyBudget.ForecastEngine.Domain.Entities;
namespace MonthlyBudget.ForecastEngine.Domain.Services;
/// <summary>
/// Core domain service: computes daily cashflow projection (INV-F1 through INV-F5).
/// Pure functional — no external dependencies, easily unit-testable.
/// </summary>
public static class ForecastCalculator
{
    /// <summary>
    /// Generates an ORIGINAL forecast from day 1 through <paramref name="monthDays"/>.
    /// </summary>
    public static ForecastVersion Generate(
        Guid budgetId,
        Guid householdId,
        decimal startBalance,
        int monthDays,
        IReadOnlyList<ExpenseSnapshot> snapshots)
    {
        var forecastId = Guid.NewGuid();
        var entries = BuildDailyEntries(forecastId, startBalance, monthDays, snapshots, startDay: 1);
        var clonedSnapshots = snapshots.ToList();
        return ForecastVersion.CreateOriginal(budgetId, householdId, startBalance, clonedSnapshots, entries);
    }
    /// <summary>
    /// Generates a REFORECAST starting from <paramref name="startDay"/> with <paramref name="actualBalance"/>.
    /// </summary>
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
        var forecastId = Guid.NewGuid();
        var entries = BuildDailyEntries(forecastId, actualBalance, monthDays, adjustedSnapshots, startDay);
        var clonedSnapshots = adjustedSnapshots.ToList();
        return ForecastVersion.CreateReforecast(budgetId, householdId, parentForecastId,
            startDay, actualBalance, versionLabel, clonedSnapshots, entries);
    }
    // --- Core Algorithm ----------------------------------------------------------
    private static List<DailyEntry> BuildDailyEntries(
        Guid forecastId,
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
            entries.Add(DailyEntry.Create(forecastId, day, balance, items));
        }
        return entries;
    }
}
