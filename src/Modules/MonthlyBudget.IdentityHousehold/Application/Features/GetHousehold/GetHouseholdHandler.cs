using MediatR;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.GetHousehold;
public sealed class GetHouseholdHandler : IRequestHandler<GetHouseholdQuery, HouseholdDto?>
{
    private readonly IHouseholdRepository _repo;
    public GetHouseholdHandler(IHouseholdRepository repo) { _repo = repo; }
    public async Task<HouseholdDto?> Handle(GetHouseholdQuery q, CancellationToken ct)
    {
        var household = await _repo.FindByIdAsync(q.HouseholdId, ct);
        if (household == null) return null;
        // Enforce: requesting user must be a member of this household
        if (!household.Members.Any(m => m.UserId == q.RequestingUserId)) return null;
        return new HouseholdDto(
            household.HouseholdId,
            household.Name,
            household.Members.Select(m => new MemberDto(m.UserId, m.Role.ToString())).ToList());
    }
}

