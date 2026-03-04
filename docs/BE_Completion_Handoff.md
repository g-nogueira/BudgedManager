# Backend Completion Handoff — MonthlyBudget

**Date:** 2026-03-02  
**Prepared by:** Audit Agent  
**For:** Next Coding Agent  
**Architecture Source:** `docs/MonthlyBudget_Architecture.md`  
**Codebase Guide:** `AGENTS.md`

---

## Context

The previous agent implemented the backend core and declared the MVP done. It is **not done**. This document lists every missing piece in strict priority order with exact file paths, required interfaces, and implementation contracts so you can pick up immediately without re-reading the architecture spec.

**Current test count:** 50 passing (29 BudgetManagement + 9 ForecastEngine + 12 IdentityHousehold).  
**Integration test project** (`tests/MonthlyBudget.Integration.Tests/`) exists but has **zero test files**.  
**Frontend** (`frontend/`) does **not exist** at all.

Run tests with: `dotnet test`  
Build with: `dotnet build`

---

## Priority 1 — Missing Application Features (Backend is broken without these)

### 1.1 `ActivateBudget` Use Case

**Status:** Folder exists (`Application/Features/ActivateBudget/`) but is **completely empty**.

**Files to create:**

#### `src/Modules/MonthlyBudget.BudgetManagement/Application/Features/ActivateBudget/ActivateBudgetCommand.cs`
```csharp
using MediatR;
namespace MonthlyBudget.BudgetManagement.Application.Features.ActivateBudget;
public sealed record ActivateBudgetCommand(Guid BudgetId, Guid HouseholdId) : IRequest<ActivateBudgetResult>;
public sealed record ActivateBudgetResult(Guid BudgetId, string Status);
```

#### `src/Modules/MonthlyBudget.BudgetManagement/Application/Features/ActivateBudget/ActivateBudgetHandler.cs`
Follow the exact same pattern as `CreateBudgetHandler.cs`:
- Inject `IBudgetRepository` and `IBudgetEventPublisher`
- Load budget via `FindByIdAsync`, throw `BudgetNotFoundException` if null or `HouseholdId` doesn't match
- Call `budget.Activate()` — this enforces INV-B1 and INV-B6 automatically
- Save, publish domain events, clear events
- Return `ActivateBudgetResult`

#### API endpoint to add to `BudgetController.cs`
```csharp
[HttpPost("{budgetId:guid}/activate")]
public async Task<IActionResult> Activate(Guid budgetId, CancellationToken ct)
{
    var result = await _mediator.Send(new ActivateBudgetCommand(budgetId, HouseholdId), ct);
    return Ok(result);
}
```

---

### 1.2 `SaveSnapshot` Use Case

**Status:** Folder exists (`Application/Features/SaveSnapshot/`) but is **completely empty**. No endpoint exists.

**Files to create:**

#### `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/SaveSnapshot/SaveSnapshotCommand.cs`
```csharp
using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.SaveSnapshot;
public sealed record SaveSnapshotCommand(Guid ForecastId, Guid HouseholdId) : IRequest<SaveSnapshotResult>;
public sealed record SaveSnapshotResult(Guid ForecastId, bool IsSnapshot);
```

#### `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/SaveSnapshot/SaveSnapshotHandler.cs`
- Inject `IForecastRepository`
- Load forecast via `FindByIdAsync`, throw `ForecastNotFoundException` if null or `HouseholdId` doesn't match
- Call `forecast.MarkAsSnapshot()` — this sets `IsSnapshot = true` (INV-F4 is already enforced: `MarkAsSnapshot()` is idempotent since there is no guard throwing on re-snapshot; just sets the flag)
- Call `_repo.SaveAsync(forecast, ct)` — **Note:** the current `SaveAsync` always does `Add`. You must update `PostgresForecastRepository.SaveAsync` to handle updates (use `Update` or `_db.Entry().State = Modified` for existing entities)
- Return `SaveSnapshotResult`

#### API endpoint to add to `ForecastController.cs`
```csharp
[HttpPost("{forecastId:guid}/snapshot")]
public async Task<IActionResult> Snapshot(Guid budgetId, Guid forecastId, CancellationToken ct)
{
    var result = await _mediator.Send(new SaveSnapshotCommand(forecastId, HouseholdId), ct);
    return Ok(result);
}
```

