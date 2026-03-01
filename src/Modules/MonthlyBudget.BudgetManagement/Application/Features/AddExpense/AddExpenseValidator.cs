using FluentValidation;
namespace MonthlyBudget.BudgetManagement.Application.Features.AddExpense;
public sealed class AddExpenseValidator : AbstractValidator<AddExpenseCommand>
{
    public AddExpenseValidator()
    {
        RuleFor(x => x.BudgetId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
