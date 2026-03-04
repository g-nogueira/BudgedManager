using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.ActivateBudget;
public sealed record ActivateBudgetCommand(Guid BudgetId, Guid HouseholdId) : IRequest<ActivateBudgetResult>;
public sealed record ActivateBudgetResult(Guid BudgetId, string Status);

