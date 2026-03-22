using FluentValidation;
using MonthlyBudget.ForecastEngine.Domain.Entities;

namespace MonthlyBudget.ForecastEngine.Application.Features.Reforecast;

public sealed class ReforecastValidator : AbstractValidator<ReforecastCommand>
{
    public ReforecastValidator()
    {
        RuleFor(x => x.BudgetId).NotEmpty();
        RuleFor(x => x.HouseholdId).NotEmpty();
        RuleFor(x => x.ParentForecastId).NotEmpty();
        RuleFor(x => x.StartDay).GreaterThan(0);
        RuleFor(x => x.ActualBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VersionLabel).NotEmpty().MaximumLength(100);

        RuleForEach(x => x.ExpenseAdjustments!).SetValidator(new ExpenseAdjustmentValidator())
            .When(x => x.ExpenseAdjustments is not null);
    }

    private sealed class ExpenseAdjustmentValidator : AbstractValidator<ExpenseAdjustment>
    {
        public ExpenseAdjustmentValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty()
                .Must(action =>
                    string.Equals(action, "MODIFY", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(action, "REMOVE", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(action, "ADD", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Action must be one of MODIFY, REMOVE, or ADD.");

            When(x => string.Equals(x.Action, "MODIFY", StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.OriginalExpenseId).NotNull().NotEqual(Guid.Empty);
                RuleFor(x => x.NewAmount).NotNull().GreaterThan(0);
            });

            When(x => string.Equals(x.Action, "REMOVE", StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.OriginalExpenseId).NotNull().NotEqual(Guid.Empty);
            });

            When(x => string.Equals(x.Action, "ADD", StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.Category)
                    .NotEmpty()
                    .Must(category => Enum.TryParse<SnapshotCategory>(category, true, out _))
                    .WithMessage("Category must be one of FIXED, SUBSCRIPTION, or VARIABLE.");
                RuleFor(x => x.NewAmount).NotNull().GreaterThan(0);

                RuleFor(x => x)
                    .Must(x => x.IsSpread == true ? x.DayOfMonth is null : x.DayOfMonth.HasValue)
                    .WithMessage("For ADD action, spread expenses require DayOfMonth null, non-spread expenses require DayOfMonth.");
            });
        }
    }
}
