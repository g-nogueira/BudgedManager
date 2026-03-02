using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.Reforecast;
public sealed record ReforecastCommand(Guid BudgetId, Guid HouseholdId, Guid ParentForecastId, int StartDay, decimal ActualBalance, string VersionLabel) : IRequest<ReforecastResult>;
public sealed record ReforecastResult(Guid ForecastId, decimal EndOfMonthBalance);
