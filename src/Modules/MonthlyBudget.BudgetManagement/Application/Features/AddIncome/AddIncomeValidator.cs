using FluentValidation;
namespace MonthlyBudget.BudgetManagement.Application.Features.AddIncome;
public sealed class AddIncomeValidator : AbstractValidator<AddIncomeCommand>
{
    public AddIncomeValidator()
    {
        RuleFor(x => x.BudgetId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
