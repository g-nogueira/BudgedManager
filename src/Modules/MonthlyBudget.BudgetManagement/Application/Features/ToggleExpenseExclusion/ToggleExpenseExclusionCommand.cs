using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.ToggleExpenseExclusion;
public sealed record ToggleExpenseExclusionCommand(Guid BudgetId, Guid HouseholdId, Guid ExpenseId, bool IsExcluded) : IRequest<Unit>;
