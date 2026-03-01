using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class ExpenseExclusionToggled : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid ExpenseId { get; }
    public bool IsExcluded { get; }
    public ExpenseExclusionToggled(Guid budgetId, Guid expenseId, bool isExcluded)
    {
        BudgetId = budgetId;
        ExpenseId = expenseId;
        IsExcluded = isExcluded;
    }
}
