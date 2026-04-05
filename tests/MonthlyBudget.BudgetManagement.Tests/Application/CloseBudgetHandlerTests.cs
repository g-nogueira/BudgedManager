using MonthlyBudget.BudgetManagement.Application.Features.CloseBudget;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
using MonthlyBudget.SharedKernel.Events;
using Budget = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;

namespace MonthlyBudget.BudgetManagement.Tests.Application;

public sealed class CloseBudgetHandlerTests
{
    private static readonly Guid HouseholdId = Guid.NewGuid();

    [Fact]
    public async Task Handle_BudgetNotFound_ThrowsBudgetNotFoundException()
    {
        var sut = BuildSut(repository: new FakeBudgetRepository());

        await Assert.ThrowsAsync<BudgetNotFoundException>(() =>
            sut.Handle(new CloseBudgetCommand(Guid.NewGuid(), HouseholdId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_HouseholdIdMismatch_ThrowsBudgetNotFoundException()
    {
        var budget = CreateActiveBudget();
        var sut = BuildSut(repository: new FakeBudgetRepository(budget));

        await Assert.ThrowsAsync<BudgetNotFoundException>(() =>
            sut.Handle(new CloseBudgetCommand(budget.BudgetId, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ActiveBudget_ReturnsResultWithClosedStatus()
    {
        var budget = CreateActiveBudget();
        var sut = BuildSut(repository: new FakeBudgetRepository(budget));

        var result = await sut.Handle(new CloseBudgetCommand(budget.BudgetId, HouseholdId), CancellationToken.None);

        Assert.Equal(budget.BudgetId, result.BudgetId);
        Assert.Equal("CLOSED", result.Status);
    }

    [Fact]
    public async Task Handle_ActiveBudget_SavesBudgetToRepository()
    {
        var budget = CreateActiveBudget();
        var repo = new FakeBudgetRepository(budget);
        var sut = BuildSut(repository: repo);

        await sut.Handle(new CloseBudgetCommand(budget.BudgetId, HouseholdId), CancellationToken.None);

        Assert.True(repo.WasSaved);
    }

    [Fact]
    public async Task Handle_ActiveBudget_PublishesDomainEvents()
    {
        var budget = CreateActiveBudget();
        var publisher = new FakeBudgetEventPublisher();
        var sut = BuildSut(repository: new FakeBudgetRepository(budget), publisher: publisher);

        await sut.Handle(new CloseBudgetCommand(budget.BudgetId, HouseholdId), CancellationToken.None);

        Assert.NotEmpty(publisher.PublishedEvents);
    }

    [Fact]
    public async Task Handle_ActiveBudget_ClearsDomainEventsAfterPublishing()
    {
        var budget = CreateActiveBudget();
        var sut = BuildSut(repository: new FakeBudgetRepository(budget));

        await sut.Handle(new CloseBudgetCommand(budget.BudgetId, HouseholdId), CancellationToken.None);

        Assert.Empty(budget.GetDomainEvents());
    }

    [Fact]
    public async Task Handle_DraftBudget_ThrowsInvalidBudgetStatusTransitionException()
    {
        var budget = Budget.Create(HouseholdId, "2026-05");
        var sut = BuildSut(repository: new FakeBudgetRepository(budget));

        await Assert.ThrowsAsync<InvalidBudgetStatusTransitionException>(() =>
            sut.Handle(new CloseBudgetCommand(budget.BudgetId, HouseholdId), CancellationToken.None));
    }

    private static CloseBudgetHandler BuildSut(
        FakeBudgetRepository? repository = null,
        FakeBudgetEventPublisher? publisher = null)
        => new(repository ?? new FakeBudgetRepository(), publisher ?? new FakeBudgetEventPublisher());

    private static Budget CreateActiveBudget()
    {
        var budget = Budget.Create(HouseholdId, "2026-05");
        budget.AddIncome("Salary", 5000m);
        budget.Activate();
        budget.ClearDomainEvents();
        return budget;
    }

    private sealed class FakeBudgetRepository : IBudgetRepository
    {
        private readonly Budget? _budget;
        public bool WasSaved { get; private set; }

        public FakeBudgetRepository(Budget? budget = null) => _budget = budget;

        public Task<Budget?> FindByIdAsync(Guid budgetId, CancellationToken ct = default)
            => Task.FromResult(_budget?.BudgetId == budgetId ? _budget : null);

        public Task SaveAsync(Budget budget, CancellationToken ct = default)
        {
            WasSaved = true;
            return Task.CompletedTask;
        }

        public Task<Budget?> FindByHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default)
            => Task.FromResult<Budget?>(null);

        public Task<IReadOnlyList<Budget>> FindAllByHouseholdAsync(Guid householdId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Budget>>(Array.Empty<Budget>());

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