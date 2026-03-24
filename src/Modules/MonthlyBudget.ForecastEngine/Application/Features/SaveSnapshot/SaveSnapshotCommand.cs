using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.SaveSnapshot;
public sealed record SaveSnapshotCommand(Guid ForecastId, Guid HouseholdId, decimal? ActualBalance) : IRequest<SaveSnapshotResult>;
public sealed record SaveSnapshotResult(Guid ForecastId, bool IsSnapshot);

