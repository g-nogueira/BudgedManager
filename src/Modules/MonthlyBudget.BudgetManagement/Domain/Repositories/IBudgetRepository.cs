using BudgetEntity = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;
namespace MonthlyBudget.BudgetManagement.Domain.Repositories;
public interface IBudgetRepository
{
    Task<BudgetEntity?> FindByIdAsync(Guid budgetId, CancellationToken ct = default);
    Task<BudgetEntity?> FindByHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default);
    Task<IReadOnlyList<BudgetEntity>> FindAllByHouseholdAsync(Guid householdId, CancellationToken ct = default);
    Task SaveAsync(BudgetEntity budget, CancellationToken ct = default);
    Task<bool> ExistsForHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default);
}
