namespace MonthlyBudget.ForecastEngine.Domain.Exceptions;
public class ForecastNotFoundException : ForecastDomainException
{
    public ForecastNotFoundException(Guid id) : base($"Forecast '{id}' was not found.") { }
}
