namespace MonthlyBudget.ForecastEngine.Infrastructure.Dto;
public record GenerateForecastRequest(decimal StartBalance);
public record ReforecastRequest(int StartDay, decimal ActualBalance, string VersionLabel);
