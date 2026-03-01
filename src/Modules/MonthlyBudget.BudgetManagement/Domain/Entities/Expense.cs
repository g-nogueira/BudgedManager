namespace MonthlyBudget.BudgetManagement.Domain.Entities;

public enum ExpenseCategory
{
    FIXED,
    SUBSCRIPTION,
    VARIABLE
}

public class Expense
{
    public Guid ExpenseId { get; private set; }
    public string Name { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public int? DayOfMonth { get; private set; }
    public bool IsSpread { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsExcluded { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private Expense() { Name = string.Empty; }

    private Expense(Guid expenseId, string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount)
    {
        ExpenseId = expenseId;
        Name = name.Trim();
        Category = category;
        DayOfMonth = dayOfMonth;
        IsSpread = isSpread;
        Amount = amount;
        IsExcluded = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Expense Create(string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount, int lastDayOfMonth)
    {
        ValidateExpense(name, dayOfMonth, isSpread, amount, lastDayOfMonth);
        return new Expense(Guid.NewGuid(), name, category, dayOfMonth, isSpread, amount);
    }

    public void Update(string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount, int lastDayOfMonth)
    {
        ValidateExpense(name, dayOfMonth, isSpread, amount, lastDayOfMonth);
        Name = name.Trim();
        Category = category;
        DayOfMonth = dayOfMonth;
        IsSpread = isSpread;
        Amount = amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleExclusion(bool isExcluded)
    {
        IsExcluded = isExcluded;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateExpense(string name, int? dayOfMonth, bool isSpread, decimal amount, int lastDayOfMonth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Expense name cannot be empty.", nameof(name));
        if (name.Trim().Length > 100)
            throw new ArgumentException("Expense name cannot exceed 100 characters.", nameof(name));
        if (amount <= 0)
            throw new ArgumentException("Expense amount must be greater than zero.", nameof(amount));

        // INV-B3: Spread expenses must not have a specific day
        if (isSpread && dayOfMonth.HasValue)
            throw new Exceptions.InvalidExpenseDayException(
                "Spread expenses must not have a specific day assigned (dayOfMonth must be null when isSpread=true).");

        // INV-B4: Specific-day expenses must have a day assigned
        if (!isSpread && !dayOfMonth.HasValue)
            throw new Exceptions.InvalidExpenseDayException(
                "Non-spread expenses must have a specific day assigned (dayOfMonth must be set when isSpread=false).");

        // INV-B2: Expense day must be valid for the budget month
        if (dayOfMonth.HasValue && (dayOfMonth.Value < 1 || dayOfMonth.Value > lastDayOfMonth))
            throw new Exceptions.InvalidExpenseDayException(
                $"Day of month {dayOfMonth.Value} is invalid for a month with {lastDayOfMonth} days.");
    }
}

