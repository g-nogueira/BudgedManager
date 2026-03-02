using MonthlyBudget.BudgetManagement.Domain.Entities;
namespace MonthlyBudget.BudgetManagement.Infrastructure.Dto;
public record CreateBudgetRequest(string YearMonth);
public record RolloverMonthRequest(string TargetYearMonth);
public record AddIncomeRequest(string Name, decimal Amount);
public record UpdateIncomeRequest(string Name, decimal Amount);
public record AddExpenseRequest(string Name, ExpenseCategory Category, int? DayOfMonth, bool IsSpread, decimal Amount);
public record UpdateExpenseRequest(string Name, ExpenseCategory Category, int? DayOfMonth, bool IsSpread, decimal Amount);
public record ToggleExclusionRequest(bool IsExcluded);
