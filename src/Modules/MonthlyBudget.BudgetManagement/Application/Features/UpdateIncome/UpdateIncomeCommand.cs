using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.UpdateIncome;
public sealed record UpdateIncomeCommand(Guid BudgetId, Guid HouseholdId, Guid IncomeId, string Name, decimal Amount) : IRequest<Unit>;
