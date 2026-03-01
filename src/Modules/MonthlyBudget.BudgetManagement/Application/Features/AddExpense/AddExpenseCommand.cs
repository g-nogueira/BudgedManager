using MediatR;
using MonthlyBudget.BudgetManagement.Domain.Entities;
namespace MonthlyBudget.BudgetManagement.Application.Features.AddExpense;
public sealed record AddExpenseCommand(Guid BudgetId, Guid HouseholdId, string Name, ExpenseCategory Category, int? DayOfMonth, bool IsSpread, decimal Amount) : IRequest<AddExpenseResult>;
public sealed record AddExpenseResult(Guid ExpenseId);
