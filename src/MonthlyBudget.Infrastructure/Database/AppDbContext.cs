using Microsoft.EntityFrameworkCore;
using MonthlyBudget.BudgetManagement.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using BudgetEntity = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;

namespace MonthlyBudget.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Budget Management context
    public DbSet<BudgetEntity> Budgets { get; set; } = null!;
    public DbSet<IncomeSource> IncomeSources { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;

    // Forecast Engine context
    public DbSet<ForecastVersion> ForecastVersions { get; set; } = null!;
    public DbSet<DailyEntry> DailyEntries { get; set; } = null!;
    public DbSet<ExpenseSnapshot> ExpenseSnapshots { get; set; } = null!;
    public DbSet<DailyExpenseItem> DailyExpenseItems { get; set; } = null!;

    // Identity & Household context
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Household> Households { get; set; } = null!;
    public DbSet<Invitation> Invitations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

