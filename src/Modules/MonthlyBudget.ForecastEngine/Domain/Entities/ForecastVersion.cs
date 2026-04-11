using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Domain.Entities;
public enum ForecastType { ORIGINAL, REFORECAST }
public class ForecastVersion
{
    private readonly List<DailyEntry> _dailyEntries = new();
    private readonly List<ExpenseSnapshot> _expenseSnapshots = new();
    private readonly List<IDomainEvent> _domainEvents = new();
    public Guid ForecastId { get; private set; }
    public Guid BudgetId { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string VersionLabel { get; private set; } = string.Empty;
    public DateOnly ForecastDate { get; private set; }
    public int StartDay { get; private set; }
    public decimal StartBalance { get; private set; }
    public decimal? ActualBalance { get; private set; }
    public ForecastType ForecastType { get; private set; }
    public Guid? ParentForecastId { get; private set; }
    public bool IsSnapshot { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<DailyEntry> DailyEntries => _dailyEntries.AsReadOnly();
    public IReadOnlyList<ExpenseSnapshot> ExpenseSnapshots => _expenseSnapshots.AsReadOnly();
    private ForecastVersion() { }
    /// <summary>
    /// Create a new original forecast version for the specified budget and household.
    /// </summary>
    /// <param name="snapshots">Expense snapshots to include in the forecast; each will have its ForecastId set to the newly generated forecast ID.</param>
    /// <param name="entries">Daily entries to include in the forecast; each will have its ForecastId set to the newly generated forecast ID.</param>
    /// <returns>A newly constructed ForecastVersion configured as an original forecast with a generated ForecastId and the provided snapshots and entries attached.</returns>
    public static ForecastVersion CreateOriginal(Guid budgetId, Guid householdId, decimal startBalance, List<ExpenseSnapshot> snapshots, List<DailyEntry> entries)
    {
        var forecast = new ForecastVersion
        {
            ForecastId = Guid.NewGuid(), BudgetId = budgetId, HouseholdId = householdId,
            VersionLabel = "Original", ForecastDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartDay = 0, StartBalance = startBalance, ForecastType = ForecastType.ORIGINAL,
            IsSnapshot = false, CreatedAt = DateTime.UtcNow
        };
        forecast._expenseSnapshots.AddRange(snapshots.Select(snapshot => ExpenseSnapshot.Create(
            forecast.ForecastId,
            snapshot.OriginalExpenseId,
            snapshot.Name,
            snapshot.Category,
            snapshot.DayOfMonth,
            snapshot.IsSpread,
            snapshot.Amount,
            snapshot.IsExcluded)));
        forecast._dailyEntries.AddRange(entries);

        foreach (var entry in forecast._dailyEntries)
            entry.AssignForecastId(forecast.ForecastId);

        return forecast;
    }
    /// <summary>
    /// Creates a reforecast ForecastVersion linked to an existing parent forecast.
    /// </summary>
    /// <param name="budgetId">Identifier of the budget the forecast belongs to.</param>
    /// <param name="householdId">Identifier of the household the forecast belongs to.</param>
    /// <param name="parentForecastId">Identifier of the parent forecast; must not be <see cref="Guid.Empty"/>.</param>
    /// <param name="startDay">The starting day index for the forecast.</param>
    /// <param name="actualBalance">The actual balance to set as both the start and actual balance for the reforecast.</param>
    /// <param name="label">A human-readable label for this forecast version.</param>
    /// <param name="snapshots">Expense snapshots to include in the forecast; their ForecastId will be set to the created forecast's ID.</param>
    /// <param name="entries">Daily entries to include in the forecast; their ForecastId will be set to the created forecast's ID.</param>
    /// <returns>The newly created ForecastVersion configured as a reforecast.</returns>
    /// <exception cref="InvalidReforecastException">Thrown when <paramref name="parentForecastId"/> is <see cref="Guid.Empty"/>.</exception>
    public static ForecastVersion CreateReforecast(Guid budgetId, Guid householdId, Guid parentForecastId,
        int startDay, decimal actualBalance, string label, List<ExpenseSnapshot> snapshots, List<DailyEntry> entries)
    {
        if (parentForecastId == Guid.Empty)
            throw new InvalidReforecastException("REFORECAST requires a valid parent forecast ID.");
        var forecast = new ForecastVersion
        {
            ForecastId = Guid.NewGuid(), BudgetId = budgetId, HouseholdId = householdId,
            VersionLabel = label, ForecastDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartDay = startDay, StartBalance = actualBalance, ActualBalance = actualBalance,
            ForecastType = ForecastType.REFORECAST, ParentForecastId = parentForecastId,
            IsSnapshot = false, CreatedAt = DateTime.UtcNow
        };
        forecast._expenseSnapshots.AddRange(snapshots.Select(snapshot => ExpenseSnapshot.Create(
            forecast.ForecastId,
            snapshot.OriginalExpenseId,
            snapshot.Name,
            snapshot.Category,
            snapshot.DayOfMonth,
            snapshot.IsSpread,
            snapshot.Amount,
            snapshot.IsExcluded)));
        forecast._dailyEntries.AddRange(entries);

        foreach (var entry in forecast._dailyEntries)
            entry.AssignForecastId(forecast.ForecastId);

        return forecast;
    }
    // INV-F4: Snapshots are immutable
    public void MarkAsSnapshot()
    {
        IsSnapshot = true;
    }

    public void SetActualBalance(decimal balance)
    {
        if (IsSnapshot)
            throw new SnapshotImmutableException();

        ActualBalance = balance;
    }

    public decimal GetEndOfMonthBalance()
        => _dailyEntries.OrderByDescending(e => e.DayNumber).FirstOrDefault()?.RemainingBalance ?? StartBalance;
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseDomainEvent(IDomainEvent e) => _domainEvents.Add(e);
}
