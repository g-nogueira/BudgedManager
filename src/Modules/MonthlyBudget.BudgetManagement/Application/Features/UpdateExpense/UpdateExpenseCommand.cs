using MediatR;
using MonthlyBudget.BudgetManagement.Domain.Entities;
namespace MonthlyBudget.BudgetManagement.Application.Features.UpdateExpense;
public sealed record UpdateExpenseCommand(Guid BudgetId, Guid HouseholdId, Guid ExpenseId, string Name, ExpenseCategory Category, int? DayOfMonth, bool IsSpread, decimal Amount) : IRequest<Unit>;
