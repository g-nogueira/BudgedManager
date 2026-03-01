namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;
public class BudgetAlreadyExistsException : DomainException
{
    public BudgetAlreadyExistsException(string yearMonth)
        : base($"A budget for {yearMonth} already exists for this household.") { }
}
