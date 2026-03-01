using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class BudgetActivated : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid HouseholdId { get; }
    public string YearMonth { get; }
    public decimal TotalIncome { get; }
    public int ExpenseCount { get; }
    public BudgetActivated(Guid budgetId, Guid householdId, string yearMonth, decimal totalIncome, int expenseCount)
    {
        BudgetId = budgetId;
        HouseholdId = householdId;
        YearMonth = yearMonth;
        TotalIncome = totalIncome;
        ExpenseCount = expenseCount;
    }
}
