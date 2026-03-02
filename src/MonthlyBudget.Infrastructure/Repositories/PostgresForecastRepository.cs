using Microsoft.EntityFrameworkCore;
using MonthlyBudget.ForecastEngine.Domain.Entities;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
using MonthlyBudget.Infrastructure.Database;
namespace MonthlyBudget.Infrastructure.Repositories;
public sealed class PostgresForecastRepository : IForecastRepository
{
    private readonly AppDbContext _db;
    public PostgresForecastRepository(AppDbContext db) { _db = db; }
    public async Task<ForecastVersion?> FindByIdAsync(Guid forecastId, CancellationToken ct = default)
        => await _db.ForecastVersions
            .Include(f => f.DailyEntries).ThenInclude(e => e.ExpenseBreakdown)
            .Include(f => f.ExpenseSnapshots)
            .FirstOrDefaultAsync(f => f.ForecastId == forecastId, ct);
    public async Task<IReadOnlyList<ForecastVersion>> FindAllByBudgetAsync(Guid budgetId, CancellationToken ct = default)
        => await _db.ForecastVersions
            .Include(f => f.ExpenseSnapshots)
            .Where(f => f.BudgetId == budgetId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    public async Task<ForecastVersion?> FindLatestByBudgetAsync(Guid budgetId, CancellationToken ct = default)
        => await _db.ForecastVersions
            .Include(f => f.DailyEntries).ThenInclude(e => e.ExpenseBreakdown)
            .Include(f => f.ExpenseSnapshots)
            .Where(f => f.BudgetId == budgetId)
            .OrderByDescending(f => f.CreatedAt)
            .FirstOrDefaultAsync(ct);
    public async Task SaveAsync(ForecastVersion forecast, CancellationToken ct = default)
    {
        _db.ForecastVersions.Add(forecast);
        await _db.SaveChangesAsync(ct);
    }
}
