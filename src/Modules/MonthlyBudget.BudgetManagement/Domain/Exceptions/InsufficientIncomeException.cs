namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;

public class InsufficientIncomeException : DomainException
{
    public InsufficientIncomeException()
        : base("Budget cannot be activated without at least one income source.") { }
}

