namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;
public class InvalidExpenseDayException : DomainException
{
    public InvalidExpenseDayException(string message) : base(message) { }
}