#### Fix `PostgresForecastRepository.SaveAsync` — currently always calls `_db.ForecastVersions.Add(forecast)`
Change it to upsert pattern (same as `PostgresBudgetRepository.SaveAsync`):
```csharp
public async Task SaveAsync(ForecastVersion forecast, CancellationToken ct = default)
{
    var existing = await _db.ForecastVersions.FindAsync(new object[] { forecast.ForecastId }, ct);
    if (existing == null)
        _db.ForecastVersions.Add(forecast);
    else
        _db.Entry(existing).CurrentValues.SetValues(forecast);
    await _db.SaveChangesAsync(ct);
}
```

---

### 1.3 `CompareForecasts` Use Case

**Status:** Folder exists (`Application/Features/CompareForecasts/`) but is **completely empty**. No endpoint exists.

**Files to create:**

#### `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/CompareForecasts/CompareForecastsQuery.cs`
```csharp
using MediatR;
namespace MonthlyBudget.ForecastEngine.Application.Features.CompareForecasts;

public sealed record CompareForecastsQuery(Guid ForecastAId, Guid ForecastBId, Guid HouseholdId)
    : IRequest<ComparisonResult>;

public sealed record ComparisonResult(
    Guid ForecastAId,
    Guid ForecastBId,
    string LabelA,
    string LabelB,
    decimal EndBalanceA,
    decimal EndBalanceB,
    decimal TotalDrift,
    IReadOnlyList<DayVariance> DayVariances,
    IReadOnlyList<ExpenseChange> ExpenseChanges);

public sealed record DayVariance(int DayNumber, decimal BalanceA, decimal BalanceB, decimal Variance);

public sealed record ExpenseChange(
    string ExpenseName,
    string ChangeType,   // "ADDED", "REMOVED", "MODIFIED"
    decimal? AmountA,
    decimal? AmountB);
```

#### `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/CompareForecasts/CompareForecastsHandler.cs`
- Inject `IForecastRepository`
- Load both `ForecastVersion` objects, enforce `HouseholdId` matches on both, throw `ForecastNotFoundException` for either missing
- **Day variance:** align `DailyEntries` by `DayNumber`; for each day compute `Variance = BalanceA - BalanceB`; days present in only one forecast get the other side as `0`
- **Total drift:** `EndBalanceA - EndBalanceB` (use `GetEndOfMonthBalance()` already on `ForecastVersion`)
- **Expense changes:** diff the two `ExpenseSnapshots` lists by `OriginalExpenseId`:
  - Present in A only → `REMOVED`
  - Present in B only → `ADDED`
  - Present in both with different `Amount` → `MODIFIED`
- Return `ComparisonResult`

#### API endpoint to add to `ForecastController.cs`
```csharp
[HttpGet("compare")]
public async Task<IActionResult> Compare(
    Guid budgetId,
    [FromQuery] Guid versionA,
    [FromQuery] Guid versionB,
    CancellationToken ct)
{
    var result = await _mediator.Send(new CompareForecastsQuery(versionA, versionB, HouseholdId), ct);
    return Ok(result);
}
```

---

## Priority 2 — Missing Cross-Context Event Wiring

### 2.1 ForecastEngine Domain Events

**Status:** `src/Modules/MonthlyBudget.ForecastEngine/Domain/Events/` folder is **empty**.

All events must extend `DomainEventBase` from `MonthlyBudget.SharedKernel.Events` (which already implements both `IDomainEvent` and MediatR `INotification`).

**Files to create:**

#### `src/Modules/MonthlyBudget.ForecastEngine/Domain/Events/ForecastGenerated.cs`
```csharp
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Domain.Events;
public sealed class ForecastGenerated : DomainEventBase
{
    public Guid ForecastId { get; }
    public Guid BudgetId { get; }
    public Guid HouseholdId { get; }
    public string ForecastType { get; }
    public ForecastGenerated(Guid forecastId, Guid budgetId, Guid householdId, string forecastType)
    { ForecastId = forecastId; BudgetId = budgetId; HouseholdId = householdId; ForecastType = forecastType; }
}
```

#### `src/Modules/MonthlyBudget.ForecastEngine/Domain/Events/SnapshotSaved.cs`
```csharp
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Domain.Events;
public sealed class SnapshotSaved : DomainEventBase
{
    public Guid ForecastId { get; }
    public Guid BudgetId { get; }
    public SnapshotSaved(Guid forecastId, Guid budgetId) { ForecastId = forecastId; BudgetId = budgetId; }
}
```

