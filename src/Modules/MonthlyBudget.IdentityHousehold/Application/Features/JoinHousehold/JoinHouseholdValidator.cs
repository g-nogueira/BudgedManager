using FluentValidation;
namespace MonthlyBudget.IdentityHousehold.Application.Features.JoinHousehold;
public sealed class JoinHouseholdValidator : AbstractValidator<JoinHouseholdCommand>
{
    public JoinHouseholdValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required.");
    }
}

