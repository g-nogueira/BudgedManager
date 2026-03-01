using MediatR;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
namespace MonthlyBudget.BudgetManagement.Application.Features.GetBudget;
public sealed class GetBudgetByIdHandler : IRequestHandler<GetBudgetByIdQuery, BudgetDto?>
{
    private readonly IBudgetRepository _repo;
    public GetBudgetByIdHandler(IBudgetRepository repo) { _repo = repo; }
    public async Task<BudgetDto?> Handle(GetBudgetByIdQuery q, CancellationToken ct)
    {
        var budget = await _repo.FindByIdAsync(q.BudgetId, ct);
        if (budget == null || budget.HouseholdId != q.HouseholdId) return null;
        return MapToDto(budget);
    }
    internal static BudgetDto MapToDto(Domain.Entities.MonthlyBudget budget) => new(
        budget.BudgetId, budget.HouseholdId, budget.YearMonth, budget.Status.ToString(),
        budget.IncomeSources.Select(i => new IncomeSourceDto(i.IncomeId, i.Name, i.Amount)).ToList(),
        budget.Expenses.Select(e => new ExpenseDto(e.ExpenseId, e.Name, e.Category.ToString(), e.DayOfMonth, e.IsSpread, e.Amount, e.IsExcluded)).ToList(),
        budget.GetTotalIncome(), budget.CreatedAt, budget.UpdatedAt);
}
public sealed class GetBudgetByMonthHandler : IRequestHandler<GetBudgetByMonthQuery, BudgetDto?>
{
    private readonly IBudgetRepository _repo;
    public GetBudgetByMonthHandler(IBudgetRepository repo) { _repo = repo; }
    public async Task<BudgetDto?> Handle(GetBudgetByMonthQuery q, CancellationToken ct)
    {
        var budget = await _repo.FindByHouseholdAndMonthAsync(q.HouseholdId, q.YearMonth, ct);
        return budget == null ? null : GetBudgetByIdHandler.MapToDto(budget);
    }
}
