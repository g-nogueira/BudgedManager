using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
namespace MonthlyBudget.BudgetManagement.Application.Features.ActivateBudget;
public sealed class ActivateBudgetHandler : IRequestHandler<ActivateBudgetCommand, ActivateBudgetResult>
{
    private readonly IBudgetRepository _repository;
    private readonly IBudgetEventPublisher _publisher;
    public ActivateBudgetHandler(IBudgetRepository repository, IBudgetEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }
    public async Task<ActivateBudgetResult> Handle(ActivateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _repository.FindByIdAsync(request.BudgetId, cancellationToken);
        if (budget == null || budget.HouseholdId != request.HouseholdId)
            throw new BudgetNotFoundException(request.BudgetId);

        budget.Activate(); // enforces INV-B1 and INV-B6 automatically

        await _repository.SaveAsync(budget, cancellationToken);
        foreach (var evt in budget.GetDomainEvents())
            await _publisher.PublishAsync(evt, cancellationToken);
        budget.ClearDomainEvents();

        return new ActivateBudgetResult(budget.BudgetId, budget.Status.ToString());
    }
}

