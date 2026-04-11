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
    /// <summary>
    /// Create a new DailyEntry for the specified forecast day with the provided remaining balance and expense items.
    /// </summary>
    /// <param name="forecastId">Identifier of the forecast this entry belongs to.</param>
    /// <param name="day">Day number or index within the forecast.</param>
    /// <param name="remainingBalance">Balance remaining after the day's spending.</param>
    /// <param name="items">Expense items to include in the entry's expense breakdown.</param>
    /// <returns>A DailyEntry initialized with a new EntryId, the given ForecastId, DayNumber, RemainingBalance, an expense breakdown populated from <paramref name="items"/>, and DailyExpenseTotal equal to the sum of the items' amounts.</returns>
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

    /// <summary>
    /// Assigns the entry's ForecastId to the specified identifier.
    /// </summary>
    /// <param name="forecastId">The forecast identifier to assign to this entry.</param>
    internal void AssignForecastId(Guid forecastId)
    {
        ForecastId = forecastId;
    }
}
