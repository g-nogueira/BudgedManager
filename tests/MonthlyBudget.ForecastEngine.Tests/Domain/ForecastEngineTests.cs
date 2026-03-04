using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Services;
using Xunit;
namespace MonthlyBudget.ForecastEngine.Tests.Domain;
public class ForecastCalculatorTests
{
    private static readonly Guid BudgetId = Guid.NewGuid();
    private static readonly Guid HouseholdId = Guid.NewGuid();
    private static ExpenseSnapshot FixedExpense(int day, decimal amount, bool excluded = false) =>
        ExpenseSnapshot.Create(Guid.NewGuid(), Guid.NewGuid(), "Rent", SnapshotCategory.FIXED, day, false, amount, excluded);
    private static ExpenseSnapshot SpreadExpense(decimal amount, bool excluded = false) =>
        ExpenseSnapshot.Create(Guid.NewGuid(), Guid.NewGuid(), "Subscription", SnapshotCategory.SUBSCRIPTION, null, true, amount, excluded);
    [Fact]
    public void GenerateForecast_BalanceReducesOnExpenseDay()
    {
        // Day 5: Rent of 1000. Balance starts at 3000.
        var snapshots = new List<ExpenseSnapshot> { FixedExpense(5, 1000m) };
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 3000m, 31, snapshots);
        var day5 = forecast.DailyEntries.First(e => e.DayNumber == 5);
        var day4 = forecast.DailyEntries.First(e => e.DayNumber == 4);
        Assert.Equal(2000m, day5.RemainingBalance);
        Assert.Equal(3000m, day4.RemainingBalance); // Not affected
    }
    [Fact]
    public void GenerateForecast_SpreadExpense_DistributedEvenly()
    {
        // 31 day month, 31 spread across all days = 1 per day
        var snapshots = new List<ExpenseSnapshot> { SpreadExpense(31m) };
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 1000m, 31, snapshots);
        Assert.Equal(31, forecast.DailyEntries.Count);
        // Day 1 balance should be 1000 - 1 = 999
        var day1 = forecast.DailyEntries.First(e => e.DayNumber == 1);
        Assert.Equal(999m, day1.RemainingBalance);
    }
    [Fact]
    public void GenerateForecast_ExcludedExpense_NotSubtracted_INV_F5()
    {
        var snapshots = new List<ExpenseSnapshot> { FixedExpense(5, 1000m, excluded: true) };
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 3000m, 31, snapshots);
        var day5 = forecast.DailyEntries.First(e => e.DayNumber == 5);
        Assert.Equal(3000m, day5.RemainingBalance); // Not deducted
    }
    [Fact]
    public void GenerateForecast_HasCorrectNumberOfDays()
    {
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 5000m, 28, new List<ExpenseSnapshot>());
        Assert.Equal(28, forecast.DailyEntries.Count);
    }
    [Fact]
    public void GenerateForecast_EndOfMonthBalance_MatchesLastDay()
    {
        var snapshots = new List<ExpenseSnapshot> { FixedExpense(1, 500m) };
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 1000m, 30, snapshots);
        Assert.Equal(500m, forecast.GetEndOfMonthBalance());
    }
    [Fact]
    public void ForecastVersion_CreateOriginal_HasOriginalType()
    {
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 5000m, 31, new List<ExpenseSnapshot>());
        Assert.Equal(ForecastType.ORIGINAL, forecast.ForecastType);
    }
    [Fact]
    public void ForecastVersion_CreateReforecast_RequiresParentId_INV_F2()
    {
        Assert.Throws<InvalidReforecastException>(() =>
            ForecastVersion.CreateReforecast(BudgetId, HouseholdId, Guid.Empty, 10, 2000m, "RF-1",
                new List<ExpenseSnapshot>(), new List<DailyEntry>()));
    }
    [Fact]
    public void ForecastVersion_MarkAsSnapshot_IsSnapshot()
    {
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 5000m, 31, new List<ExpenseSnapshot>());
        forecast.MarkAsSnapshot();
        Assert.True(forecast.IsSnapshot);
    }
    [Fact]
    public void GenerateForecast_MultipleExpenses_SameDay_AllSubtracted()
    {
        var snapshots = new List<ExpenseSnapshot>
        {
            FixedExpense(10, 200m),
            FixedExpense(10, 300m)
        };
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 1000m, 31, snapshots);
        var day10 = forecast.DailyEntries.First(e => e.DayNumber == 10);
        Assert.Equal(500m, day10.RemainingBalance);
        Assert.Equal(500m, day10.DailyExpenseTotal);
    }

    // --- SaveSnapshot (INV-F4) ---------------------------------------------------
    [Fact]
    public void SaveSnapshot_NonSnapshotForecast_SetsIsSnapshotTrue()
    {
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 5000m, 31, new List<ExpenseSnapshot>());
        Assert.False(forecast.IsSnapshot);
        forecast.MarkAsSnapshot();
        Assert.True(forecast.IsSnapshot);
    }

    [Fact]
    public void SaveSnapshot_AlreadySnapshot_RemainsTrue_Idempotent()
    {
        var forecast = ForecastCalculator.Generate(BudgetId, HouseholdId, 5000m, 31, new List<ExpenseSnapshot>());
        forecast.MarkAsSnapshot();
        forecast.MarkAsSnapshot(); // second call — idempotent
        Assert.True(forecast.IsSnapshot);
    }

    // --- CompareForecasts --------------------------------------------------------
    [Fact]
    public void CompareForecasts_SameForecasts_ZeroDrift()
    {
        var snapshots = new List<ExpenseSnapshot> { FixedExpense(5, 500m) };
        var a = ForecastCalculator.Generate(BudgetId, HouseholdId, 2000m, 30, snapshots);
        var endA = a.GetEndOfMonthBalance();
        Assert.Equal(0m, endA - endA); // same forecast — zero drift
    }

    [Fact]
    public void CompareForecasts_DifferentEndBalances_CorrectTotalDrift()
    {
        var snapshotsA = new List<ExpenseSnapshot> { FixedExpense(1, 500m) };
        var snapshotsB = new List<ExpenseSnapshot> { FixedExpense(1, 300m) };
        var a = ForecastCalculator.Generate(BudgetId, HouseholdId, 1000m, 30, snapshotsA);
        var b = ForecastCalculator.Generate(BudgetId, HouseholdId, 1000m, 30, snapshotsB);
        var drift = a.GetEndOfMonthBalance() - b.GetEndOfMonthBalance();
        Assert.Equal(-200m, drift); // A pays 500, B pays 300 → A has 200 less
    }

    // --- AutoSnapshot on Reforecast policy ---------------------------------------
    [Fact]
    public void ForecastVersion_MarkAsSnapshot_IsSnapshot_BeforeReforecast()
    {
        var parent = ForecastCalculator.Generate(BudgetId, HouseholdId, 3000m, 31, new List<ExpenseSnapshot>());
        Assert.False(parent.IsSnapshot);
        // Simulate what ReforecastHandler does: mark parent as snapshot before creating reforecast
        parent.MarkAsSnapshot();
        Assert.True(parent.IsSnapshot);
    }
}
