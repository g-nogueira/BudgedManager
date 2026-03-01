using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class ExpenseUpdated : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid ExpenseId { get; }
    public ExpenseUpdated(Guid budgetId, Guid expenseId)
    {
        BudgetId = budgetId;
        ExpenseId = expenseId;
    }
}
