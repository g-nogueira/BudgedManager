using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
namespace MonthlyBudget.BudgetManagement.Application.Features.RolloverMonth;
public sealed class RolloverMonthHandler : IRequestHandler<RolloverMonthCommand, RolloverMonthResult>
{
    private readonly IBudgetRepository _repo; private readonly IBudgetEventPublisher _pub;
    public RolloverMonthHandler(IBudgetRepository repo, IBudgetEventPublisher pub) { _repo = repo; _pub = pub; }
    public async Task<RolloverMonthResult> Handle(RolloverMonthCommand cmd, CancellationToken ct)
    {
        var source = await _repo.FindByIdAsync(cmd.BudgetId, ct) ?? throw new BudgetNotFoundException(cmd.BudgetId);
        if (source.HouseholdId != cmd.HouseholdId) throw new BudgetNotFoundException(cmd.BudgetId);
        if (await _repo.ExistsForHouseholdAndMonthAsync(cmd.HouseholdId, cmd.TargetYearMonth, ct))
            throw new BudgetAlreadyExistsException(cmd.TargetYearMonth);
        var newBudget = source.RolloverTo(cmd.TargetYearMonth);
        await _repo.SaveAsync(newBudget, ct);
        foreach (var evt in newBudget.GetDomainEvents()) await _pub.PublishAsync(evt, ct);
        newBudget.ClearDomainEvents();
        return new RolloverMonthResult(newBudget.BudgetId);
    }
}
