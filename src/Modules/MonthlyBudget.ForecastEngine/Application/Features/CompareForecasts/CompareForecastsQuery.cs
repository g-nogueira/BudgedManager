using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.CompareForecasts;

public sealed record CompareForecastsQuery(Guid ForecastAId, Guid ForecastBId, Guid HouseholdId)
    : IRequest<ComparisonResult>;

public sealed record ComparisonResult(
    Guid ForecastAId,
    Guid ForecastBId,
    string LabelA,
    string LabelB,
    decimal EndBalanceA,
    decimal EndBalanceB,
    decimal TotalDrift,
    IReadOnlyList<DayVariance> DayVariances,
    IReadOnlyList<ExpenseChange> ExpenseChanges);

public sealed record DayVariance(int DayNumber, decimal BalanceA, decimal BalanceB, decimal Variance);

public sealed record ExpenseChange(
    string ExpenseName,
    string ChangeType,   // "ADDED", "REMOVED", "MODIFIED"
    decimal? AmountA,
    decimal? AmountB);

