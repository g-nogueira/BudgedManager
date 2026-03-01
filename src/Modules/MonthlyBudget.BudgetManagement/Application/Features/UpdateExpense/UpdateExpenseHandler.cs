using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
namespace MonthlyBudget.BudgetManagement.Application.Features.UpdateExpense;
public sealed class UpdateExpenseHandler : IRequestHandler<UpdateExpenseCommand, Unit>
{
    private readonly IBudgetRepository _repo; private readonly IBudgetEventPublisher _pub;
    public UpdateExpenseHandler(IBudgetRepository repo, IBudgetEventPublisher pub) { _repo = repo; _pub = pub; }
    public async Task<Unit> Handle(UpdateExpenseCommand cmd, CancellationToken ct)
    {
        var budget = await _repo.FindByIdAsync(cmd.BudgetId, ct) ?? throw new BudgetNotFoundException(cmd.BudgetId);
        if (budget.HouseholdId != cmd.HouseholdId) throw new BudgetNotFoundException(cmd.BudgetId);
        budget.UpdateExpense(cmd.ExpenseId, cmd.Name, cmd.Category, cmd.DayOfMonth, cmd.IsSpread, cmd.Amount);
        await _repo.SaveAsync(budget, ct);
        foreach (var evt in budget.GetDomainEvents()) await _pub.PublishAsync(evt, ct);
        budget.ClearDomainEvents();
        return Unit.Value;
    }
}
