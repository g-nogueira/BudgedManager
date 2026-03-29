using MediatR;
using MonthlyBudget.BudgetManagement.Domain.Repositories;

namespace MonthlyBudget.BudgetManagement.Application.Features.GetBudget;

public sealed class GetBudgetsByHouseholdHandler : IRequestHandler<GetBudgetsByHouseholdQuery, IReadOnlyList<BudgetSummaryDto>>
{
    private readonly IBudgetRepository _repo;

    public GetBudgetsByHouseholdHandler(IBudgetRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<BudgetSummaryDto>> Handle(GetBudgetsByHouseholdQuery q, CancellationToken ct)
    {
        var budgets = await _repo.FindAllByHouseholdAsync(q.HouseholdId, ct);

        return budgets
            .Select(budget => new BudgetSummaryDto(
                budget.BudgetId,
                budget.YearMonth,
                budget.Status.ToString(),
                budget.GetTotalIncome(),
                budget.Expenses.Sum(expense => expense.Amount),
                budget.CreatedAt))
            .ToList();
    }
}