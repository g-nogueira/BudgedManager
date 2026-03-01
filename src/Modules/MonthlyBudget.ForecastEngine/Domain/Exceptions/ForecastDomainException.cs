namespace MonthlyBudget.ForecastEngine.Domain.Exceptions;
public abstract class ForecastDomainException : Exception
{
    protected ForecastDomainException(string message) : base(message) { }
}
