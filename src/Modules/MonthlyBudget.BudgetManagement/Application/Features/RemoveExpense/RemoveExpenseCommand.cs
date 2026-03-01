using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.RemoveExpense;
public sealed record RemoveExpenseCommand(Guid BudgetId, Guid HouseholdId, Guid ExpenseId) : IRequest<Unit>;