#### `src/Modules/MonthlyBudget.ForecastEngine/Domain/Events/ForecastStaleMarked.cs`
```csharp
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Domain.Events;
public sealed class ForecastStaleMarked : DomainEventBase
{
    public Guid ForecastId { get; }
    public string Reason { get; }
    public ForecastStaleMarked(Guid forecastId, string reason) { ForecastId = forecastId; Reason = reason; }
}
```

---

### 2.2 `IForecastEventPublisher` Port

**Status:** Missing from `src/Modules/MonthlyBudget.ForecastEngine/Application/Ports/`.

**File to create:**

#### `src/Modules/MonthlyBudget.ForecastEngine/Application/Ports/IForecastEventPublisher.cs`
Model it exactly after `IBudgetEventPublisher`:
```csharp
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Application.Ports;
public interface IForecastEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
```

---

### 2.3 `MediatRForecastEventPublisher` Adapter

**Status:** `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Events/` folder is **empty**.

**File to create:**

#### `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Events/MediatRForecastEventPublisher.cs`
Model it exactly after `MediatRBudgetEventPublisher`:
```csharp
using MediatR;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.ForecastEngine.Infrastructure.Events;
public sealed class MediatRForecastEventPublisher : IForecastEventPublisher
{
    private readonly IPublisher _publisher;
    public MediatRForecastEventPublisher(IPublisher publisher) { _publisher = publisher; }
    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        if (domainEvent is INotification notification)
            await _publisher.Publish(notification, ct);
    }
}
```

Then register it in `ServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<IForecastEventPublisher, MediatRForecastEventPublisher>();
```

---

### 2.4 `BudgetEventHandler` — Forecast Staleness Handler

**Status:** `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Events/` folder is **empty**.

This is the MediatR `INotificationHandler` that listens to Budget Management domain events and marks forecasts as stale. It is the implementation of the `PropagateExpenseChanges` corrective policy.

**File to create:**

#### `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Events/BudgetEventHandler.cs`
```csharp
using MediatR;
using MonthlyBudget.BudgetManagement.Domain.Events;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
namespace MonthlyBudget.ForecastEngine.Infrastructure.Events;

/// <summary>
/// Consumes Budget Management domain events and marks affected forecasts as stale.
/// Does NOT auto-regenerate — only sets a staleness indicator (PropagateExpenseChanges policy).
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
        {
            await _publisher.PublishAsync(
                new MonthlyBudget.ForecastEngine.Domain.Events.ForecastStaleMarked(forecast.ForecastId, reason), ct);
        }
    }
}
```

> **Important:** For this handler to be discovered by MediatR, its assembly must be registered. The `BudgetEventHandler` lives in the `MonthlyBudget.ForecastEngine` module assembly, which is already registered via `GenerateForecastHandler`. No extra DI wiring is needed.

---

### 2.5 `AutoSnapshotOnReforecast` Policy in `ReforecastHandler`

**Status:** `ReforecastHandler.cs` does **not** auto-snapshot the parent before creating a re-forecast.

**File to update:** `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/Reforecast/ReforecastHandler.cs`

After loading the parent forecast and before creating the new one, insert:
```csharp
// AutoSnapshotOnReforecast policy: snapshot parent if not already snapshotted
if (!parent.IsSnapshot)
{
    parent.MarkAsSnapshot();
    await _repo.SaveAsync(parent, ct);
}
```

This requires `PostgresForecastRepository.SaveAsync` to support updates (see Priority 1.2 fix above).

---

## Priority 3 — Missing API Endpoints

### 3.1 `GET /api/v1/households/{householdId}` — GetHousehold

**Status:** Missing from `AuthHouseholdController.cs` (`HouseholdController` class).

Add to `HouseholdController` in `src/Modules/MonthlyBudget.IdentityHousehold/Infrastructure/Controllers/AuthHouseholdController.cs`:
```csharp
[HttpGet("{householdId:guid}")]
public async Task<IActionResult> Get(Guid householdId, CancellationToken ct)
{
    // Inline query — no separate use case needed for MVP; read directly or add a GetHouseholdQuery
    // Enforce: requesting user must belong to the household (UserId check against members)
    // For MVP simplicity, return the household only if the requesting user is a member
    ...
}
```

