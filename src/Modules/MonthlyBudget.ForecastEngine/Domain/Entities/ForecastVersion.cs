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
    public static ForecastVersion CreateOriginal(Guid budgetId, Guid householdId, decimal startBalance, List<ExpenseSnapshot> snapshots, List<DailyEntry> entries)
    {
        var forecast = new ForecastVersion
        {
            ForecastId = Guid.NewGuid(), BudgetId = budgetId, HouseholdId = householdId,
            VersionLabel = "Original", ForecastDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StartDay = 0, StartBalance = startBalance, ForecastType = ForecastType.ORIGINAL,
            IsSnapshot = false, CreatedAt = DateTime.UtcNow
        };
        forecast._expenseSnapshots.AddRange(snapshots);
        forecast._dailyEntries.AddRange(entries);
        return forecast;
    }
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
        forecast._expenseSnapshots.AddRange(snapshots);
        forecast._dailyEntries.AddRange(entries);
        return forecast;
    }
    // INV-F4: Snapshots are immutable
    public void MarkAsSnapshot()
    {
        IsSnapshot = true;
    }
    public decimal GetEndOfMonthBalance()
        => _dailyEntries.OrderByDescending(e => e.DayNumber).FirstOrDefault()?.RemainingBalance ?? StartBalance;
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseDomainEvent(IDomainEvent e) => _domainEvents.Add(e);
}
