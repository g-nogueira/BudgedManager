using MonthlyBudget.BudgetManagement.Domain.Entities;
using MonthlyBudget.BudgetManagement.Domain.Events;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using Xunit;
using Budget = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;

namespace MonthlyBudget.BudgetManagement.Tests.Domain;
public class MonthlyBudgetTests
{
    private static readonly Guid HouseholdId = Guid.NewGuid();
    private const string YearMonth = "2026-03";
    // --- Create ------------------------------------------------------------------
    [Fact]
    public void Create_ValidInputs_ReturnsDraftBudgetWithEvent()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Equal(HouseholdId, budget.HouseholdId);
        Assert.Equal(YearMonth, budget.YearMonth);
        Assert.Equal(BudgetStatus.DRAFT, budget.Status);
        Assert.Single(budget.GetDomainEvents().OfType<BudgetCreated>());
    }
    [Theory]
    [InlineData("")]
    [InlineData("2026-3")]
    [InlineData("26-03")]
    [InlineData("2026/03")]
    public void Create_InvalidYearMonth_Throws(string invalidYearMonth)
    {
        Assert.Throws<ArgumentException>(() =>
            Budget.Create(HouseholdId, invalidYearMonth));
    }
    // --- Activate (INV-B1, INV-B6) -----------------------------------------------
    [Fact]
    public void Activate_WithIncomeSources_TransitionsToActive()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        budget.AddIncome("Salary", 5000m);
        budget.Activate();
        Assert.Equal(BudgetStatus.ACTIVE, budget.Status);
        Assert.Single(budget.GetDomainEvents().OfType<BudgetActivated>());
    }
    [Fact]
    public void Activate_WithNoIncomeSources_ThrowsInsufficientIncomeException()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<InsufficientIncomeException>(() => budget.Activate());
    }
    [Fact]
    public void Activate_AlreadyActive_ThrowsInvalidTransition()
    {
        var budget = CreateActiveBudget();
        Assert.Throws<InvalidBudgetStatusTransitionException>(() => budget.Activate());
    }
    [Fact]
    public void Close_FromActive_TransitionsToClosed()
    {
        var budget = CreateActiveBudget();
        budget.Close();
        Assert.Equal(BudgetStatus.CLOSED, budget.Status);
        Assert.Single(budget.GetDomainEvents().OfType<BudgetClosed>());
    }
    [Fact]
    public void Close_FromDraft_ThrowsInvalidTransition()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<InvalidBudgetStatusTransitionException>(() => budget.Close());
    }
    [Fact]
    public void Close_FromClosed_ThrowsInvalidTransition()
    {
        var budget = CreateActiveBudget();
        budget.Close();
        Assert.Throws<InvalidBudgetStatusTransitionException>(() => budget.Close());
    }
    // --- Income (INV-B5, INV-B8) -------------------------------------------------
    [Fact]
    public void AddIncome_ValidData_AddsIncomeAndRaisesEvent()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        var income = budget.AddIncome("Salary", 5000m);
        Assert.Single(budget.IncomeSources);
        Assert.Equal("Salary", income.Name);
        Assert.Equal(5000m, income.Amount);
        Assert.Single(budget.GetDomainEvents().OfType<IncomeSourceAdded>());
    }
    [Fact]
    public void AddIncome_ZeroAmount_ThrowsArgumentException()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<ArgumentException>(() => budget.AddIncome("Salary", 0m));
    }
    [Fact]
    public void AddIncome_NegativeAmount_ThrowsArgumentException()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<ArgumentException>(() => budget.AddIncome("Salary", -100m));
    }
    [Fact]
    public void AddIncome_ToBudgetClosed_ThrowsBudgetNotModifiable()
    {
        var budget = CreateActiveBudget();
        budget.Close();
        Assert.Throws<BudgetNotModifiableException>(() => budget.AddIncome("Salary", 1000m));
    }
    [Fact]
    public void RemoveIncome_ExistingIncome_RemovesAndRaisesEvent()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        var income = budget.AddIncome("Salary", 5000m);
        budget.RemoveIncome(income.IncomeId);
        Assert.Empty(budget.IncomeSources);
        Assert.Single(budget.GetDomainEvents().OfType<IncomeSourceRemoved>());
    }
    [Fact]
    public void RemoveIncome_NotFound_ThrowsIncomeSourceNotFoundException()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<IncomeSourceNotFoundException>(() => budget.RemoveIncome(Guid.NewGuid()));
    }
    // --- Expense (INV-B2, B3, B4, B5, B8) ---------------------------------------
    [Fact]
    public void AddExpense_SpreadWithNullDay_AddsExpense()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        var expense = budget.AddExpense("Netflix", ExpenseCategory.SUBSCRIPTION, null, true, 15m);
        Assert.Single(budget.Expenses);
        Assert.True(expense.IsSpread);
        Assert.Null(expense.DayOfMonth);
    }
    [Fact]
    public void AddExpense_SpecificDayNotSpread_AddsExpense()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        var expense = budget.AddExpense("Rent", ExpenseCategory.FIXED, 1, false, 1200m);
        Assert.Single(budget.Expenses);
        Assert.Equal(1, expense.DayOfMonth);
    }
    [Fact]
    public void AddExpense_SpreadWithDaySet_ThrowsInvalidExpenseDay_INV_B3()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<InvalidExpenseDayException>(() =>
            budget.AddExpense("Spread", ExpenseCategory.VARIABLE, 5, true, 100m));
    }
    [Fact]
    public void AddExpense_NotSpreadWithNullDay_ThrowsInvalidExpenseDay_INV_B4()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<InvalidExpenseDayException>(() =>
            budget.AddExpense("Rent", ExpenseCategory.FIXED, null, false, 100m));
    }
    [Fact]
    public void AddExpense_DayExceedsMonthLength_ThrowsInvalidExpenseDay_INV_B2()
    {
        var budget = Budget.Create(HouseholdId, "2026-02"); // Feb = 28 days
        Assert.Throws<InvalidExpenseDayException>(() =>
            budget.AddExpense("Rent", ExpenseCategory.FIXED, 31, false, 100m));
    }
    [Fact]
    public void AddExpense_Day28InFebruary_IsValid()
    {
        var budget = Budget.Create(HouseholdId, "2026-02");
        var expense = budget.AddExpense("Rent", ExpenseCategory.FIXED, 28, false, 1200m);
        Assert.Equal(28, expense.DayOfMonth);
    }
    [Fact]
    public void ToggleExpenseExclusion_ExistingExpense_TogglesAndRaisesEvent()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        var expense = budget.AddExpense("Rent", ExpenseCategory.FIXED, 1, false, 1200m);
        budget.ToggleExpenseExclusion(expense.ExpenseId, true);
        Assert.True(budget.Expenses[0].IsExcluded);
        Assert.Single(budget.GetDomainEvents().OfType<ExpenseExclusionToggled>());
    }
    [Fact]
    public void RemoveExpense_NotFound_ThrowsExpenseNotFoundException()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        Assert.Throws<ExpenseNotFoundException>(() => budget.RemoveExpense(Guid.NewGuid()));
    }
    // --- Rollover -----------------------------------------------------------------
    [Fact]
    public void RolloverTo_CarriesForwardFixedAndSubscriptionOnly()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        budget.AddExpense("Rent", ExpenseCategory.FIXED, 1, false, 1200m);
        budget.AddExpense("Netflix", ExpenseCategory.SUBSCRIPTION, null, true, 15m);
        budget.AddExpense("Groceries", ExpenseCategory.VARIABLE, 10, false, 300m);
        var newBudget = budget.RolloverTo("2026-04");
        Assert.Equal(2, newBudget.Expenses.Count);
        Assert.All(newBudget.Expenses, e => Assert.NotEqual(ExpenseCategory.VARIABLE, e.Category));
    }
    [Fact]
    public void RolloverTo_PreservesExclusionFlags()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        var expense = budget.AddExpense("Rent", ExpenseCategory.FIXED, 1, false, 1200m);
        budget.ToggleExpenseExclusion(expense.ExpenseId, true);
        var newBudget = budget.RolloverTo("2026-04");
        Assert.True(newBudget.Expenses[0].IsExcluded);
    }
    [Fact]
    public void RolloverTo_NewBudgetIsInDraftStatus()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        budget.AddIncome("Salary", 5000m);
        var newBudget = budget.RolloverTo("2026-04");
        Assert.Equal(BudgetStatus.DRAFT, newBudget.Status);
    }
    [Fact]
    public void GetTotalIncome_SumsAllIncomeSources()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        budget.AddIncome("Salary", 5000m);
        budget.AddIncome("Bonus", 500m);
        Assert.Equal(5500m, budget.GetTotalIncome());
    }
    // --- Helpers -----------------------------------------------------------------
    private static Budget CreateActiveBudget()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        budget.AddIncome("Salary", 5000m);
        budget.Activate();
        budget.ClearDomainEvents();
        return budget;
    }
}
