using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class BudgetCreated : DomainEventBase
{
    public Guid BudgetId { get; }
    public Guid HouseholdId { get; }
    public string YearMonth { get; }
    public BudgetCreated(Guid budgetId, Guid householdId, string yearMonth)
    {
        BudgetId = budgetId;
        HouseholdId = householdId;
        YearMonth = yearMonth;
    }
}
