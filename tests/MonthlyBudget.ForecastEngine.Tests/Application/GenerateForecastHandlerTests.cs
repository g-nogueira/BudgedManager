using MonthlyBudget.ForecastEngine.Application.Features.GenerateForecast;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;

namespace MonthlyBudget.ForecastEngine.Tests.Application;

public sealed class GenerateForecastHandlerTests
{
    [Fact]
    public async Task Handle_BudgetDataExists_UsesTotalIncomeAsStartBalance()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var budgetData = new BudgetData(
            budgetId,
            householdId,
            "2026-09",
            5000m,
            Array.Empty<ExpenseSnapshotData>());

        var repo = new FakeForecastRepository();
        var budgetPort = new FakeBudgetDataPort(budgetData);
        var sut = new GenerateForecastHandler(repo, budgetPort);

        var result = await sut.Handle(new GenerateForecastCommand(budgetId, householdId), CancellationToken.None);

        Assert.NotNull(repo.SavedForecast);
        Assert.Equal(5000m, repo.SavedForecast!.StartBalance);
        Assert.Equal(30, result.DayCount);
    }

    [Fact]
    public async Task Handle_BudgetDataMissing_ThrowsForecastNotFoundException()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var repo = new FakeForecastRepository();
        var budgetPort = new FakeBudgetDataPort(null);
        var sut = new GenerateForecastHandler(repo, budgetPort);

        await Assert.ThrowsAsync<ForecastNotFoundException>(() =>
            sut.Handle(new GenerateForecastCommand(budgetId, householdId), CancellationToken.None));
    }

    private sealed class FakeBudgetDataPort : IBudgetDataPort
    {
        private readonly BudgetData? _data;

        public FakeBudgetDataPort(BudgetData? data)
        {
            _data = data;
        }

        public Task<BudgetData?> GetBudgetDataAsync(Guid budgetId, Guid householdId, CancellationToken ct = default)
        {
            return Task.FromResult(_data);
        }
    }

    private sealed class FakeForecastRepository : IForecastRepository
    {
        public ForecastVersion? SavedForecast { get; private set; }

        public Task<ForecastVersion?> FindByIdAsync(Guid forecastId, CancellationToken ct = default)
        {
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
}
