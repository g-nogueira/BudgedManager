using MonthlyBudget.SharedKernel.Events;
using MonthlyBudget.BudgetManagement.Domain.Entities;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class ExpenseAdded : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid ExpenseId { get; }
    public string Name { get; }
    public ExpenseCategory Category { get; }
    public int? DayOfMonth { get; }
    public bool IsSpread { get; }
    public decimal Amount { get; }
    public ExpenseAdded(Guid budgetId, Guid expenseId, string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount)
    {
        BudgetId = budgetId;
        ExpenseId = expenseId;
        Name = name;
        Category = category;
        DayOfMonth = dayOfMonth;
        IsSpread = isSpread;
        Amount = amount;
    }
}
