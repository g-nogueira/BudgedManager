namespace MonthlyBudget.ForecastEngine.Infrastructure.Dto;
public record ReforecastRequest(int StartDay, decimal ActualBalance, string VersionLabel);
