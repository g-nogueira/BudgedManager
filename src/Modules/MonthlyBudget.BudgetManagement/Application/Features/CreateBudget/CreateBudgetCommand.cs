using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.CreateBudget;
public sealed record CreateBudgetCommand(Guid HouseholdId, string YearMonth) : IRequest<CreateBudgetResult>;
public sealed record CreateBudgetResult(Guid BudgetId, string Status);
