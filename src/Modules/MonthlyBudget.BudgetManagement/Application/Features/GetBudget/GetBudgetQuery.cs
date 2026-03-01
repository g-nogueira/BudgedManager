using MediatR;
using MonthlyBudget.BudgetManagement.Domain.Entities;
namespace MonthlyBudget.BudgetManagement.Application.Features.GetBudget;
public sealed record GetBudgetByIdQuery(Guid BudgetId, Guid HouseholdId) : IRequest<BudgetDto?>;
public sealed record GetBudgetByMonthQuery(Guid HouseholdId, string YearMonth) : IRequest<BudgetDto?>;
public sealed record BudgetDto(
    Guid BudgetId, Guid HouseholdId, string YearMonth, string Status,
    IReadOnlyList<IncomeSourceDto> IncomeSources,
    IReadOnlyList<ExpenseDto> Expenses,
    decimal TotalIncome, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record IncomeSourceDto(Guid IncomeId, string Name, decimal Amount);
public sealed record ExpenseDto(Guid ExpenseId, string Name, string Category, int? DayOfMonth, bool IsSpread, decimal Amount, bool IsExcluded);
