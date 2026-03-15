using FluentValidation;
namespace MonthlyBudget.IdentityHousehold.Application.Features.InviteMember;
public sealed class InviteMemberValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberValidator()
    {
        RuleFor(x => x.PartnerEmail)
            .NotEmpty().WithMessage("Partner email is required.")
            .EmailAddress().WithMessage("Partner email must be a valid email address.");
        RuleFor(x => x.HouseholdId)
            .NotEmpty().WithMessage("Household ID is required.");
    }
}

