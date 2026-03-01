using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
namespace MonthlyBudget.BudgetManagement.Application.Features.RemoveIncome;
public sealed class RemoveIncomeHandler : IRequestHandler<RemoveIncomeCommand, Unit>
{
    private readonly IBudgetRepository _repo; private readonly IBudgetEventPublisher _pub;
    public RemoveIncomeHandler(IBudgetRepository repo, IBudgetEventPublisher pub) { _repo = repo; _pub = pub; }
    public async Task<Unit> Handle(RemoveIncomeCommand cmd, CancellationToken ct)
    {
        var budget = await _repo.FindByIdAsync(cmd.BudgetId, ct) ?? throw new BudgetNotFoundException(cmd.BudgetId);
        if (budget.HouseholdId != cmd.HouseholdId) throw new BudgetNotFoundException(cmd.BudgetId);
        budget.RemoveIncome(cmd.IncomeId);
        await _repo.SaveAsync(budget, ct);
        foreach (var evt in budget.GetDomainEvents()) await _pub.PublishAsync(evt, ct);
        budget.ClearDomainEvents();
        return Unit.Value;
    }
}
