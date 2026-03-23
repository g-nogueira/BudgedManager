using MonthlyBudget.ForecastEngine.Application.Features.Reforecast;

namespace MonthlyBudget.ForecastEngine.Tests.Application;

public sealed class ReforecastValidatorTests
{
    private readonly ReforecastValidator _sut = new();

    [Fact]
    public void Should_Pass_ValidCommandWithNoAdjustments()
    {
        var result = _sut.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Fail_EmptyBudgetId()
    {
        var result = _sut.Validate(ValidCommand() with { BudgetId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReforecastCommand.BudgetId));
    }

    [Fact]
    public void Should_Fail_EmptyHouseholdId()
    {
        var result = _sut.Validate(ValidCommand() with { HouseholdId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReforecastCommand.HouseholdId));
    }

    [Fact]
    public void Should_Fail_EmptyParentForecastId()
    {
        var result = _sut.Validate(ValidCommand() with { ParentForecastId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReforecastCommand.ParentForecastId));
    }

    [Fact]
    public void Should_Fail_StartDayZero()
    {
        var result = _sut.Validate(ValidCommand() with { StartDay = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReforecastCommand.StartDay));
    }

    [Fact]
    public void Should_Fail_NegativeActualBalance()
    {
        var result = _sut.Validate(ValidCommand() with { ActualBalance = -1m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReforecastCommand.ActualBalance));
    }

    [Fact]
    public void Should_Fail_EmptyVersionLabel()
    {
        var result = _sut.Validate(ValidCommand() with { VersionLabel = string.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReforecastCommand.VersionLabel));
    }

    [Fact]
    public void Should_Fail_VersionLabelTooLong()
    {
        var result = _sut.Validate(ValidCommand() with { VersionLabel = new string('X', 51) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReforecastCommand.VersionLabel));
    }

    [Fact]
    public void Should_Fail_InvalidAction()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(Guid.NewGuid(), "INVALID", 95m, null, null, null, null)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Action", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_ModifyMissingOriginalExpenseId()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "MODIFY", 95m, null, null, null, null)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("OriginalExpenseId", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_ModifyMissingNewAmount()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(Guid.NewGuid(), "MODIFY", null, null, null, null, null)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("NewAmount", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_RemoveMissingOriginalExpenseId()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "REMOVE", null, null, null, null, null)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("OriginalExpenseId", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_AddMissingName()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, null, "VARIABLE", 20, false)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Name", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_AddMissingCategory()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, "Car Repair", null, 20, false)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Category", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_AddInvalidCategory()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, "Car Repair", "WRONG", 20, false)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Category", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_AddMissingNewAmount()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", null, "Car Repair", "VARIABLE", 20, false)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("NewAmount", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_AddSpreadWithDayOfMonth()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, "Car Repair", "VARIABLE", 10, true)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("spread expenses require DayOfMonth null", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Fail_AddNonSpreadWithoutDayOfMonth()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, "Car Repair", "VARIABLE", null, false)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("non-spread expenses require DayOfMonth", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_Pass_ValidModifyAdjustment()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(Guid.NewGuid(), "MODIFY", 95m, null, null, null, null)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Pass_ValidRemoveAdjustment()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(Guid.NewGuid(), "REMOVE", null, null, null, null, null)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Pass_ValidAddAdjustment()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, "Car Repair", "VARIABLE", 20, false)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Pass_ValidAddSpreadAdjustment()
    {
        var adjustments = new[]
        {
            new ExpenseAdjustment(null, "ADD", 200m, "Utilities", "FIXED", null, true)
        };

        var result = _sut.Validate(ValidCommand(adjustments));

        Assert.True(result.IsValid);
    }

    private static ReforecastCommand ValidCommand(IReadOnlyList<ExpenseAdjustment>? adjustments = null)
    {
        return new ReforecastCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            20,
            3100m,
            "Re-forecast Mar 20",
            adjustments);
    }
}