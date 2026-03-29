using MediatR;

namespace MonthlyBudget.BudgetManagement.Application.Features.GetBudget;

public sealed record GetBudgetsByHouseholdQuery(Guid HouseholdId) : IRequest<IReadOnlyList<BudgetSummaryDto>>;

public sealed record BudgetSummaryDto(
    Guid BudgetId,
    string YearMonth,
    string Status,
    decimal TotalIncome,
    decimal TotalExpenses,
    DateTime CreatedAt);