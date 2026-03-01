using MonthlyBudget.BudgetManagement.Domain.Events;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.SharedKernel.Events;

namespace MonthlyBudget.BudgetManagement.Domain.Entities;

public enum BudgetStatus
{
    DRAFT,
    ACTIVE,
    CLOSED
}

/// <summary>
/// Aggregate Root for the Budget Management bounded context.
/// Enforces invariants INV-B1 through INV-B8.
/// </summary>
public class MonthlyBudget
{
    private readonly List<IncomeSource> _incomeSources = new();
    private readonly List<Expense> _expenses = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid BudgetId { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string YearMonth { get; private set; }
    public BudgetStatus Status { get; private set; }
    public IReadOnlyList<IncomeSource> IncomeSources => _incomeSources.AsReadOnly();
    public IReadOnlyList<Expense> Expenses => _expenses.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private MonthlyBudget() { YearMonth = string.Empty; }

    private MonthlyBudget(Guid budgetId, Guid householdId, string yearMonth)
    {
        BudgetId = budgetId;
        HouseholdId = householdId;
        YearMonth = yearMonth;
        Status = BudgetStatus.DRAFT;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BudgetCreated(budgetId, householdId, yearMonth));
    }

    public static MonthlyBudget Create(Guid householdId, string yearMonth)
    {
        if (string.IsNullOrWhiteSpace(yearMonth) || !System.Text.RegularExpressions.Regex.IsMatch(yearMonth, @"^\d{4}-\d{2}$"))
            throw new ArgumentException("YearMonth must be in format YYYY-MM.", nameof(yearMonth));

        return new MonthlyBudget(Guid.NewGuid(), householdId, yearMonth);
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    // ─── INV-B1: Activate ───────────────────────────────────────────────────────
    /// <summary>INV-B1: Activates the budget. Throws if no income sources exist.</summary>
    public void Activate()
    {
        // INV-B6: DRAFT → ACTIVE only
        if (Status != BudgetStatus.DRAFT)
            throw new InvalidBudgetStatusTransitionException(Status.ToString(), BudgetStatus.ACTIVE.ToString());

        // INV-B1: At least one income source required
        if (_incomeSources.Count == 0)
            throw new InsufficientIncomeException();

        Status = BudgetStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new BudgetActivated(BudgetId, HouseholdId, YearMonth,
            _incomeSources.Sum(i => i.Amount), _expenses.Count));
    }

    public void Close()
    {
        // INV-B6: ACTIVE → CLOSED only
        if (Status != BudgetStatus.ACTIVE)
            throw new InvalidBudgetStatusTransitionException(Status.ToString(), BudgetStatus.CLOSED.ToString());

        Status = BudgetStatus.CLOSED;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new BudgetClosed(BudgetId, HouseholdId, YearMonth));
    }

    // ─── Income Sources ──────────────────────────────────────────────────────────
    public IncomeSource AddIncome(string name, decimal amount)
    {
        // INV-B8: DRAFT allows income setup, ACTIVE allows modifications
        EnsureModifiable();

        var income = IncomeSource.Create(name, amount);
        _incomeSources.Add(income);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new IncomeSourceAdded(BudgetId, income.IncomeId, income.Name, income.Amount));
        return income;
    }

    public void UpdateIncome(Guid incomeId, string name, decimal amount)
    {
        EnsureModifiable();

        var income = _incomeSources.FirstOrDefault(i => i.IncomeId == incomeId)
            ?? throw new IncomeSourceNotFoundException(incomeId);

        income.Update(name, amount);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new IncomeSourceUpdated(BudgetId, income.IncomeId, income.Name, income.Amount));
    }

    public void RemoveIncome(Guid incomeId)
    {
        EnsureModifiable();

        var income = _incomeSources.FirstOrDefault(i => i.IncomeId == incomeId)
            ?? throw new IncomeSourceNotFoundException(incomeId);

        _incomeSources.Remove(income);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new IncomeSourceRemoved(BudgetId, incomeId));
    }

    // ─── Expenses ────────────────────────────────────────────────────────────────
    public Expense AddExpense(string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount)
    {
        EnsureModifiable();

        var expense = Expense.Create(name, category, dayOfMonth, isSpread, amount, GetLastDayOfMonth());
        _expenses.Add(expense);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ExpenseAdded(BudgetId, expense.ExpenseId, expense.Name,
            expense.Category, expense.DayOfMonth, expense.IsSpread, expense.Amount));
        return expense;
    }

    public void UpdateExpense(Guid expenseId, string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount)
    {
        EnsureModifiable();

        var expense = _expenses.FirstOrDefault(e => e.ExpenseId == expenseId)
            ?? throw new ExpenseNotFoundException(expenseId);

        expense.Update(name, category, dayOfMonth, isSpread, amount, GetLastDayOfMonth());
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ExpenseUpdated(BudgetId, expenseId));
    }

    public void RemoveExpense(Guid expenseId)
    {
        EnsureModifiable();

        var expense = _expenses.FirstOrDefault(e => e.ExpenseId == expenseId)
            ?? throw new ExpenseNotFoundException(expenseId);

        _expenses.Remove(expense);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ExpenseRemoved(BudgetId, expenseId));
    }

    public void ToggleExpenseExclusion(Guid expenseId, bool isExcluded)
    {
        EnsureModifiable();

        var expense = _expenses.FirstOrDefault(e => e.ExpenseId == expenseId)
            ?? throw new ExpenseNotFoundException(expenseId);

        expense.ToggleExclusion(isExcluded);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ExpenseExclusionToggled(BudgetId, expenseId, isExcluded));
    }

    // ─── Rollover ────────────────────────────────────────────────────────────────
    /// <summary>
    /// Creates a new MonthlyBudget for the target month, carrying forward only
    /// FIXED and SUBSCRIPTION expenses (not VARIABLE). Preserves exclusion flags.
    /// The new budget is created in DRAFT status.
    /// </summary>
    public MonthlyBudget RolloverTo(string targetYearMonth)
    {
        var newBudget = new MonthlyBudget(Guid.NewGuid(), HouseholdId, targetYearMonth);

        // Copy income sources
        foreach (var income in _incomeSources)
            newBudget._incomeSources.Add(IncomeSource.Create(income.Name, income.Amount));

        // Carry forward only FIXED and SUBSCRIPTION expenses, preserve exclusion flags
        foreach (var expense in _expenses.Where(e => e.Category != ExpenseCategory.VARIABLE))
        {
            var newExpense = Expense.Create(
                expense.Name, expense.Category,
                expense.DayOfMonth, expense.IsSpread, expense.Amount,
                newBudget.GetLastDayOfMonth());

            if (expense.IsExcluded)
                newExpense.ToggleExclusion(true);

            newBudget._expenses.Add(newExpense);
        }

        newBudget.UpdatedAt = DateTime.UtcNow;
        newBudget.RaiseDomainEvent(new BudgetRolledOver(BudgetId, newBudget.BudgetId, targetYearMonth));
        return newBudget;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────
    public decimal GetTotalIncome() => _incomeSources.Sum(i => i.Amount);

    private void EnsureModifiable()
    {
        // INV-B8: Only DRAFT (for setup) and ACTIVE budgets can be modified
        if (Status == BudgetStatus.CLOSED)
            throw new BudgetNotModifiableException(Status.ToString());
    }

    private int GetLastDayOfMonth()
    {
        // YearMonth format: YYYY-MM
        var parts = YearMonth.Split('-');
        int year = int.Parse(parts[0]);
        int month = int.Parse(parts[1]);
        return DateTime.DaysInMonth(year, month);
    }

    private void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}

