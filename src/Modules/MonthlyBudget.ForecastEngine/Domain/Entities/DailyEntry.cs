namespace MonthlyBudget.ForecastEngine.Domain.Entities;
public class DailyEntry
{
    private readonly List<DailyExpenseItem> _expenseBreakdown = new();
    public Guid EntryId { get; private set; }
    public Guid ForecastId { get; private set; }
    public int DayNumber { get; private set; }
    public decimal RemainingBalance { get; private set; }
    public decimal DailyExpenseTotal { get; private set; }
    public IReadOnlyList<DailyExpenseItem> ExpenseBreakdown => _expenseBreakdown.AsReadOnly();
    private DailyEntry() { }
    public static DailyEntry Create(Guid forecastId, int day, decimal remainingBalance, IEnumerable<DailyExpenseItem> items)
    {
        var entry = new DailyEntry
        {
            EntryId = Guid.NewGuid(), ForecastId = forecastId,
            DayNumber = day, RemainingBalance = remainingBalance,
        };
        entry._expenseBreakdown.AddRange(items);
        entry.DailyExpenseTotal = items.Sum(i => i.Amount);
        return entry;
    }
}