Create a minimal `GetHouseholdQuery` + handler in `Application/Features/` following the same pattern as `GetBudgetHandler`, or inline directly in the controller if it's a trivial repo read.

---

## Priority 4 — Domain Invariant Gap

### 4.1 INV-H4: Only one pending invitation per household

**Status:** `InviteMemberHandler.cs` does **not** check for an existing `PENDING` invitation.

**File to update:** `src/Modules/MonthlyBudget.IdentityHousehold/Application/Features/InviteMember/InviteMemberHandler.cs`

`IInvitationRepository` already has `FindByHouseholdAndEmailAsync`. Add a method `FindPendingByHouseholdAsync(Guid householdId)` to the interface and repository, then enforce before creating:
```csharp
// INV-H4: reject if a PENDING invitation already exists for this household
var existing = await _invitations.FindPendingByHouseholdAsync(cmd.HouseholdId, ct);
if (existing != null) throw new PendingInvitationExistsException(cmd.HouseholdId);
```

Create `PendingInvitationExistsException` in `Domain/Exceptions/` following the same pattern as `HouseholdFullException`:
```csharp
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
public sealed class PendingInvitationExistsException : IdentityDomainException
{
    public PendingInvitationExistsException(Guid householdId)
        : base($"A pending invitation already exists for household {householdId}.") { }
}
```

Add to `IInvitationRepository` (in `Domain/Repositories/IIdentityRepositories.cs`):
```csharp
Task<Invitation?> FindPendingByHouseholdAsync(Guid householdId, CancellationToken ct = default);
```

Implement in `PostgresIdentityRepositories.cs` — find an `INVITATION` row where `household_id = householdId AND status = 'PENDING'`.

---

## Priority 5 — Infrastructure Gaps

### 5.1 `RequestLoggerMiddleware`

**Status:** Referenced in the architecture spec directory structure but not implemented.

**File to create:** `src/MonthlyBudget.Infrastructure/Middleware/RequestLoggerMiddleware.cs`

Standard ASP.NET Core middleware that logs `{Method} {Path} → {StatusCode}` using `ILogger<RequestLoggerMiddleware>`.

Register in `Program.cs` before `app.UseHttpsRedirection()`:
```csharp
app.UseMiddleware<RequestLoggerMiddleware>();
```

---

## Priority 6 — Test Coverage Gaps

### 6.1 Missing domain tests for new features

Add to `tests/MonthlyBudget.ForecastEngine.Tests/Domain/ForecastEngineTests.cs`:

- `SaveSnapshot_NonSnapshotForecast_SetsIsSnapshotTrue` — verify `MarkAsSnapshot()` sets `IsSnapshot = true`
- `SaveSnapshot_AlreadySnapshot_RemainsTrue` — idempotency check
- `CompareForecasts_SameForecasts_ZeroDrift` — both inputs identical
- `CompareForecasts_DifferentEndBalances_CorrectTotalDrift` — drift = EndBalanceA - EndBalanceB
- `ReforecastHandler_ParentNotSnapshot_AutoSnapshotsParent` — (application layer test) verify `MarkAsSnapshot` was called on parent

Add to `tests/MonthlyBudget.BudgetManagement.Tests/Domain/MonthlyBudgetTests.cs`:

- `ActivateBudget_WithIncomeSources_StatusBecomesActive` — verify `Activate()` from DRAFT with income (already tested in domain; add integration via handler)

### 6.2 Integration tests (empty project)

The project at `tests/MonthlyBudget.Integration.Tests/` is an empty shell.  
Add `Microsoft.AspNetCore.Mvc.Testing` to the `.csproj`:
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="4.4.0" />
```

Create at minimum:

- `tests/MonthlyBudget.Integration.Tests/BudgetManagementApiTests.cs` — `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, `POST /api/v1/households`, `POST /api/v1/budgets`, `POST /api/v1/budgets/{id}/activate`
- `tests/MonthlyBudget.Integration.Tests/ForecastApiTests.cs` — `POST /api/v1/budgets/{id}/forecasts`, `POST /api/v1/budgets/{id}/forecasts/{id}/snapshot`, forecast reforecast flow

Use `WebApplicationFactory<Program>` (the `Program` class is already declared `partial` for this purpose) with a Testcontainers PostgreSQL instance.

---

## What Is Already Done — Do Not Touch

