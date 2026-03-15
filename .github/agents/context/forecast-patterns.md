# Codebase Patterns — Forecast Engine Context

> Real code patterns extracted from the existing codebase. Copy these conventions exactly.

## Namespace Root
`MonthlyBudget.ForecastEngine`

## Aggregate Root — ForecastVersion

**File:** `src/Modules/MonthlyBudget.ForecastEngine/Domain/Entities/ForecastVersion.cs`

Public API surface (verify against actual file before using):
```csharp
public static ForecastVersion CreateOriginal(Guid budgetId, Guid householdId, string versionLabel, decimal startBalance, List<ExpenseSnapshot> snapshots, List<DailyEntry> dailyEntries)
public ForecastVersion CreateReforecast(string versionLabel, decimal actualBalance, int startDay, List<ExpenseSnapshot> snapshots, List<DailyEntry> dailyEntries)
public void MarkAsSnapshot()
public IReadOnlyList<IDomainEvent> GetDomainEvents()
public void ClearDomainEvents()
```

Properties: `ForecastId`, `BudgetId`, `HouseholdId`, `VersionLabel`, `ForecastDate`, `StartDay`, `StartBalance`, `ActualBalance`, `ForecastType` (enum), `ParentForecastId`, `IsSnapshot`, `DailyEntries` (readonly), `ExpenseSnapshots` (readonly), `CreatedAt`.

## Value Objects

- `DailyEntry` — `Day`, `RemainingBalance`, `DailyExpenseTotal`, `ExpenseBreakdown` (list of `DailyExpenseItem`)
- `DailyExpenseItem` — `ExpenseSnapshotId`, `Name`, `Amount`
- `ExpenseSnapshot` — `SnapshotId`, `OriginalExpenseId`, `Name`, `Category`, `DayOfMonth`, `IsSpread`, `Amount`, `IsExcluded`
- `ComparisonResult` — `ForecastAId`, `ForecastBId`, `TotalDrift`, `DayVariances`, `ExpenseChanges`
- `DayVariance` — day-level balance difference
- `ExpenseChange` — per-expense diff

## Enums

```csharp
public enum ForecastType { ORIGINAL, REFORECAST }
```

## Repository Interface

**File:** `Domain/Repositories/IForecastRepository.cs`
```csharp
public interface IForecastRepository
{
    Task<ForecastVersion?> FindByIdAsync(Guid forecastId, CancellationToken ct = default);
    Task<IReadOnlyList<ForecastVersion>> FindAllByBudgetIdAsync(Guid budgetId, CancellationToken ct = default);
    Task SaveAsync(ForecastVersion forecast, CancellationToken ct = default);
}
```

## ACL Port (reads budget data)

**File:** `Application/Ports/IBudgetDataPort.cs`
```csharp
public interface IBudgetDataPort
{
    Task<BudgetData?> FetchBudgetDataAsync(Guid budgetId, CancellationToken ct = default);
}
```
The `BudgetData` record is defined in the Forecast Engine's Application layer — it's the translated representation of budget data, NOT a shared domain model.

## ACL Adapter (Infrastructure)

**File:** `src/MonthlyBudget.Infrastructure/Acl/BudgetManagementAcl.cs`
- Implements `IBudgetDataPort`
- Reads from `IBudgetRepository` and translates `MonthlyBudget` → `BudgetData`

## Event Handler (consumed from Budget Management)

**File:** `src/MonthlyBudget.Infrastructure/EventHandlers/BudgetEventHandler.cs`
- Handles `ExpenseAdded`, `ExpenseUpdated`, `ExpenseRemoved` notifications
- Marks affected forecasts as stale (`ForecastStaleMarked`)
- Does NOT auto-regenerate forecasts

## Controller — ForecastController

**File:** `Infrastructure/Controllers/ForecastController.cs`
- Route: `[Route("api/v1/forecasts")]`
- HouseholdId from JWT claim

## EF Config — Schema: `forecast`

Tables: `forecast_versions`, `daily_entries`, `daily_expense_items`, `expense_snapshots`

## DI registrations
```csharp
services.AddScoped<IForecastRepository, PostgresForecastRepository>();
services.AddScoped<IForecastEventPublisher, MediatRForecastEventPublisher>();
services.AddScoped<IBudgetDataPort, BudgetManagementAcl>();
```

## Existing Feature Folders
```
Application/Features/
  GenerateForecast/
  SaveSnapshot/
  Reforecast/
  GetForecast/
  CompareForecasts/
```
