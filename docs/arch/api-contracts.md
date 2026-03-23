# API Contracts Reference

> **Source of truth** for all frontend API clients and backend controllers.
> Last synced with codebase: 2026-03-21.
> All endpoints require JWT authentication (except auth endpoints marked `[AllowAnonymous]`).
> `householdId` is extracted from the JWT `householdId` claim — never sent in request body.
> `userId` is extracted from the JWT `sub` / `NameIdentifier` claim.

---

## Budget Management API

### Budget Endpoints

| Method | Endpoint | Controller | Request Body | Success Response | Status |
|---|---|---|---|---|---|
| `POST` | `/api/v1/budgets` | `BudgetController` | [`CreateBudgetRequest`](#createbudgetrequest) | [`CreateBudgetResult`](#createbudgetresult) | 201 |
| `GET` | `/api/v1/budgets/{budgetId}` | `BudgetController` | — | [`BudgetDto`](#budgetdto) | 200 |
| `GET` | `/api/v1/budgets/by-month/{yearMonth}` | `BudgetController` | — | [`BudgetDto`](#budgetdto) | 200 |
| `GET` | `/api/v1/budgets` | `BudgetController` | — | [`BudgetSummaryDto[]`](#budgetsummarydto) | 200 |
| `POST` | `/api/v1/budgets/{budgetId}/activate` | `BudgetController` | — | [`ActivateBudgetResult`](#activatebudgetresult) | 200 |
| `POST` | `/api/v1/budgets/{budgetId}/close` | `BudgetController` | — | `{ budgetId, status: "CLOSED" }` | 200 |
| `POST` | `/api/v1/budgets/{budgetId}/rollover` | `BudgetController` | [`RolloverMonthRequest`](#rollovermonthrequest) | [`RolloverMonthResult`](#rollovermonthresult) | 201 |

### Income Endpoints

> **Note:** Income route is `/income` (singular) — this matches current implementation.

| Method | Endpoint | Controller | Request Body | Success Response | Status |
|---|---|---|---|---|---|
| `POST` | `/api/v1/budgets/{budgetId}/income` | `IncomeController` | [`AddIncomeRequest`](#addincomerequest) | [`AddIncomeResult`](#addincomeresult) | 201 |
| `PUT` | `/api/v1/budgets/{budgetId}/income/{incomeId}` | `IncomeController` | [`UpdateIncomeRequest`](#updateincomerequest) | — | 204 |
| `DELETE` | `/api/v1/budgets/{budgetId}/income/{incomeId}` | `IncomeController` | — | — | 204 |

### Expense Endpoints

| Method | Endpoint | Controller | Request Body | Success Response | Status |
|---|---|---|---|---|---|
| `POST` | `/api/v1/budgets/{budgetId}/expenses` | `ExpenseController` | [`AddExpenseRequest`](#addexpenserequest) | [`AddExpenseResult`](#addexpenseresult) | 201 |
| `PUT` | `/api/v1/budgets/{budgetId}/expenses/{expenseId}` | `ExpenseController` | [`UpdateExpenseRequest`](#updateexpenserequest) | — | 204 |
| `DELETE` | `/api/v1/budgets/{budgetId}/expenses/{expenseId}` | `ExpenseController` | — | — | 204 |
| `PATCH` | `/api/v1/budgets/{budgetId}/expenses/{expenseId}/exclusion` | `ExpenseController` | [`ToggleExclusionRequest`](#toggleexclusionrequest) | — | 204 |

---

## Forecast Engine API

All forecast endpoints are nested under the budget: `/api/v1/budgets/{budgetId}/forecasts/...`

| Method | Endpoint | Controller | Request Body | Success Response | Status |
|---|---|---|---|---|---|
| `POST` | `/api/v1/budgets/{budgetId}/forecasts` | `ForecastController` | — | [`GenerateForecastResult`](#generateforecastresult) | 201 |
| `GET` | `/api/v1/budgets/{budgetId}/forecasts` | `ForecastController` | — | [`ForecastSummaryDto[]`](#forecastsummarydto) | 200 |
| `GET` | `/api/v1/budgets/{budgetId}/forecasts/{forecastId}` | `ForecastController` | — | [`ForecastDto`](#forecastdto) | 200 |
| `POST` | `/api/v1/budgets/{budgetId}/forecasts/{forecastId}/snapshot` | `ForecastController` | [`SaveSnapshotRequest`](#savesnapshotrequest) | [`SaveSnapshotResult`](#savesnapshotresult) | 200 |
| `POST` | `/api/v1/budgets/{budgetId}/forecasts/{forecastId}/reforecast` | `ForecastController` | [`ReforecastRequest`](#reforecastrequest) | [`ReforecastResult`](#reforecastresult) | 201 |
| `GET` | `/api/v1/budgets/{budgetId}/forecasts/compare?versionA={id}&versionB={id}` | `ForecastController` | — | [`ComparisonResult`](#comparisonresult) | 200 |

> **CHANGED from previous spec:** Forecast generation no longer accepts `startBalance` in the request body. The handler derives `startBalance` from the budget's total income (INV-F6). `POST` with no body is sufficient.

> **CHANGED from previous spec:** Reforecast now accepts `expenseAdjustments[]` for US-4.3 (adjust future expenses during re-forecast). See [ReforecastRequest](#reforecastrequest).

> **CHANGED from previous spec:** Snapshot save now accepts optional `actualBalance` for US-4.1 AC (user-entered actual bank balance at snapshot time). See [SaveSnapshotRequest](#savesnapshotrequest).

---

## Identity & Household API

### Auth Endpoints

| Method | Endpoint | Controller | Auth | Request Body | Success Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/api/v1/auth/register` | `AuthController` | `[AllowAnonymous]` | [`RegisterRequest`](#registerrequest) | [`RegisterUserResult`](#registeruserresult) | 201 |
| `POST` | `/api/v1/auth/login` | `AuthController` | `[AllowAnonymous]` | [`LoginRequest`](#loginrequest) | [`AuthenticateUserResult`](#authenticateuserresult) | 200 |
| `POST` | `/api/v1/auth/refresh` | `AuthController` | `[AllowAnonymous]` | [`RefreshTokenRequest`](#refreshtokenrequest) | [`AuthenticateUserResult`](#authenticateuserresult) | 200 |

### Household Endpoints

| Method | Endpoint | Controller | Request Body | Success Response | Status |
|---|---|---|---|---|---|
| `POST` | `/api/v1/households` | `HouseholdController` | [`CreateHouseholdRequest`](#createhouseholdrequest) | [`CreateHouseholdResult`](#createhouseholdresult) | 201 |
| `GET` | `/api/v1/households/{householdId}` | `HouseholdController` | — | [`HouseholdDto`](#householddto) | 200 |
| `POST` | `/api/v1/households/{householdId}/invite` | `HouseholdController` | [`InviteRequest`](#inviterequest) | [`InviteMemberResult`](#invitememberresult) | 201 |
| `POST` | `/api/v1/households/join` | `HouseholdController` | [`JoinRequest`](#joinrequest) | [`JoinHouseholdResult`](#joinhouseholdresult) | 200 |

---

## Request & Response Schemas

### Budget Management — Requests

#### CreateBudgetRequest
```json
{ "yearMonth": "2026-03" }
```
> File: `src/Modules/MonthlyBudget.BudgetManagement/Infrastructure/Dto/RequestDtos.cs`

#### RolloverMonthRequest
```json
{ "targetYearMonth": "2026-04" }
```

#### AddIncomeRequest
```json
{ "name": "Salary", "amount": 4200.00 }
```

#### UpdateIncomeRequest
```json
{ "name": "Salary", "amount": 4500.00 }
```

#### AddExpenseRequest
```json
{
  "name": "Rent",
  "category": "FIXED",
  "dayOfMonth": 1,
  "isSpread": false,
  "amount": 1200.00
}
```
> `category` enum values: `"FIXED"`, `"SUBSCRIPTION"`, `"VARIABLE"`
> If `isSpread` is `true`, `dayOfMonth` must be `null` (INV-B3). If `isSpread` is `false`, `dayOfMonth` must be set (INV-B4).

#### UpdateExpenseRequest
```json
{
  "name": "Rent",
  "category": "FIXED",
  "dayOfMonth": 1,
  "isSpread": false,
  "amount": 1300.00
}
```

#### ToggleExclusionRequest
```json
{ "isExcluded": true }
```

### Budget Management — Responses

#### CreateBudgetResult
```json
{ "budgetId": "guid", "status": "DRAFT" }
```

#### ActivateBudgetResult
```json
{ "budgetId": "guid", "status": "ACTIVE" }
```

#### RolloverMonthResult
```json
{ "newBudgetId": "guid" }
```

#### AddIncomeResult
```json
{ "incomeId": "guid" }
```

#### AddExpenseResult
```json
{ "expenseId": "guid" }
```

#### BudgetDto
```json
{
  "budgetId": "guid",
  "householdId": "guid",
  "yearMonth": "2026-03",
  "status": "ACTIVE",
  "incomeSources": [
    { "incomeId": "guid", "name": "Salary", "amount": 4200.00 }
  ],
  "expenses": [
    {
      "expenseId": "guid",
      "name": "Rent",
      "category": "FIXED",
      "dayOfMonth": 1,
      "isSpread": false,
      "amount": 1200.00,
      "isExcluded": false
    }
  ],
  "totalIncome": 5200.00,
  "createdAt": "2026-03-01T00:00:00Z",
  "updatedAt": "2026-03-01T00:00:00Z"
}
```
> File: `src/Modules/MonthlyBudget.BudgetManagement/Application/Features/GetBudget/GetBudgetQuery.cs`

#### BudgetSummaryDto
```json
{
  "budgetId": "guid",
  "yearMonth": "2026-03",
  "status": "ACTIVE",
  "totalIncome": 5200.00,
  "totalExpenses": 2890.98,
  "createdAt": "2026-03-01T00:00:00Z"
}
```
> **NEW** — Not yet implemented. Required by Dashboard month navigation.

---

### Forecast Engine — Requests

#### SaveSnapshotRequest
```json
{ "actualBalance": 3100.00 }
```
> `actualBalance` is optional (nullable). If omitted, the snapshot is saved without a balance anchor.
> **CHANGED:** Previous impl accepted no body. Must be updated.
> File: `src/Modules/MonthlyBudget.ForecastEngine/Infrastructure/Dto/RequestDtos.cs`

#### ReforecastRequest
```json
{
  "startDay": 20,
  "actualBalance": 3100.00,
  "versionLabel": "Re-forecast Mar 20",
  "expenseAdjustments": [
    {
      "originalExpenseId": "guid",
      "action": "MODIFY",
      "newAmount": 95.00
    },
    {
      "originalExpenseId": "guid",
      "action": "REMOVE"
    },
    {
      "action": "ADD",
      "name": "Car Repair",
      "category": "VARIABLE",
      "dayOfMonth": 22,
      "isSpread": false,
      "newAmount": 200.00
    }
  ]
}
```
> `expenseAdjustments` is optional. If omitted, re-forecast uses the parent forecast's expenses unchanged.
> `action` enum: `"MODIFY"`, `"REMOVE"`, `"ADD"`
> For `MODIFY`: `originalExpenseId` + `newAmount` required. All other expense fields preserved from parent snapshot.
> For `REMOVE`: `originalExpenseId` required. Expense excluded from re-forecast simulation.
> For `ADD`: `name`, `category`, `dayOfMonth`/`isSpread`, `newAmount` required. Creates a new `ExpenseSnapshot`.
> **CHANGED:** Previous impl had no `expenseAdjustments`. Must be added to `ReforecastRequest`, `ReforecastCommand`, and `ReforecastHandler`.

### Forecast Engine — Responses

#### GenerateForecastResult
```json
{
  "forecastId": "guid",
  "versionLabel": "Original",
  "endOfMonthBalance": 2847.50,
  "dayCount": 31
}
```
> File: `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/GenerateForecast/GenerateForecastCommand.cs`

#### ForecastDto
```json
{
  "forecastId": "guid",
  "budgetId": "guid",
  "versionLabel": "Original",
  "forecastType": "ORIGINAL",
  "startDay": 0,
  "startBalance": 5200.00,
  "endOfMonthBalance": 2847.50,
  "isSnapshot": false,
  "dailyEntries": [
    {
      "dayNumber": 0,
      "remainingBalance": 5200.00,
      "dailyExpenseTotal": 0.00,
      "breakdown": []
    },
    {
      "dayNumber": 1,
      "remainingBalance": 3987.10,
      "dailyExpenseTotal": 1212.90,
      "breakdown": [
        { "name": "Rent", "amount": 1200.00 },
        { "name": "Groceries (spread)", "amount": 12.90 }
      ]
    }
  ]
}
```
> File: `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/GetForecast/GetForecastQuery.cs`

#### ForecastSummaryDto
```json
{
  "forecastId": "guid",
  "versionLabel": "Original",
  "forecastType": "ORIGINAL",
  "endOfMonthBalance": 2847.50,
  "isSnapshot": false,
  "createdAt": "2026-03-01T00:00:00Z"
}
```

#### SaveSnapshotResult
```json
{ "forecastId": "guid", "isSnapshot": true }
```

#### ReforecastResult
```json
{ "forecastId": "guid", "endOfMonthBalance": 2420.00 }
```

#### ComparisonResult
```json
{
  "forecastAId": "guid",
  "forecastBId": "guid",
  "labelA": "Original",
  "labelB": "Re-forecast Mar 15",
  "endBalanceA": 2847.50,
  "endBalanceB": 2420.00,
  "totalDrift": -427.50,
  "dayVariances": [
    { "dayNumber": 0, "balanceA": 5200.00, "balanceB": 5200.00, "variance": 0.00 },
    { "dayNumber": 1, "balanceA": 4000.00, "balanceB": 3800.00, "variance": -200.00 }
  ],
  "expenseChanges": [
    { "expenseName": "Electricity", "changeType": "MODIFIED", "amountA": 85.00, "amountB": 95.00 },
    { "expenseName": "Car Repair", "changeType": "ADDED", "amountA": null, "amountB": 200.00 }
  ]
}
```
> File: `src/Modules/MonthlyBudget.ForecastEngine/Application/Features/CompareForecasts/CompareForecastsQuery.cs`

---

### Identity & Household — Requests

#### RegisterRequest
```json
{ "email": "user@example.com", "displayName": "Guilherme", "password": "securePassword" }
```

#### LoginRequest
```json
{ "email": "user@example.com", "password": "securePassword" }
```

#### RefreshTokenRequest
```json
{ "refreshToken": "jwt-refresh-token-string" }
```
> **NEW** — Not yet implemented. Required for JWT rotation (15 min access tokens).

#### CreateHouseholdRequest
```json
{ "name": "The Nogueira Home" }
```

#### InviteRequest
```json
{ "email": "partner@example.com" }
```

#### JoinRequest
```json
{ "token": "invitation-token-string" }
```

### Identity & Household — Responses

#### RegisterUserResult
```json
{ "userId": "guid" }
```

#### AuthenticateUserResult
```json
{ "accessToken": "jwt-access-token", "refreshToken": "jwt-refresh-token" }
```

#### CreateHouseholdResult
```json
{ "householdId": "guid" }
```

#### InviteMemberResult
```json
{ "invitationId": "guid" }
```

#### JoinHouseholdResult
```json
{ "householdId": "guid" }
```

#### HouseholdDto
```json
{
  "householdId": "guid",
  "name": "The Nogueira Home",
  "members": [
    { "userId": "guid", "role": "OWNER" },
    { "userId": "guid", "role": "MEMBER" }
  ]
}
```
> File: `src/Modules/MonthlyBudget.IdentityHousehold/Application/Features/GetHousehold/GetHouseholdQuery.cs`

---

## Error Response Format

```json
{
  "error": "Descriptive error message",
  "type": "ValidationError | NotFound | Conflict | Forbidden"
}
```

## Status Code Conventions

| Operation | Success Code | Notes |
|---|---|---|
| Create | 201 | With `Location` header or `CreatedAtAction` |
| Read | 200 | Returns resource or 404 |
| Update (PUT) | 204 | No body (current implementation) |
| Update (PATCH) | 204 | No body (current implementation) |
| Delete | 204 | No body |
| Domain violation | 400/422 | Returns error object |
| Not found | 404 | Returns error object |
| Unauthorized | 401 | Missing/invalid JWT |
| Wrong household | 404 | Treated as "not found" to avoid leaking info |

---

## Implementation Status Tracker

| Endpoint | Spec | Controller | Handler | Tests | Notes |
|---|---|---|---|---|---|
| `POST /budgets` | ✅ | ✅ | ✅ | ✅ | — |
| `GET /budgets/{id}` | ✅ | ✅ | ✅ | ✅ | — |
| `GET /budgets/by-month/{ym}` | ✅ | ✅ | ✅ | ✅ | — |
| `GET /budgets` (list all) | ✅ | ❌ | ❌ | ❌ | **NEW** — needed for month navigation |
| `POST /budgets/{id}/activate` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /budgets/{id}/close` | ✅ | ❌ | ❌ | ❌ | **NEW** — Domain `Close()` exists, needs controller+handler |
| `POST /budgets/{id}/rollover` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /budgets/{id}/income` | ✅ | ✅ | ✅ | ✅ | — |
| `PUT /budgets/{id}/income/{id}` | ✅ | ✅ | ✅ | ✅ | Returns 204 |
| `DELETE /budgets/{id}/income/{id}` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /budgets/{id}/expenses` | ✅ | ✅ | ✅ | ✅ | — |
| `PUT /budgets/{id}/expenses/{id}` | ✅ | ✅ | ✅ | ✅ | Returns 204 |
| `DELETE /budgets/{id}/expenses/{id}` | ✅ | ✅ | ✅ | ✅ | — |
| `PATCH .../expenses/{id}/exclusion` | ✅ | ✅ | ✅ | ✅ | Returns 204 |
| `POST /budgets/{id}/forecasts` | ✅ | ✅ | ⚠️ | ⚠️ | **FIX**: remove `startBalance` from body, derive from budget |
| `GET /budgets/{id}/forecasts` | ✅ | ✅ | ✅ | ✅ | — |
| `GET /budgets/{id}/forecasts/{id}` | ✅ | ✅ | ✅ | ✅ | — |
| `POST .../forecasts/{id}/snapshot` | ✅ | ✅ | ⚠️ | ⚠️ | **FIX**: add optional `actualBalance` to request body |
| `POST .../forecasts/{id}/reforecast` | ✅ | ✅ | ⚠️ | ⚠️ | **FIX**: add `expenseAdjustments[]` to request + handler |
| `GET .../forecasts/compare` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /auth/register` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /auth/login` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /auth/refresh` | ✅ | ❌ | ❌ | ❌ | **NEW** — needed for JWT rotation |
| `POST /households` | ✅ | ✅ | ✅ | ✅ | — |
| `GET /households/{id}` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /households/{id}/invite` | ✅ | ✅ | ✅ | ✅ | — |
| `POST /households/join` | ✅ | ✅ | ✅ | ✅ | — |
