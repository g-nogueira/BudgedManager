using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.Reforecast;
public sealed record ExpenseAdjustment(
	Guid? OriginalExpenseId,
	string Action,
	decimal? NewAmount,
	string? Name,
	string? Category,
	int? DayOfMonth,
	bool? IsSpread);

public sealed record ReforecastCommand(
	Guid BudgetId,
	Guid HouseholdId,
	Guid ParentForecastId,
	int StartDay,
	decimal ActualBalance,
	string VersionLabel,
	IReadOnlyList<ExpenseAdjustment>? ExpenseAdjustments = null) : IRequest<ReforecastResult>;
public sealed record ReforecastResult(Guid ForecastId, decimal EndOfMonthBalance);
