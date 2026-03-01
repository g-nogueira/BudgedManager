using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class IncomeSourceAdded : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid IncomeId { get; }
    public string Name { get; }
    public decimal Amount { get; }
    public IncomeSourceAdded(Guid budgetId, Guid incomeId, string name, decimal amount)
    {
        BudgetId = budgetId;
        IncomeId = incomeId;
        Name = name;
        Amount = amount;
    }
}
