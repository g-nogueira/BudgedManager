using MediatR;
using MonthlyBudget.ForecastEngine.Domain.Entities;
namespace MonthlyBudget.ForecastEngine.Application.Features.GetForecast;
public sealed record GetForecastQuery(Guid ForecastId, Guid HouseholdId) : IRequest<ForecastDto?>;
public sealed record GetForecastsByBudgetQuery(Guid BudgetId, Guid HouseholdId) : IRequest<IReadOnlyList<ForecastSummaryDto>>;
public sealed record ForecastDto(Guid ForecastId, Guid BudgetId, string VersionLabel, string ForecastType, int StartDay, decimal StartBalance, decimal EndOfMonthBalance, bool IsSnapshot, IReadOnlyList<DailyEntryDto> DailyEntries);
public sealed record ForecastSummaryDto(Guid ForecastId, string VersionLabel, string ForecastType, decimal EndOfMonthBalance, bool IsSnapshot, DateTime CreatedAt);
public sealed record DailyEntryDto(int DayNumber, decimal RemainingBalance, decimal DailyExpenseTotal, IReadOnlyList<ExpenseItemDto> Breakdown);
public sealed record ExpenseItemDto(string Name, decimal Amount);
