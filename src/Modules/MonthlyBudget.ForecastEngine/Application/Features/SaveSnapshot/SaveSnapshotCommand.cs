using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.SaveSnapshot;
public sealed record SaveSnapshotCommand(Guid ForecastId, Guid HouseholdId) : IRequest<SaveSnapshotResult>;
public sealed record SaveSnapshotResult(Guid ForecastId, bool IsSnapshot);

