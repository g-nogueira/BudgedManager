using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.GenerateForecast;
public sealed record GenerateForecastCommand(Guid BudgetId, Guid HouseholdId) : IRequest<GenerateForecastResult>;
public sealed record GenerateForecastResult(Guid ForecastId, string VersionLabel, decimal EndOfMonthBalance, int DayCount);
