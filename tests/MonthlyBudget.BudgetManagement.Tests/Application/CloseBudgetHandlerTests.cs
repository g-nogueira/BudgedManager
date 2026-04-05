using MonthlyBudget.BudgetManagement.Application.Features.CloseBudget;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Events;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
using MonthlyBudget.SharedKernel.Events;
using Budget = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;

namespace MonthlyBudget.BudgetManagement.Tests.Application;

public sealed class CloseBudgetHandlerTests
{
    private static readonly Guid HouseholdId = Guid.NewGuid();
    private const string YearMonth = "2026-03";

    [Fact]
    public async Task Handle_ActiveBudget_ReturnsClosedResult()
    {
        var budget = CreateActiveBudget();
        var repo = new FakeBudgetRepository(budget);
        var publisher = new FakeBudgetEventPublisher();
        var sut = new CloseBudgetHandler(repo, publisher);

        var result = await sut.Handle(new CloseBudgetCommand(budget.BudgetId, HouseholdId), CancellationToken.None);

        Assert.Equal(budget.BudgetId, result.BudgetId);
        Assert.Equal("CLOSED", result.Status);
    }

    [Fact]
    public async Task Handle_ActiveBudget_PublishesBudgetClosedEvent()
    {
        var budget = CreateActiveBudget();
        var repo = new FakeBudgetRepository(budget);
        var publisher = new FakeBudgetEventPublisher();
        var sut = new CloseBudgetHandler(repo, publisher);

        await sut.Handle(new CloseBudgetCommand(budget.BudgetId, HouseholdId), CancellationToken.None);

        Assert.Single(publisher.PublishedEvents.OfType<BudgetClosed>());
    }

    [Fact]
    public async Task Handle_BudgetNotFound_ThrowsBudgetNotFoundException()
    {
        var repo = new FakeBudgetRepository();
        var publisher = new FakeBudgetEventPublisher();
        var sut = new CloseBudgetHandler(repo, publisher);

        await Assert.ThrowsAsync<BudgetNotFoundException>(() =>
            sut.Handle(new CloseBudgetCommand(Guid.NewGuid(), HouseholdId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongHouseholdId_ThrowsBudgetNotFoundException()
    {
        var budget = CreateActiveBudget();
        var repo = new FakeBudgetRepository(budget);
        var publisher = new FakeBudgetEventPublisher();
        var sut = new CloseBudgetHandler(repo, publisher);
        var wrongHouseholdId = Guid.NewGuid();

        await Assert.ThrowsAsync<BudgetNotFoundException>(() =>
            sut.Handle(new CloseBudgetCommand(budget.BudgetId, wrongHouseholdId), CancellationToken.None));
    }

    private static Budget CreateActiveBudget()
    {
        var budget = Budget.Create(HouseholdId, YearMonth);
        budget.AddIncome("Salary", 5000m);
        budget.Activate();
        budget.ClearDomainEvents();
        return budget;
    }

    private sealed class FakeBudgetRepository : IBudgetRepository
    {
        private readonly Budget? _budget;

        public FakeBudgetRepository(Budget? budget = null)
        {
            _budget = budget;
        }

        public Task<Budget?> FindByIdAsync(Guid budgetId, CancellationToken ct = default)
            => Task.FromResult(_budget?.BudgetId == budgetId ? _budget : null);

        public Task<Budget?> FindByHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default)
            => Task.FromResult<Budget?>(null);

        public Task<IReadOnlyList<Budget>> FindAllByHouseholdAsync(Guid householdId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Budget>>(Array.Empty<Budget>());

        public Task SaveAsync(Budget budget, CancellationToken ct = default) => Task.CompletedTask;

        public Task<bool> ExistsForHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default)
            => Task.FromResult(false);
    }

    private sealed class FakeBudgetEventPublisher : IBudgetEventPublisher
    {
        public List<IDomainEvent> PublishedEvents { get; } = new();

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
        {
            PublishedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }
}
