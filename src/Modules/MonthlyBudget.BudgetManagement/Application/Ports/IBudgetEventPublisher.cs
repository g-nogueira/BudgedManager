using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.BudgetManagement.Application.Ports;
public interface IBudgetEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
