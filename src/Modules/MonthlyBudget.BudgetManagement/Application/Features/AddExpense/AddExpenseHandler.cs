using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
namespace MonthlyBudget.BudgetManagement.Application.Features.AddExpense;
public sealed class AddExpenseHandler : IRequestHandler<AddExpenseCommand, AddExpenseResult>
{
    private readonly IBudgetRepository _repo; private readonly IBudgetEventPublisher _pub;
    public AddExpenseHandler(IBudgetRepository repo, IBudgetEventPublisher pub) { _repo = repo; _pub = pub; }
    public async Task<AddExpenseResult> Handle(AddExpenseCommand cmd, CancellationToken ct)
    {
        var budget = await _repo.FindByIdAsync(cmd.BudgetId, ct) ?? throw new BudgetNotFoundException(cmd.BudgetId);
        if (budget.HouseholdId != cmd.HouseholdId) throw new BudgetNotFoundException(cmd.BudgetId);
        var expense = budget.AddExpense(cmd.Name, cmd.Category, cmd.DayOfMonth, cmd.IsSpread, cmd.Amount);
        await _repo.SaveAsync(budget, ct);
        foreach (var evt in budget.GetDomainEvents()) await _pub.PublishAsync(evt, ct);
        budget.ClearDomainEvents();
        return new AddExpenseResult(expense.ExpenseId);
    }
}
