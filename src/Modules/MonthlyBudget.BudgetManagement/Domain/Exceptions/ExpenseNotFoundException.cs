namespace MonthlyBudget.BudgetManagement.Domain.Exceptions;
public class ExpenseNotFoundException : DomainException
{
    public ExpenseNotFoundException(Guid expenseId)
        : base($"Expense with ID '{expenseId}' was not found.") { }
}
