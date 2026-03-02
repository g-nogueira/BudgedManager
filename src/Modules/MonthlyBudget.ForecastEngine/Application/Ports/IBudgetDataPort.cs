using MonthlyBudget.ForecastEngine.Domain.Entities;
namespace MonthlyBudget.ForecastEngine.Application.Ports;
/// <summary>ACL port: fetches budget data from the BudgetManagement context</summary>
public interface IBudgetDataPort
{
    Task<BudgetData?> GetBudgetDataAsync(Guid budgetId, Guid householdId, CancellationToken ct = default);
}
public sealed record BudgetData(
    Guid BudgetId, Guid HouseholdId, string YearMonth,
    decimal TotalIncome, IReadOnlyList<ExpenseSnapshotData> Expenses);
public sealed record ExpenseSnapshotData(
    Guid ExpenseId, string Name, SnapshotCategory Category,
    int? DayOfMonth, bool IsSpread, decimal Amount, bool IsExcluded);
