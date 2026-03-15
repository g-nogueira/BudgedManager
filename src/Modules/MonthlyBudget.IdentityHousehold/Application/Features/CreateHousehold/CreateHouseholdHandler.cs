using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Events;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.CreateHousehold;
public sealed class CreateHouseholdHandler : IRequestHandler<CreateHouseholdCommand, CreateHouseholdResult>
{
    private readonly IHouseholdRepository _repo;
    private readonly IHouseholdEventPublisher _events;
    public CreateHouseholdHandler(IHouseholdRepository repo, IHouseholdEventPublisher events) { _repo = repo; _events = events; }
    public async Task<CreateHouseholdResult> Handle(CreateHouseholdCommand cmd, CancellationToken ct)
    {
        // Prevent a user from owning multiple households
        var existing = await _repo.FindByMemberIdAsync(cmd.UserId, ct);
        if (existing != null) throw new UserAlreadyInHouseholdException();
        var household = Household.Create(cmd.Name, cmd.UserId);
        await _repo.SaveAsync(household, ct);
        await _events.PublishAsync(new HouseholdCreated(household.HouseholdId, cmd.UserId), ct);
        return new CreateHouseholdResult(household.HouseholdId);
    }
}
