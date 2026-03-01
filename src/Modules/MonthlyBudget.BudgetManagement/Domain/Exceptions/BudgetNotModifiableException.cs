namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;
public class BudgetNotModifiableException : DomainException
{
    public BudgetNotModifiableException(string status)
        : base($"Budget in status '{status}' cannot be modified. Only ACTIVE (or DRAFT for initial setup) budgets allow mutations.") { }
}
