using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.RolloverMonth;
public sealed record RolloverMonthCommand(Guid BudgetId, Guid HouseholdId, string TargetYearMonth) : IRequest<RolloverMonthResult>;
public sealed record RolloverMonthResult(Guid NewBudgetId);
