namespace MonthlyBudget.ForecastEngine.Infrastructure.Dto;
public record SaveSnapshotRequest(decimal? ActualBalance);

public record ExpenseAdjustmentDto(
	Guid? OriginalExpenseId,
	string Action,
	decimal? NewAmount,
	string? Name,
	string? Category,
	int? DayOfMonth,
	bool? IsSpread);

public record ReforecastRequest(
	int StartDay,
	decimal ActualBalance,
	string VersionLabel,
	List<ExpenseAdjustmentDto>? ExpenseAdjustments);
