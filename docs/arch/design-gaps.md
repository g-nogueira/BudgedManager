# Design Gaps — Backend Changes Required for Frontend

> Generated: 2026-03-21.
> Source: PRD (`docs/product/PRD.md`), screen designs (`docs/product/screens/*.html`), screen mapping (`docs/product/monthly-budget-screens.md`), and codebase verification.
>
> Each gap specifies: the user story driving it, the exact files to change, the code changes, and priority.
> **Downstream agents:** implement gaps in priority order (P0 → P1 → P2).

---

## GAP-1: Remove `startBalance` from Forecast Generation Request (P0)

**User Story:** US-3.1 — Generate daily forecast
**Invariant:** INV-F6 — Start balance for ORIGINAL equals total income
**Screen:** SCR-04 (Forecast Detail) — user clicks "Generate Forecast" button, no balance input

### Problem

`GenerateForecastHandler` receives `startBalance` from the client via `GenerateForecastRequest(decimal StartBalance)`, but INV-F6 requires the start balance to be derived from the budget's total income. The handler already fetches `BudgetData` from `IBudgetDataPort` which contains `TotalIncome`, but ignores it.

### Changes Required

| # | File | Change |
|---|---|---|
| 1 | `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Dto/RequestDtos.cs` | Remove `GenerateForecastRequest` record entirely (or replace with empty marker record if needed for deserialization) |
| 2 | `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/GenerateForecast/GenerateForecastCommand.cs` | Remove `StartBalance` parameter: `record GenerateForecastCommand(Guid BudgetId, Guid HouseholdId) : IRequest<...>` |
| 3 | `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/GenerateForecast/GenerateForecastHandler.cs` | Line 23: Change `cmd.StartBalance` → `data.TotalIncome` in the `ForecastCalculator.Generate(...)` call |
| 4 | `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Controllers/ForecastController.cs` | Line 22-23: Remove `[FromBody] GenerateForecastRequest req` parameter. Change to `new GenerateForecastCommand(budgetId, HouseholdId)` |
| 5 | `tests/MonthlyBudget.ForecastEngine.Tests/` | Update any tests using `GenerateForecastCommand` to remove `StartBalance` argument |
| 6 | `tests/MonthlyBudget.Integration.Tests/ForecastApiTests.cs` | Update integration tests to POST with no body |

### Verification

```http
POST /api/v1/budgets/{budgetId}/forecasts
Authorization: Bearer {token}
Content-Length: 0
```

Expected: 201 with `startBalance` in the returned forecast == budget's `totalIncome`.

---

## GAP-2: Add `expenseAdjustments[]` to Reforecast (P0)

**User Story:** US-4.3 — Adjust future expenses during re-forecast
**Screen:** SCR-07 (Reforecast Step 2) — user modifies/removes/adds expenses in a table before simulating

### Problem

`ReforecastCommand` and `ReforecastRequest` have no `expenseAdjustments` parameter. The handler copies parent's `ExpenseSnapshots` without modification. The domain already has `ExpenseSnapshot.CreateAdjusted()` for modifying snapshots, but it's never called.

### Changes Required

| # | File | Change |
|---|---|---|
| 1 | `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Dto/RequestDtos.cs` | Add DTO records: |

```csharp
public record ExpenseAdjustmentDto(
    Guid? OriginalExpenseId,
    string Action, // "MODIFY", "REMOVE", "ADD"
    decimal? NewAmount,
    string? Name,
    string? Category,
    int? DayOfMonth,
    bool? IsSpread);

// Update existing:
public record ReforecastRequest(
    int StartDay,
    decimal ActualBalance,
    string VersionLabel,
    List<ExpenseAdjustmentDto>? ExpenseAdjustments);
```

| # | File | Change |
|---|---|---|
| 2 | `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/Reforecast/ReforecastCommand.cs` | Add adjustments parameter: |

