using MediatR;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
using BudgetEntity = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;
namespace MonthlyBudget.BudgetManagement.Application.Features.CreateBudget;
public sealed class CreateBudgetHandler : IRequestHandler<CreateBudgetCommand, CreateBudgetResult>
{
    private readonly IBudgetRepository _repository;
    private readonly IBudgetEventPublisher _publisher;
    public CreateBudgetHandler(IBudgetRepository repository, IBudgetEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }
    public async Task<CreateBudgetResult> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        // INV-B7: Only one budget per household per month
        if (await _repository.ExistsForHouseholdAndMonthAsync(request.HouseholdId, request.YearMonth, cancellationToken))
            throw new BudgetAlreadyExistsException(request.YearMonth);
        var budget = BudgetEntity.Create(request.HouseholdId, request.YearMonth);
        await _repository.SaveAsync(budget, cancellationToken);
        foreach (var evt in budget.GetDomainEvents())
            await _publisher.PublishAsync(evt, cancellationToken);
        budget.ClearDomainEvents();
        return new CreateBudgetResult(budget.BudgetId, budget.Status.ToString());
    }
}
