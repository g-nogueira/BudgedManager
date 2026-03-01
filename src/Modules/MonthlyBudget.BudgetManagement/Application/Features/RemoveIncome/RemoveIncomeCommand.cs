using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.RemoveIncome;
public sealed record RemoveIncomeCommand(Guid BudgetId, Guid HouseholdId, Guid IncomeId) : IRequest<Unit>;
