using MediatR;
using MonthlyBudget.BudgetManagement.Domain.Events;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Events;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
namespace MonthlyBudget.Infrastructure.EventHandlers;

/// <summary>
/// Consumes Budget Management domain events and marks affected forecasts as stale.
/// Lives in the Infrastructure layer which references both bounded contexts.
/// Does NOT auto-regenerate — only publishes ForecastStaleMarked (PropagateExpenseChanges policy).
/// </summary>
public sealed class BudgetEventHandler :
    INotificationHandler<ExpenseAdded>,
    INotificationHandler<ExpenseUpdated>,
    INotificationHandler<ExpenseRemoved>,
    INotificationHandler<ExpenseExclusionToggled>
{
    private readonly IForecastRepository _repo;
    private readonly IForecastEventPublisher _publisher;
    public BudgetEventHandler(IForecastRepository repo, IForecastEventPublisher publisher)
    { _repo = repo; _publisher = publisher; }

    public Task Handle(ExpenseAdded n, CancellationToken ct) => MarkStale(n.BudgetId, "ExpenseAdded", ct);
    public Task Handle(ExpenseUpdated n, CancellationToken ct) => MarkStale(n.BudgetId, "ExpenseUpdated", ct);
    public Task Handle(ExpenseRemoved n, CancellationToken ct) => MarkStale(n.BudgetId, "ExpenseRemoved", ct);
    public Task Handle(ExpenseExclusionToggled n, CancellationToken ct) => MarkStale(n.BudgetId, "ExpenseExclusionToggled", ct);

    private async Task MarkStale(Guid budgetId, string reason, CancellationToken ct)
    {
        var forecasts = await _repo.FindAllByBudgetAsync(budgetId, ct);
        foreach (var forecast in forecasts.Where(f => !f.IsSnapshot))
            await _publisher.PublishAsync(new ForecastStaleMarked(forecast.ForecastId, reason), ct);
    }
}

