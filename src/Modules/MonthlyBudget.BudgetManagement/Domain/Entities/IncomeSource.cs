namespace MonthlyBudget.BudgetManagement.Domain.Entities;

public class IncomeSource
{
    public Guid IncomeId { get; private set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private IncomeSource() { Name = string.Empty; }

    private IncomeSource(Guid incomeId, string name, decimal amount)
    {
        IncomeId = incomeId;
        Name = name.Trim();
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static IncomeSource Create(string name, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Income source name cannot be empty.", nameof(name));
        if (name.Trim().Length > 100)
            throw new ArgumentException("Income source name cannot exceed 100 characters.", nameof(name));
        if (amount <= 0)
            throw new ArgumentException("Income amount must be greater than zero.", nameof(amount));

        return new IncomeSource(Guid.NewGuid(), name, amount);
    }

    public void Update(string name, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Income source name cannot be empty.", nameof(name));
        if (name.Trim().Length > 100)
            throw new ArgumentException("Income source name cannot exceed 100 characters.", nameof(name));
        if (amount <= 0)
            throw new ArgumentException("Income amount must be greater than zero.", nameof(amount));

        Name = name.Trim();
        Amount = amount;
        UpdatedAt = DateTime.UtcNow;
    }
}

