namespace MonthlyBudget.ForecastEngine.Domain.Exceptions;
public class SnapshotImmutableException : ForecastDomainException
{
    public SnapshotImmutableException() : base("This forecast is a snapshot and cannot be modified.") { }
}
