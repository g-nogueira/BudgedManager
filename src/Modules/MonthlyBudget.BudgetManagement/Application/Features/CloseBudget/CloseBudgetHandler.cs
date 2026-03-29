using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;

namespace MonthlyBudget.BudgetManagement.Application.Features.CloseBudget;

public sealed class CloseBudgetHandler : IRequestHandler<CloseBudgetCommand, CloseBudgetResult>
{
    private readonly IBudgetRepository _repository;
    private readonly IBudgetEventPublisher _publisher;

    public CloseBudgetHandler(IBudgetRepository repository, IBudgetEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<CloseBudgetResult> Handle(CloseBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _repository.FindByIdAsync(request.BudgetId, cancellationToken);
        if (budget == null || budget.HouseholdId != request.HouseholdId)
            throw new BudgetNotFoundException(request.BudgetId);

        budget.Close();

        await _repository.SaveAsync(budget, cancellationToken);
        foreach (var evt in budget.GetDomainEvents())
            await _publisher.PublishAsync(evt, cancellationToken);
        budget.ClearDomainEvents();

        return new CloseBudgetResult(budget.BudgetId, budget.Status.ToString());
    }
}