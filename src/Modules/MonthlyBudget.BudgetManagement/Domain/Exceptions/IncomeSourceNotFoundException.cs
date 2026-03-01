namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;
public class IncomeSourceNotFoundException : DomainException
{
    public IncomeSourceNotFoundException(Guid incomeId)
        : base($"Income source with ID '{incomeId}' was not found.") { }
}
