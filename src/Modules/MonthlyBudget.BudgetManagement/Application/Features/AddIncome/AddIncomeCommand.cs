using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.AddIncome;
public sealed record AddIncomeCommand(Guid BudgetId, Guid HouseholdId, string Name, decimal Amount) : IRequest<AddIncomeResult>;
public sealed record AddIncomeResult(Guid IncomeId);
