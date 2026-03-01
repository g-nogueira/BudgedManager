using FluentValidation;
namespace MonthlyBudget.BudgetManagement.Application.Features.RolloverMonth;
public sealed class RolloverMonthValidator : AbstractValidator<RolloverMonthCommand>
{
    public RolloverMonthValidator()
    {
        RuleFor(x => x.BudgetId).NotEmpty();
        RuleFor(x => x.TargetYearMonth).NotEmpty().Matches(@"^\d{4}-\d{2}$").WithMessage("TargetYearMonth must be in format YYYY-MM.");
    }
}
