namespace MonthlyBudget.ForecastEngine.Domain.Entities;
public class DailyExpenseItem
{
    public Guid ItemId { get; private set; }
    public Guid EntryId { get; private set; }
    public Guid ExpenseSnapshotId { get; private set; }
    public string ExpenseName { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    private DailyExpenseItem() { }
    public static DailyExpenseItem Create(Guid entryId, Guid snapshotId, string name, decimal amount) => new()
    {
        ItemId = Guid.NewGuid(), EntryId = entryId,
        ExpenseSnapshotId = snapshotId, ExpenseName = name, Amount = amount
    };
}