```csharp
public sealed record ExpenseAdjustment(
    Guid? OriginalExpenseId,
    string Action, // "MODIFY", "REMOVE", "ADD"
    decimal? NewAmount,
    string? Name,
    string? Category,
    int? DayOfMonth,
    bool? IsSpread);

public sealed record ReforecastCommand(
    Guid BudgetId, Guid HouseholdId, Guid ParentForecastId,
    int StartDay, decimal ActualBalance, string VersionLabel,
    IReadOnlyList<ExpenseAdjustment>? ExpenseAdjustments) : IRequest<ReforecastResult>;
```

| # | File | Change |
|---|---|---|
| 3 | `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/Reforecast/ReforecastHandler.cs` | After `var snapshots = parent.ExpenseSnapshots.ToList();` (line ~28), apply adjustments: |

```csharp
// Apply expense adjustments if provided
if (cmd.ExpenseAdjustments is { Count: > 0 })
{
    var adjustedSnapshots = new List<ExpenseSnapshot>();
    var removedIds = cmd.ExpenseAdjustments
        .Where(a => a.Action == "REMOVE")
        .Select(a => a.OriginalExpenseId!.Value)
        .ToHashSet();
    
    foreach (var snap in snapshots)
    {
        if (removedIds.Contains(snap.OriginalExpenseId))
            continue; // REMOVE
        
        var mod = cmd.ExpenseAdjustments
            .FirstOrDefault(a => a.Action == "MODIFY" && a.OriginalExpenseId == snap.OriginalExpenseId);
        if (mod != null)
            adjustedSnapshots.Add(ExpenseSnapshot.CreateAdjusted(Guid.Empty, snap, mod.NewAmount));
        else
            adjustedSnapshots.Add(snap);
    }
    
    // ADD new expenses
    foreach (var add in cmd.ExpenseAdjustments.Where(a => a.Action == "ADD"))
    {
        var cat = Enum.Parse<SnapshotCategory>(add.Category!, ignoreCase: true);
        adjustedSnapshots.Add(ExpenseSnapshot.Create(
            Guid.Empty, Guid.NewGuid(), add.Name!, cat,
            add.DayOfMonth, add.IsSpread ?? false, add.NewAmount ?? 0m, false));
    }
    
    snapshots = adjustedSnapshots;
}
```

> Note: `Guid.Empty` for `forecastId` is a placeholder — `ForecastCalculator.Reforecast()` assigns the real `forecastId` to each snapshot. Verify that the calculator re-assigns `ForecastId` on snapshots; if not, the handler must do it post-creation.

| # | File | Change |
|---|---|---|
| 4 | `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Controllers/ForecastController.cs` | Line 53: Map DTO adjustments to command: |

```csharp
var adjustments = req.ExpenseAdjustments?.Select(a =>
    new ExpenseAdjustment(a.OriginalExpenseId, a.Action, a.NewAmount,
        a.Name, a.Category, a.DayOfMonth, a.IsSpread)).ToList();

var result = await _mediator.Send(new ReforecastCommand(
    budgetId, HouseholdId, forecastId, req.StartDay,
    req.ActualBalance, req.VersionLabel, adjustments), ct);
```

| # | File | Change |
|---|---|---|
| 5 | `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/Reforecast/ReforecastValidator.cs` | Create FluentValidation validator: validate `Action` is one of `MODIFY/REMOVE/ADD`, validate required fields per action type |
| 6 | Tests | Add unit tests: reforecast with MODIFY, REMOVE, ADD, and empty adjustments |

### Verification

```http
POST /api/v1/budgets/{budgetId}/forecasts/{forecastId}/reforecast
Content-Type: application/json
{
  "startDay": 15,
  "actualBalance": 3100.00,
  "versionLabel": "Mid-month re-forecast",
  "expenseAdjustments": [
    { "originalExpenseId": "...", "action": "MODIFY", "newAmount": 95.00 }
  ]
}
```

Expected: 201, returned forecast reflects adjusted expenses.

---

## GAP-3: Add optional `actualBalance` to Snapshot Save (P1)

**User Story:** US-4.1 — Save forecast snapshot with actual bank balance
**Screen:** SCR-04 (Forecast Detail) — snapshot button with optional balance input

### Problem

`SaveSnapshotCommand(Guid ForecastId, Guid HouseholdId)` has no `actualBalance` parameter. The ForecastVersion aggregate has `ActualBalance` property but it's only set during reforecast, not during snapshot save.

