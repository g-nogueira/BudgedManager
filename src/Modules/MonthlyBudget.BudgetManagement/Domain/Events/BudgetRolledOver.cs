using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Domain.Events;
public sealed class BudgetRolledOver : DomainEventBase
{
    public Guid SourceBudgetId { get; }
    public Guid TargetBudgetId { get; }
    public string TargetYearMonth { get; }
    public BudgetRolledOver(Guid sourceBudgetId, Guid targetBudgetId, string targetYearMonth)
    {
        SourceBudgetId = sourceBudgetId;
        TargetBudgetId = targetBudgetId;
        TargetYearMonth = targetYearMonth;
    }
}