| Component | Status |
|-----------|--------|
| `MonthlyBudget` aggregate + all INV-B1–B8 invariants | ✅ Complete, 29 tests |
| All Budget Management domain events (11 events) | ✅ Complete |
| All Budget Management domain exceptions | ✅ Complete |
| Budget Management application handlers (CreateBudget, Add/Update/Remove Income, Add/Update/Remove Expense, ToggleExclusion, Rollover, GetBudget) | ✅ Complete |
| Budget API controllers (BudgetController, IncomeController, ExpenseController) | ✅ Complete |
| `ForecastVersion` aggregate + `ForecastCalculator` domain service | ✅ Complete, 9 tests |
| ForecastEngine domain exceptions (ForecastDomainException, ForecastNotFoundException, InvalidReforecastException, SnapshotImmutableException) | ✅ Complete |
| ForecastEngine application handlers (GenerateForecast, GetForecast, Reforecast) | ✅ Complete |
| ForecastController (generate, get, list, reforecast endpoints) | ✅ Complete |
| Identity & Household domain (User, Household, Invitation, Member, INV-H1, INV-H2) | ✅ Complete, 12 tests |
| Identity application handlers (Register, Authenticate, CreateHousehold, InviteMember, JoinHousehold) | ✅ Complete |
| AuthController + HouseholdController (register, login, create household, invite, join) | ✅ Complete |
| `BudgetManagementAcl` (ACL adapter from Budget context to Forecast context) | ✅ Complete |
| `PostgresBudgetRepository`, `PostgresIdentityRepositories` | ✅ Complete |
| `PostgresForecastRepository` (needs SaveAsync upsert fix — see §1.2) | ⚠️ Needs fix |
| EF Core `AppDbContext`, all entity configurations, 2 migrations | ✅ Complete |
| `HouseholdScopeMiddleware`, JWT auth wiring, Swagger, `Program.cs` | ✅ Complete |
| `ServiceCollectionExtensions` DI wiring | ✅ Mostly complete (needs `IForecastEventPublisher` — see §2.3) |
| `docker-compose.yml` + `Dockerfile` | ✅ Present |

---

## Summary Checklist

```
Priority 1 — Missing features (backend broken without these)
[ ] 1.1 ActivateBudget command + handler + POST /budgets/{id}/activate endpoint
[ ] 1.2 SaveSnapshot command + handler + POST /forecasts/{id}/snapshot endpoint + PostgresForecastRepository SaveAsync upsert fix
[ ] 1.3 CompareForecasts query + handler + GET /forecasts/compare endpoint

Priority 2 — Cross-context event wiring (architecture contract)
[ ] 2.1 ForecastEngine domain events: ForecastGenerated, SnapshotSaved, ForecastStaleMarked
[ ] 2.2 IForecastEventPublisher port
[ ] 2.3 MediatRForecastEventPublisher adapter + DI registration
[ ] 2.4 BudgetEventHandler (staleness propagation for ExpenseAdded/Updated/Removed/ExclusionToggled)
[ ] 2.5 AutoSnapshotOnReforecast policy in ReforecastHandler

Priority 3 — Missing API endpoints
[ ] 3.1 GET /api/v1/households/{householdId}

Priority 4 — Domain invariant gap
[ ] 4.1 INV-H4: PendingInvitationExistsException + FindPendingByHouseholdAsync + enforcement in InviteMemberHandler

Priority 5 — Infrastructure
[ ] 5.1 RequestLoggerMiddleware

Priority 6 — Tests
[ ] 6.1 Domain tests for SaveSnapshot, CompareForecasts, AutoSnapshot policy
[ ] 6.2 Integration tests (WebApplicationFactory + Testcontainers PostgreSQL)
```

---

## Key Code Conventions to Follow

- All domain events extend `DomainEventBase` from `MonthlyBudget.SharedKernel.Events` — this gives them both `IDomainEvent` and MediatR `INotification` for free
- All domain exceptions in a context extend that context's base exception (e.g., `ForecastDomainException`, `IdentityDomainException`, `DomainException`)
- Application handlers follow the pattern: load → validate household ownership → mutate domain → save → publish events → return DTO
- `HouseholdId` is always extracted from the JWT claim `"householdId"` in controllers — never from the request body
- All application layer handlers are `IRequestHandler<TCommand, TResult>` via MediatR — no direct controller → repository calls
- Domain layer has **zero external library imports** — only `MonthlyBudget.SharedKernel` references are allowed