### Changes Required

| # | File | Change |
|---|---|---|
| 1 | `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Dto/RequestDtos.cs` | Add: `public record SaveSnapshotRequest(decimal? ActualBalance);` |
| 2 | `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/SaveSnapshot/SaveSnapshotCommand.cs` | Add parameter: `record SaveSnapshotCommand(Guid ForecastId, Guid HouseholdId, decimal? ActualBalance)` |
| 3 | `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/SaveSnapshot/SaveSnapshotHandler.cs` | After `MarkAsSnapshot()`, set `ActualBalance` if provided (add setter method to `ForecastVersion` if needed) |
| 4 | `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Controllers/ForecastController.cs` | Line 47-49: Add `[FromBody] SaveSnapshotRequest? req` parameter. Pass `req?.ActualBalance` to command |
| 5 | `src/Modules/MonthlyBudget.ForecastEngine/Domain/Entities/ForecastVersion.cs` | Add method: `public void SetActualBalance(decimal balance)` — only allowed before or during snapshot save |

---

## GAP-4: Add Budget Listing Endpoint (P1)

**User Story:** US-1.1 — View monthly budget
**Screen:** SCR-02 (Dashboard) — month navigation (chevron arrows) requires listing all budget months

### Problem

No `GET /api/v1/budgets` endpoint exists. The dashboard month navigation needs to fetch all budget months for the household to enable prev/next navigation. Only `GetById` and `GetByMonth` queries exist.

### Changes Required

| # | File | Change |
|---|---|---|
| 1 | `src/Modules/MonthlyBudget.BudgetManagement/Application/Features/GetBudget/` | Add query: `GetBudgetsByHouseholdQuery.cs` |

```csharp
public sealed record GetBudgetsByHouseholdQuery(Guid HouseholdId) : IRequest<IReadOnlyList<BudgetSummaryDto>>;
public sealed record BudgetSummaryDto(
    Guid BudgetId, string YearMonth, string Status,
    decimal TotalIncome, decimal TotalExpenses, DateTime CreatedAt);
```

| # | File | Change |
|---|---|---|
| 2 | `src/Modules/MonthlyBudget.BudgetManagement/Application/Features/GetBudget/` | Add handler: `GetBudgetsByHouseholdHandler.cs` |
| 3 | `src/Modules/MonthlyBudget.BudgetManagement/Domain/Repositories/IBudgetRepository.cs` | Add method: `Task<IReadOnlyList<MonthlyBudget>> FindByHouseholdAsync(Guid householdId, CancellationToken ct)` |
| 4 | `src/MonthlyBudget.Infrastructure/Repositories/PostgresBudgetRepository.cs` | Implement `FindByHouseholdAsync` |
| 5 | `src/Modules/MonthlyBudget.BudgetManagement/Infrastructure/Controllers/BudgetController.cs` | Add `[HttpGet]` action returning list of `BudgetSummaryDto` |

---

## GAP-5: Add Close Budget Endpoint (P1)

**User Story:** Implied by INV-B6 — `DRAFT → ACTIVE → CLOSED`
**Screen:** SCR-10 (Rollover) — closing old budget is part of the rollover flow

### Problem

Domain `Close()` method exists on the aggregate with full test coverage, but there's no command, handler, or controller action to expose it.

### Changes Required

| # | File | Change |
|---|---|---|
| 1 | `src/Modules/MonthlyBudget.BudgetManagement/Application/Features/CloseBudget/CloseBudgetCommand.cs` | Create: `record CloseBudgetCommand(Guid BudgetId, Guid HouseholdId) : IRequest<CloseBudgetResult>` |
| 2 | `src/Modules/MonthlyBudget.BudgetManagement/Application/Features/CloseBudget/CloseBudgetHandler.cs` | Create handler: load budget, call `Close()`, save |
| 3 | `src/Modules/MonthlyBudget.BudgetManagement/Infrastructure/Controllers/BudgetController.cs` | Add `[HttpPost("{budgetId:guid}/close")]` action |

---

## GAP-6: Add Token Refresh Endpoint (P1)

