using MediatR;

namespace MonthlyBudget.BudgetManagement.Application.Features.CloseBudget;

public sealed record CloseBudgetCommand(Guid BudgetId, Guid HouseholdId) : IRequest<CloseBudgetResult>;

public sealed record CloseBudgetResult(Guid BudgetId, string Status);