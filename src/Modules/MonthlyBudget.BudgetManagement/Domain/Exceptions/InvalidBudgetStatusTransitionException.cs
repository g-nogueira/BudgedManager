namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;
public class InvalidBudgetStatusTransitionException : DomainException
{
    public InvalidBudgetStatusTransitionException(string from, string to)
        : base($"Cannot transition budget status from {from} to {to}.") { }
}
