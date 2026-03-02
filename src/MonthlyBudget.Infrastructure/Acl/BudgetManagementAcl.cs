using Microsoft.EntityFrameworkCore;
using MonthlyBudget.BudgetManagement.Domain.Entities;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.Infrastructure.Database;
namespace MonthlyBudget.Infrastructure.Acl;
public sealed class BudgetManagementAcl : IBudgetDataPort
{
    private readonly AppDbContext _db;
    public BudgetManagementAcl(AppDbContext db) { _db = db; }
    public async Task<BudgetData?> GetBudgetDataAsync(Guid budgetId, Guid householdId, CancellationToken ct = default)
    {
        var budget = await _db.Budgets.Include(b => b.IncomeSources).Include(b => b.Expenses)
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId && b.HouseholdId == householdId, ct);
        if (budget == null) return null;
        var expenses = budget.Expenses.Select(e => new ExpenseSnapshotData(
            e.ExpenseId, e.Name, TranslateCategory(e.Category),
            e.DayOfMonth, e.IsSpread, e.Amount, e.IsExcluded)).ToList();
        return new BudgetData(budget.BudgetId, budget.HouseholdId, budget.YearMonth, budget.GetTotalIncome(), expenses);
    }
    private static SnapshotCategory TranslateCategory(ExpenseCategory c) => c switch
    {
        ExpenseCategory.FIXED        => SnapshotCategory.FIXED,
        ExpenseCategory.SUBSCRIPTION => SnapshotCategory.SUBSCRIPTION,
        _                            => SnapshotCategory.VARIABLE
    };
}