**User Story:** US-7.1 — Secure login
**Screen:** SCR-01 (Login) — JWT tokens expire after 15 minutes; frontend needs silent refresh

### Problem

`AuthenticateUserResult` returns both `AccessToken` and `RefreshToken`, but there's no endpoint to exchange a refresh token for a new token pair. `ITokenService.GenerateRefreshToken()` exists but there's no validation/rotation logic.

### Changes Required

| # | File | Change |
|---|---|---|
| 1 | `src/Modules/MonthlyBudget.IdentityHousehold/Application/Ports/IIdentityPorts.cs` | Add to `ITokenService`: `bool ValidateRefreshToken(string token, Guid userId)` |
| 2 | `src/Modules/MonthlyBudget.IdentityHousehold/Application/Features/RefreshToken/RefreshTokenCommand.cs` | Create: `record RefreshTokenCommand(string RefreshToken) : IRequest<AuthenticateUserResult>` |
| 3 | `src/Modules/MonthlyBudget.IdentityHousehold/Application/Features/RefreshToken/RefreshTokenHandler.cs` | Create handler: validate refresh token, load user + household, generate new token pair |
| 4 | `src/Modules/MonthlyBudget.IdentityHousehold/Infrastructure/Auth/JwtTokenService.cs` | Implement `ValidateRefreshToken()` — requires storing refresh tokens (hashed) in database |
| 5 | `src/Modules/MonthlyBudget.IdentityHousehold/Infrastructure/Controllers/AuthController.cs` | Add `[HttpPost("refresh")] [AllowAnonymous]` action |
| 6 | `src/Modules/MonthlyBudget.IdentityHousehold/Infrastructure/Dto/RequestDtos.cs` | Add: `record RefreshTokenRequest(string RefreshToken)` |
| 7 | Persistence | New table `identity.refresh_tokens (token_hash, user_id, expires_at, created_at)` — EF configuration in `src/MonthlyBudget.Infrastructure/Database/Configurations/` |

### Decision Required

Refresh token storage adds a new entity to Identity & Household. This is standard JWT rotation practice. **No ADR needed** — JWT refresh is listed in the tech stack. The refresh token entity (`RefreshTokenEntry`) belongs to the Identity domain.

---

## GAP-7: `ForecastCalculator.Reforecast` Must Accept Adjusted Snapshots (P0 — depends on GAP-2)

**User Story:** US-4.3 — Adjust future expenses during re-forecast
**Screen:** SCR-08 (Reforecast Step 3) — shows simulation result with adjusted expenses

### Problem

Verify that `ForecastCalculator.Reforecast()` correctly assigns `ForecastId` to the snapshots passed in. If it creates copies internally, the adjusted snapshots from GAP-2 may be discarded.

### Investigation Required

| File | Check |
|---|---|
| `src/Modules/MonthlyBudget.ForecastEngine/Domain/Services/ForecastCalculator.cs` | Does `Reforecast()` create new `ExpenseSnapshot` instances from the input list, or does it use them directly? |

If it copies: no issue — adjusted amounts carry through.
If it uses references directly: the `Guid.Empty` ForecastId placeholder from GAP-2 must be handled.

---

## Summary — Implementation Priority

| Priority | Gap | Effort | Blocked By |
|---|---|---|---|
| **P0** | GAP-1: Remove `startBalance` from forecast generation | S (small) | — |
| **P0** | GAP-2: Add `expenseAdjustments` to reforecast | L (large) | — |
| **P0** | GAP-7: Verify `ForecastCalculator` snapshot handling | XS (investigation) | GAP-2 |
| **P1** | GAP-3: Add `actualBalance` to snapshot | S (small) | — |
| **P1** | GAP-4: Add budget listing endpoint | M (medium) | — |
| **P1** | GAP-5: Add close budget endpoint | S (small) | — |
| **P1** | GAP-6: Add token refresh endpoint | L (large) | — |

**Not required for MVP frontend launch (P2-deferred):**
- Household name update endpoint — not shown in design SCR-09
- `IncomeSource.Description` field — design shows "Primary Employment" subtitle but this can be a frontend-only label initially
