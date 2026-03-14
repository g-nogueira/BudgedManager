# API Contracts Reference

> Extracted from `docs/MonthlyBudget_Architecture.md` §2.5.
> All endpoints require JWT authentication (except auth endpoints marked `[AllowAnonymous]`).
> HouseholdId is extracted from the JWT `householdId` claim, never from request body.

## Budget Management API

| Method | Endpoint | Request Body | Response | Status |
|---|---|---|---|---|
| `POST` | `/api/v1/budgets` | `{ householdId, yearMonth }` | `{ budgetId, status }` | 201 |
| `GET` | `/api/v1/budgets?householdId={id}&month={YYYY-MM}` | — | `{ budget }` | 200 |
| `GET` | `/api/v1/budgets/{budgetId}` | — | `{ budget }` | 200 |
| `POST` | `/api/v1/budgets/{budgetId}/incomes` | `{ name, amount }` | `{ incomeId }` | 201 |
| `PUT` | `/api/v1/budgets/{budgetId}/incomes/{incomeId}` | `{ name, amount }` | `{ income }` | 200 |
| `DELETE` | `/api/v1/budgets/{budgetId}/incomes/{incomeId}` | — | — | 204 |
| `POST` | `/api/v1/budgets/{budgetId}/expenses` | `{ name, category, dayOfMonth, isSpread, amount }` | `{ expenseId }` | 201 |
| `PUT` | `/api/v1/budgets/{budgetId}/expenses/{expenseId}` | `{ name, category, dayOfMonth, isSpread, amount }` | `{ expense }` | 200 |
| `DELETE` | `/api/v1/budgets/{budgetId}/expenses/{expenseId}` | — | — | 204 |
| `PATCH` | `/api/v1/budgets/{budgetId}/expenses/{expenseId}/exclusion` | `{ isExcluded }` | `{ expense }` | 200 |
| `POST` | `/api/v1/budgets/{budgetId}/rollover` | `{ targetYearMonth }` | `{ newBudgetId }` | 201 |

## Forecast Engine API

| Method | Endpoint | Request Body | Response | Status |
|---|---|---|---|---|
| `POST` | `/api/v1/forecasts/generate` | `{ budgetId }` | `{ forecastId, dailyEntries, endOfMonthBalance }` | 201 |
| `GET` | `/api/v1/forecasts?budgetId={id}` | — | `{ forecasts[] }` | 200 |
| `GET` | `/api/v1/forecasts/{forecastId}` | — | `{ forecast }` | 200 |
| `POST` | `/api/v1/forecasts/{forecastId}/snapshot` | `{ actualBalance? }` | `{ forecastId, isSnapshot: true }` | 201 |
| `POST` | `/api/v1/forecasts/reforecast` | `{ parentForecastId, actualBalance, reforecastDate, expenseAdjustments[] }` | `{ forecastId, dailyEntries }` | 201 |
| `GET` | `/api/v1/forecasts/compare?versionA={id}&versionB={id}` | — | `{ comparison }` | 200 |

## Identity & Household API

| Method | Endpoint | Request Body | Response | Status |
|---|---|---|---|---|
| `POST` | `/api/v1/auth/register` | `{ email, displayName, password }` | `{ userId }` | 201 |
| `POST` | `/api/v1/auth/login` | `{ email, password }` | `{ accessToken, refreshToken }` | 200 |
| `POST` | `/api/v1/households` | `{ name }` | `{ householdId }` | 201 |
| `GET` | `/api/v1/households/{householdId}` | — | `{ household }` | 200 |
| `POST` | `/api/v1/households/{householdId}/invite` | `{ email }` | `{ invitationId }` | 201 |
| `POST` | `/api/v1/households/join` | `{ token }` | `{ householdId }` | 200 |

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
| Update | 200 | Returns updated resource |
| Delete | 204 | No body |
| Domain violation | 400/422 | Returns error object |
| Not found | 404 | Returns error object |
| Unauthorized | 401 | Missing/invalid JWT |
| Wrong household | 404 | Treated as "not found" to avoid leaking info |
