# Codebase Patterns — Budget Management Context

> Real code patterns extracted from the existing codebase. Copy these conventions exactly.

## Namespace Root
`MonthlyBudget.BudgetManagement`

## Entity Alias Convention
The aggregate root shares the project name. Use this alias throughout:
```csharp
using BudgetEntity = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;
```
In tests:
```csharp
using Budget = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;
```

## Aggregate Root — MonthlyBudget

**File:** `src/Modules/MonthlyBudget.BudgetManagement/Domain/Entities/MonthlyBudget.cs`

Public API surface:
```csharp
public static MonthlyBudget Create(Guid householdId, string yearMonth)
public void Activate()
public void Close()
public IncomeSource AddIncome(string name, decimal amount)
public void UpdateIncome(Guid incomeId, string name, decimal amount)
public void RemoveIncome(Guid incomeId)
public Expense AddExpense(string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount)
public void UpdateExpense(Guid expenseId, string name, ExpenseCategory category, int? dayOfMonth, bool isSpread, decimal amount)
public void RemoveExpense(Guid expenseId)
public void ToggleExpenseExclusion(Guid expenseId, bool isExcluded)
public MonthlyBudget RolloverTo(string targetYearMonth)
public decimal GetTotalIncome()
public IReadOnlyList<IDomainEvent> GetDomainEvents()
public void ClearDomainEvents()
```

Properties: `BudgetId`, `HouseholdId`, `YearMonth`, `Status` (enum `BudgetStatus`), `IncomeSources` (readonly), `Expenses` (readonly), `CreatedAt`, `UpdatedAt`.

## Command Example

**File:** `Application/Features/CreateBudget/CreateBudgetCommand.cs`
```csharp
public sealed record CreateBudgetCommand(Guid HouseholdId, string YearMonth) : IRequest<CreateBudgetResult>;
public sealed record CreateBudgetResult(Guid BudgetId, string Status);
```

## Handler Example

**File:** `Application/Features/CreateBudget/CreateBudgetHandler.cs`
```csharp
public sealed class CreateBudgetHandler : IRequestHandler<CreateBudgetCommand, CreateBudgetResult>
{
    private readonly IBudgetRepository _repository;
    private readonly IBudgetEventPublisher _publisher;
    // ... constructor injection
    public async Task<CreateBudgetResult> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
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
```

## Controller — BudgetController

**File:** `Infrastructure/Controllers/BudgetController.cs`
- Route: `[Route("api/v1/budgets")]`
- Household extraction: `private Guid HouseholdId => Guid.Parse(User.FindFirstValue("householdId")!);`
- Creates return `CreatedAtAction(nameof(GetById), new { budgetId = result.BudgetId }, result);`

## Repository Interface

**File:** `Domain/Repositories/IBudgetRepository.cs`
```csharp
public interface IBudgetRepository
{
    Task<BudgetEntity?> FindByIdAsync(Guid budgetId, CancellationToken ct = default);
    Task<BudgetEntity?> FindByHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default);
    Task<IReadOnlyList<BudgetEntity>> FindAllByHouseholdAsync(Guid householdId, CancellationToken ct = default);
    Task SaveAsync(BudgetEntity budget, CancellationToken ct = default);
    Task<bool> ExistsForHouseholdAndMonthAsync(Guid householdId, string yearMonth, CancellationToken ct = default);
}
```

## Event Publisher Port

**File:** `Application/Ports/IBudgetEventPublisher.cs`
```csharp
public interface IBudgetEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
```

## Enums

```csharp
// Domain/Entities/MonthlyBudget.cs
public enum BudgetStatus { DRAFT, ACTIVE, CLOSED }

// Domain/Entities/Expense.cs
public enum ExpenseCategory { FIXED, SUBSCRIPTION, VARIABLE }
```

## EF Config — Schema: `budget`

**File:** `src/MonthlyBudget.Infrastructure/Database/Configurations/MonthlyBudgetConfiguration.cs`
- Table: `monthly_budgets` in schema `budget`
- Unique index: `(HouseholdId, YearMonth)`
- Children via `HasMany(...).WithOne().HasForeignKey("BudgetId").OnDelete(DeleteBehavior.Cascade)`
- Navigation: `UsePropertyAccessMode(PropertyAccessMode.Field)`

## DI registrations (in ServiceCollectionExtensions.cs)
```csharp
services.AddScoped<IBudgetRepository, PostgresBudgetRepository>();
services.AddScoped<IBudgetEventPublisher, MediatRBudgetEventPublisher>();
```

## Existing Feature Folders
```
Application/Features/
  CreateBudget/
  AddIncome/
  UpdateIncome/
  RemoveIncome/
  AddExpense/
  UpdateExpense/
  RemoveExpense/
  ToggleExpenseExclusion/
  ActivateBudget/
  RolloverMonth/
  GetBudget/ (GetBudgetByIdQuery, GetBudgetByMonthQuery)
```
