using FluentValidation;
namespace MonthlyBudget.BudgetManagement.Application.Features.UpdateIncome;
public sealed class UpdateIncomeValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeValidator()
    {
        RuleFor(x => x.BudgetId).NotEmpty();
        RuleFor(x => x.IncomeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
