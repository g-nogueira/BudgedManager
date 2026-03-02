using Microsoft.EntityFrameworkCore;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
using MonthlyBudget.Infrastructure.Database;
using BudgetEntity = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;
namespace MonthlyBudget.Infrastructure.Repositories;
public sealed class PostgresBudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _db;
    public PostgresBudgetRepository(AppDbContext db) { _db = db; }
    public async Task<BudgetEntity?> FindByIdAsync(Guid budgetId, CancellationToken ct = default)
        => await _db.Budgets
            .Include(b => b.IncomeSources)
            .Include(b => b.Expenses)
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId, ct);
    public async Task<BudgetEntity?> FindByHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default)
        => await _db.Budgets
            .Include(b => b.IncomeSources)
            .Include(b => b.Expenses)
            .FirstOrDefaultAsync(b => b.HouseholdId == householdId && b.YearMonth == yearMonth, ct);
    public async Task<IReadOnlyList<BudgetEntity>> FindAllByHouseholdAsync(Guid householdId, CancellationToken ct = default)
        => await _db.Budgets
            .Include(b => b.IncomeSources)
            .Include(b => b.Expenses)
            .Where(b => b.HouseholdId == householdId)
            .OrderByDescending(b => b.YearMonth)
            .ToListAsync(ct);
    public async Task SaveAsync(BudgetEntity budget, CancellationToken ct = default)
    {
        var existing = await _db.Budgets.FindAsync(new object[] { budget.BudgetId }, ct);
        if (existing == null)
            _db.Budgets.Add(budget);
        else
            _db.Entry(existing).CurrentValues.SetValues(budget);
        await _db.SaveChangesAsync(ct);
    }
    public async Task<bool> ExistsForHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default)
        => await _db.Budgets.AnyAsync(b => b.HouseholdId == householdId && b.YearMonth == yearMonth, ct);
}
