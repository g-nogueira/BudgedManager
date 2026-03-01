using FluentValidation;
namespace MonthlyBudget.BudgetManagement.Application.Features.CreateBudget;
public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.HouseholdId).NotEmpty();
        RuleFor(x => x.YearMonth)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{2}$").WithMessage("YearMonth must be in format YYYY-MM.");
    }
}
