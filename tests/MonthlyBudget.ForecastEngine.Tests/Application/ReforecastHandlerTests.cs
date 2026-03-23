using MonthlyBudget.ForecastEngine.Application.Features.Reforecast;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
using MonthlyBudget.ForecastEngine.Domain.Services;

namespace MonthlyBudget.ForecastEngine.Tests.Application;

public sealed class ReforecastHandlerTests
{
    [Fact]
    public async Task Handle_NullAdjustments_UsesParentSnapshotsUnchanged()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var parent = CreateParentForecast(budgetId, householdId, FixedExpense(Guid.NewGuid(), 10, 1200m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        await sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", null), CancellationToken.None);

        Assert.NotNull(repo.SavedReforecast);
        var snapshot = Assert.Single(repo.SavedReforecast!.ExpenseSnapshots);
        Assert.Equal(1200m, snapshot.Amount);
    }

    [Fact]
    public async Task Handle_EmptyAdjustments_UsesParentSnapshotsUnchanged()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var parent = CreateParentForecast(budgetId, householdId, FixedExpense(Guid.NewGuid(), 10, 1200m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        await sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", Array.Empty<ExpenseAdjustment>()), CancellationToken.None);

        Assert.NotNull(repo.SavedReforecast);
        var snapshot = Assert.Single(repo.SavedReforecast!.ExpenseSnapshots);
        Assert.Equal(1200m, snapshot.Amount);
    }

    [Fact]
    public async Task Handle_ModifyAdjustment_AppliesNewAmountToMatchingSnapshot()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var parent = CreateParentForecast(budgetId, householdId, FixedExpense(expenseId, 10, 1200m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        var adjustments = new[]
        {
            new ExpenseAdjustment(expenseId, "MODIFY", 1000m, null, null, null, null)
        };

        await sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", adjustments), CancellationToken.None);

        Assert.NotNull(repo.SavedReforecast);
        Assert.Equal(1000m, repo.SavedReforecast!.ExpenseSnapshots.Single().Amount);
    }

    [Fact]
    public async Task Handle_RemoveAdjustment_ExcludesSnapshotFromForecast()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var expenseA = Guid.NewGuid();
        var expenseB = Guid.NewGuid();
        var parent = CreateParentForecast(
            budgetId,
            householdId,
            FixedExpense(expenseA, 10, 1200m),
            FixedExpense(expenseB, 15, 100m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        var adjustments = new[]
        {
            new ExpenseAdjustment(expenseB, "REMOVE", null, null, null, null, null)
        };

        await sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", adjustments), CancellationToken.None);

        Assert.NotNull(repo.SavedReforecast);
        var snapshot = Assert.Single(repo.SavedReforecast!.ExpenseSnapshots);
        Assert.Equal(expenseA, snapshot.OriginalExpenseId);
    }

    [Fact]
    public async Task Handle_AddAdjustment_CreatesNewSnapshot()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var expenseA = Guid.NewGuid();
        var parent = CreateParentForecast(budgetId, householdId, FixedExpense(expenseA, 10, 1200m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, "Car Repair", "VARIABLE", 20, false)
        };

        await sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", adjustments), CancellationToken.None);

        Assert.NotNull(repo.SavedReforecast);
        Assert.Equal(2, repo.SavedReforecast!.ExpenseSnapshots.Count);
        Assert.Contains(repo.SavedReforecast.ExpenseSnapshots, s => s.Name == "Car Repair" && s.Amount == 200m);
    }

    [Fact]
    public async Task Handle_CombinedAdjustments_ModifyRemoveAdd()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var expenseA = Guid.NewGuid();
        var expenseB = Guid.NewGuid();
        var parent = CreateParentForecast(
            budgetId,
            householdId,
            FixedExpense(expenseA, 10, 1200m),
            FixedExpense(expenseB, 15, 100m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        var adjustments = new[]
        {
            new ExpenseAdjustment(expenseA, "MODIFY", 900m, null, null, null, null),
            new ExpenseAdjustment(expenseB, "REMOVE", null, null, null, null, null),
            new ExpenseAdjustment(null, "ADD", 200m, "Car Repair", "VARIABLE", 20, false)
        };

        await sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", adjustments), CancellationToken.None);

        Assert.NotNull(repo.SavedReforecast);
        Assert.Equal(2, repo.SavedReforecast!.ExpenseSnapshots.Count);
        Assert.Contains(repo.SavedReforecast.ExpenseSnapshots, s => s.OriginalExpenseId == expenseA && s.Amount == 900m);
        Assert.Contains(repo.SavedReforecast.ExpenseSnapshots, s => s.Name == "Car Repair" && s.Amount == 200m);
    }

    [Fact]
    public async Task Handle_ModifyNonExistentSnapshot_ThrowsException()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var parent = CreateParentForecast(budgetId, householdId, FixedExpense(Guid.NewGuid(), 10, 1200m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        var adjustments = new[]
        {
            new ExpenseAdjustment(Guid.NewGuid(), "MODIFY", 1000m, null, null, null, null)
        };

        await Assert.ThrowsAsync<InvalidReforecastException>(() =>
            sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", adjustments), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RemoveNonExistentSnapshot_ThrowsException()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var parent = CreateParentForecast(budgetId, householdId, FixedExpense(Guid.NewGuid(), 10, 1200m));

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        var adjustments = new[]
        {
            new ExpenseAdjustment(Guid.NewGuid(), "REMOVE", null, null, null, null, null)
        };

        await Assert.ThrowsAsync<InvalidReforecastException>(() =>
            sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", adjustments), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AutoSnapshotsParent_BeforeCreatingReforecast()
    {
        var budgetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var parent = CreateParentForecast(budgetId, householdId, FixedExpense(Guid.NewGuid(), 10, 1200m));
        Assert.False(parent.IsSnapshot);

        var repo = new FakeForecastRepository(parent);
        var budgetPort = new FakeBudgetDataPort(budgetId, householdId, CreateBudgetData(budgetId, householdId));
        var sut = new ReforecastHandler(repo, budgetPort);

        await sut.Handle(new ReforecastCommand(budgetId, householdId, parent.ForecastId, 10, 3000m, "RF-1", null), CancellationToken.None);

        Assert.True(parent.IsSnapshot);
        Assert.Equal(2, repo.SaveCalls.Count);
    }

    private static BudgetData CreateBudgetData(Guid budgetId, Guid householdId)
    {
        return new BudgetData(budgetId, householdId, "2026-09", 5000m, Array.Empty<ExpenseSnapshotData>());
    }

    private static ForecastVersion CreateParentForecast(Guid budgetId, Guid householdId, params ExpenseSnapshot[] snapshots)
    {
        var entries = new List<DailyEntry>
        {
            DailyEntry.Create(Guid.NewGuid(), 1, 5000m, new List<DailyExpenseItem>())
        };

        return ForecastVersion.CreateReforecast(
            budgetId,
            householdId,
            Guid.NewGuid(),
            1,
            5000m,
            "Parent",
            snapshots.ToList(),
            entries);
    }

    private static ExpenseSnapshot FixedExpense(Guid expenseId, int day, decimal amount)
    {
        return ExpenseSnapshot.Create(Guid.NewGuid(), expenseId, "Expense", SnapshotCategory.FIXED, day, false, amount, false);
    }

    private sealed class FakeBudgetDataPort : IBudgetDataPort
    {
        private readonly Guid _expectedBudgetId;
        private readonly Guid _expectedHouseholdId;
        private readonly BudgetData? _data;

        public FakeBudgetDataPort(Guid expectedBudgetId, Guid expectedHouseholdId, BudgetData? data)
        {
            _expectedBudgetId = expectedBudgetId;
            _expectedHouseholdId = expectedHouseholdId;
            _data = data;
        }

        public Task<BudgetData?> GetBudgetDataAsync(Guid budgetId, Guid householdId, CancellationToken ct = default)
        {
            if (budgetId != _expectedBudgetId || householdId != _expectedHouseholdId)
            {
                return Task.FromResult<BudgetData?>(null);
            }

            return Task.FromResult(_data);
        }
    }

    private sealed class FakeForecastRepository : IForecastRepository
    {
        private readonly ForecastVersion _parent;

        public FakeForecastRepository(ForecastVersion parent)
        {
            _parent = parent;
        }

        public List<ForecastVersion> SaveCalls { get; } = new();

        public ForecastVersion? SavedReforecast => SaveCalls.LastOrDefault(x => x.ForecastType == ForecastType.REFORECAST);

        public Task<ForecastVersion?> FindByIdAsync(Guid forecastId, CancellationToken ct = default)
        {
            if (_parent.ForecastId == forecastId)
            {
                return Task.FromResult<ForecastVersion?>(_parent);
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
            SaveCalls.Add(forecast);
            return Task.CompletedTask;
        }
    }
}
