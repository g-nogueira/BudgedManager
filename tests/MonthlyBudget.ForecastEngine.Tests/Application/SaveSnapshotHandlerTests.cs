using MonthlyBudget.ForecastEngine.Application.Features.SaveSnapshot;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
using MonthlyBudget.ForecastEngine.Domain.Services;
using MonthlyBudget.SharedKernel.Events;

namespace MonthlyBudget.ForecastEngine.Tests.Application;

public sealed class SaveSnapshotHandlerTests
{
    [Fact]
    public async Task Handle_WithActualBalance_SetsBalanceBeforeSnapshot()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var forecast = ForecastCalculator.Generate(budgetId, householdId, 5000m, 31, new List<ExpenseSnapshot>());

        var repo = new FakeForecastRepository(forecast);
        var publisher = new FakeForecastEventPublisher();
        var sut = new SaveSnapshotHandler(repo, publisher);

        var result = await sut.Handle(new SaveSnapshotCommand(forecast.ForecastId, householdId, 3100m), CancellationToken.None);

        Assert.NotNull(repo.SavedForecast);
        Assert.Equal(3100m, repo.SavedForecast!.ActualBalance);
        Assert.True(repo.SavedForecast.IsSnapshot);
        Assert.True(result.IsSnapshot);
    }

    [Fact]
    public async Task Handle_WithoutActualBalance_DoesNotSetBalance()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var forecast = ForecastCalculator.Generate(budgetId, householdId, 5000m, 31, new List<ExpenseSnapshot>());

        var repo = new FakeForecastRepository(forecast);
        var publisher = new FakeForecastEventPublisher();
        var sut = new SaveSnapshotHandler(repo, publisher);

        var result = await sut.Handle(new SaveSnapshotCommand(forecast.ForecastId, householdId, null), CancellationToken.None);

        Assert.NotNull(repo.SavedForecast);
        Assert.Null(repo.SavedForecast!.ActualBalance);
        Assert.True(repo.SavedForecast.IsSnapshot);
        Assert.True(result.IsSnapshot);
    }

    [Fact]
    public async Task Handle_ForecastNotFound_ThrowsForecastNotFoundException()
    {
        var forecastId = Guid.NewGuid();
        var householdId = Guid.NewGuid();

        var repo = new FakeForecastRepository(null);
        var publisher = new FakeForecastEventPublisher();
        var sut = new SaveSnapshotHandler(repo, publisher);

        await Assert.ThrowsAsync<ForecastNotFoundException>(() =>
            sut.Handle(new SaveSnapshotCommand(forecastId, householdId, 3100m), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_HouseholdMismatch_ThrowsForecastNotFoundException()
    {
        var budgetId = Guid.NewGuid();
        var requestHouseholdId = Guid.NewGuid();
        var otherHouseholdId = Guid.NewGuid();
        var forecast = ForecastCalculator.Generate(budgetId, otherHouseholdId, 5000m, 31, new List<ExpenseSnapshot>());

        var repo = new FakeForecastRepository(forecast);
        var publisher = new FakeForecastEventPublisher();
        var sut = new SaveSnapshotHandler(repo, publisher);

        await Assert.ThrowsAsync<ForecastNotFoundException>(() =>
            sut.Handle(new SaveSnapshotCommand(forecast.ForecastId, requestHouseholdId, 3100m), CancellationToken.None));
    }

    private sealed class FakeForecastRepository : IForecastRepository
    {
        private readonly ForecastVersion? _forecast;

        public FakeForecastRepository(ForecastVersion? forecast)
        {
            _forecast = forecast;
        }

        public ForecastVersion? SavedForecast { get; private set; }

        public Task<ForecastVersion?> FindByIdAsync(Guid forecastId, CancellationToken ct = default)
        {
            if (_forecast?.ForecastId == forecastId)
            {
                return Task.FromResult<ForecastVersion?>(_forecast);
            }

            return Task.FromResult<ForecastVersion?>(null);
        }

        public Task<IReadOnlyList<ForecastVersion>> FindAllByBudgetAsync(Guid budgetId, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<ForecastVersion>>(Array.Empty<ForecastVersion>());
        }

        public Task<ForecastVersion?> FindLatestByBudgetAsync(Guid budgetId, CancellationToken ct = default)
        {
            return Task.FromResult<ForecastVersion?>(null);
        }

        public Task SaveAsync(ForecastVersion forecast, CancellationToken ct = default)
        {
            SavedForecast = forecast;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeForecastEventPublisher : IForecastEventPublisher
    {
        public List<IDomainEvent> PublishedEvents { get; } = new();

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
        {
            PublishedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }
}