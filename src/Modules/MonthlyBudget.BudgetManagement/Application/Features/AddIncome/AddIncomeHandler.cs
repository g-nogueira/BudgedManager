using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
namespace MonthlyBudget.BudgetManagement.Application.Features.AddIncome;
public sealed class AddIncomeHandler : IRequestHandler<AddIncomeCommand, AddIncomeResult>
{
    private readonly IBudgetRepository _repo; private readonly IBudgetEventPublisher _pub;
    public AddIncomeHandler(IBudgetRepository repo, IBudgetEventPublisher pub) { _repo = repo; _pub = pub; }
    public async Task<AddIncomeResult> Handle(AddIncomeCommand cmd, CancellationToken ct)
    {
        var budget = await _repo.FindByIdAsync(cmd.BudgetId, ct) ?? throw new BudgetNotFoundException(cmd.BudgetId);
        if (budget.HouseholdId != cmd.HouseholdId) throw new BudgetNotFoundException(cmd.BudgetId);
        var income = budget.AddIncome(cmd.Name, cmd.Amount);
        await _repo.SaveAsync(budget, ct);
        foreach (var evt in budget.GetDomainEvents()) await _pub.PublishAsync(evt, ct);
        budget.ClearDomainEvents();
        return new AddIncomeResult(income.IncomeId);
    }
}
