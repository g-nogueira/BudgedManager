namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;
public class BudgetNotFoundException : DomainException
{
    public BudgetNotFoundException(Guid budgetId)
        : base($"Budget with ID '{budgetId}' was not found.") { }
}
