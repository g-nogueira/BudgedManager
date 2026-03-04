using FluentValidation;
namespace MonthlyBudget.IdentityHousehold.Application.Features.CreateHousehold;
public sealed class CreateHouseholdValidator : AbstractValidator<CreateHouseholdCommand>
{
    public CreateHouseholdValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Household name is required.")
            .MaximumLength(100).WithMessage("Household name must not exceed 100 characters.");
    }
}

