using MediatR;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
namespace MonthlyBudget.ForecastEngine.Application.Features.SaveSnapshot;
public sealed class SaveSnapshotHandler : IRequestHandler<SaveSnapshotCommand, SaveSnapshotResult>
{
    private readonly IForecastRepository _repo;
    private readonly IForecastEventPublisher _publisher;
    public SaveSnapshotHandler(IForecastRepository repo, IForecastEventPublisher publisher)
    { _repo = repo; _publisher = publisher; }
    public async Task<SaveSnapshotResult> Handle(SaveSnapshotCommand cmd, CancellationToken ct)
    {
        var forecast = await _repo.FindByIdAsync(cmd.ForecastId, ct);
        if (forecast == null || forecast.HouseholdId != cmd.HouseholdId)
            throw new ForecastNotFoundException(cmd.ForecastId);
        forecast.MarkAsSnapshot();
        await _repo.SaveAsync(forecast, ct);
        foreach (var evt in forecast.GetDomainEvents())
            await _publisher.PublishAsync(evt, ct);
        forecast.ClearDomainEvents();
        return new SaveSnapshotResult(forecast.ForecastId, forecast.IsSnapshot);
    }
}

