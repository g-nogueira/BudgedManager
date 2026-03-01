namespace MonthlyBudget.ForecastEngine.Domain.Exceptions;
public class InvalidReforecastException : ForecastDomainException
{
    public InvalidReforecastException(string message) : base(message) { }
}
