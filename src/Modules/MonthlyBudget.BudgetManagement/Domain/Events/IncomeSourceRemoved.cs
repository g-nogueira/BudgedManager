using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class IncomeSourceRemoved : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid IncomeId { get; }
    public IncomeSourceRemoved(Guid budgetId, Guid incomeId)
    {
        BudgetId = budgetId;
        IncomeId = incomeId;
    }
}
