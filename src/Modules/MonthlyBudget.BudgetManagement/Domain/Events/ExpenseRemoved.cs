using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class ExpenseRemoved : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid ExpenseId { get; }
    public ExpenseRemoved(Guid budgetId, Guid expenseId)
    {
        BudgetId = budgetId;
        ExpenseId = expenseId;
    }
}
