namespace MonthlyBudget.ForecastEngine.Domain.Entities;
/// <summary>Frozen copy of a Budget expense at forecast generation time (ACL boundary).</summary>
public enum SnapshotCategory { FIXED, SUBSCRIPTION, VARIABLE }
public class ExpenseSnapshot
{
    public Guid SnapshotId { get; private set; }
    public Guid ForecastId { get; private set; }
    public Guid OriginalExpenseId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SnapshotCategory Category { get; private set; }
    public int? DayOfMonth { get; private set; }
    public bool IsSpread { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsExcluded { get; private set; }
    private ExpenseSnapshot() { }
    public static ExpenseSnapshot Create(Guid forecastId, Guid originalExpenseId, string name,
        SnapshotCategory category, int? dayOfMonth, bool isSpread, decimal amount, bool isExcluded) => new()
    {
        SnapshotId = Guid.NewGuid(), ForecastId = forecastId,
        OriginalExpenseId = originalExpenseId, Name = name,
        Category = category, DayOfMonth = dayOfMonth,
        IsSpread = isSpread, Amount = amount, IsExcluded = isExcluded
    };
    /// <summary>
    /// Creates a new expense snapshot based on an existing snapshot, optionally overriding its amount.
    /// </summary>
    /// <param name="forecastId">Identifier of the forecast to associate with the new snapshot.</param>
    /// <param name="source">The source snapshot whose properties will be copied.</param>
    /// <param name="newAmount">If provided, the amount to set on the new snapshot; otherwise the source's amount is used.</param>
    /// <returns>An <see cref="ExpenseSnapshot"/> with a new <see cref="ExpenseSnapshot.SnapshotId"/>, <see cref="ExpenseSnapshot.ForecastId"/> set to <paramref name="forecastId"/>, other properties copied from <paramref name="source"/>, and the amount set as described.</returns>
    public static ExpenseSnapshot CreateAdjusted(Guid forecastId, ExpenseSnapshot source, decimal? newAmount) => new()
    {
        SnapshotId = Guid.NewGuid(), ForecastId = forecastId,
        OriginalExpenseId = source.OriginalExpenseId, Name = source.Name,
        Category = source.Category, DayOfMonth = source.DayOfMonth,
        IsSpread = source.IsSpread, Amount = newAmount ?? source.Amount, IsExcluded = source.IsExcluded
    };
}
