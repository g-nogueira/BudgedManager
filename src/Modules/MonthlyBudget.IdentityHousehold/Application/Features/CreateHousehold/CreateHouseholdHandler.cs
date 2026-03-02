using MediatR;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.CreateHousehold;
public sealed class CreateHouseholdHandler : IRequestHandler<CreateHouseholdCommand, CreateHouseholdResult>
{
    private readonly IHouseholdRepository _repo;
    public CreateHouseholdHandler(IHouseholdRepository repo) { _repo = repo; }
    public async Task<CreateHouseholdResult> Handle(CreateHouseholdCommand cmd, CancellationToken ct)
    {
        var household = Household.Create(cmd.Name, cmd.UserId);
        await _repo.SaveAsync(household, ct);
        return new CreateHouseholdResult(household.HouseholdId, household.Name);
    }
}
