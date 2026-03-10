---
name: api-exercise
description: "Validate MonthlyBudget API endpoints against architecture contracts. Use when testing API responses, exercising endpoints after implementation, or performing Phase 6 API validation."
---

# API Exercise & Validation Skill

This skill provides scripts and procedures for validating MonthlyBudget API endpoints against the architecture contract spec.

## Prerequisites

```powershell
# 1. Start PostgreSQL
docker compose up -d postgres

# 2. Build the solution
dotnet build

# 3. Run all tests
dotnet test

# 4. Start the API (background)
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run","--project","src/MonthlyBudget.Api","--urls","http://localhost:5000"
Start-Sleep -Seconds 5
```

## Authentication Setup

Every endpoint requires JWT authentication. Use this flow to get a valid token:

```powershell
# Register a test user
$registerBody = @{
    email       = "test@example.com"
    displayName = "Test User"
    password    = "TestPass123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/register" `
    -Method POST -ContentType "application/json" -Body $registerBody

# Login to get access token
$loginBody = @{
    email    = "test@example.com"
    password = "TestPass123!"
} | ConvertTo-Json

$loginResult = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/login" `
    -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResult.accessToken

# Create a Household (needed for householdId JWT claim)
$headers = @{ Authorization = "Bearer $token" }
$householdBody = @{ name = "Test Household" } | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/households" `
    -Method POST -ContentType "application/json" -Body $householdBody -Headers $headers

# Re-login to get token with householdId claim embedded
$loginResult = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/login" `
    -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResult.accessToken
$headers = @{ Authorization = "Bearer $token" }
```

## Budget Management Endpoints

### Create Budget → 201
```powershell
$body = @{ yearMonth = "2026-03" } | ConvertTo-Json
$budget = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets" `
    -Method POST -ContentType "application/json" -Body $body -Headers $headers
# Expected: { budgetId: "uuid", status: "DRAFT" }
$budgetId = $budget.budgetId
```

### Get Budget → 200
```powershell
$budget = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets/$budgetId" `
    -Method GET -Headers $headers
# Expected: { budgetId, householdId, yearMonth, status, incomeSources: [], expenses: [] }
```

### Add Income → 201
```powershell
$body = @{ label = "Salary"; amount = 3000.00 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets/$budgetId/incomes" `
    -Method POST -ContentType "application/json" -Body $body -Headers $headers
```

### Add Expense → 201
```powershell
# Fixed expense (non-spread, has dayOfMonth)
$body = @{ label = "Rent"; amount = 1200.00; category = "FIXED"; isSpread = $false; dayOfMonth = 1 } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets/$budgetId/expenses" `
    -Method POST -ContentType "application/json" -Body $body -Headers $headers

# Spread expense (no dayOfMonth)
$body = @{ label = "Groceries"; amount = 400.00; category = "VARIABLE"; isSpread = $true } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets/$budgetId/expenses" `
    -Method POST -ContentType "application/json" -Body $body -Headers $headers
```

### Activate Budget → 200
```powershell
$result = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets/$budgetId/activate" `
    -Method POST -Headers $headers
# Expected: { budgetId, status: "ACTIVE" }
```

### Toggle Expense Exclusion → 200
```powershell
$expenseId = $budget.expenses[0].expenseId
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets/$budgetId/expenses/$expenseId/exclusion" `
    -Method PATCH -Headers $headers
```

### Rollover → 201
```powershell
$rollover = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/budgets/$budgetId/rollover" `
    -Method POST -Headers $headers
# Expected: { newBudgetId: "uuid" }
# Only FIXED and SUBSCRIPTION expenses carry forward, VARIABLE dropped
```

## Forecast Engine Endpoints

### Generate Forecast → 201
```powershell
$body = @{ budgetId = $budgetId } | ConvertTo-Json
$forecast = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/forecasts/generate" `
    -Method POST -ContentType "application/json" -Body $body -Headers $headers
# Expected: { forecastId, dailyEntries: [...], endOfMonthBalance }
$forecastId = $forecast.forecastId
```

### Get Forecast → 200
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/forecasts/$forecastId" `
    -Method GET -Headers $headers
```

### List Forecasts → 200
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/forecasts?budgetId=$budgetId" `
    -Method GET -Headers $headers
```

### Save Snapshot → 201
```powershell
$result = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/forecasts/$forecastId/snapshot" `
    -Method POST -Headers $headers
# Expected: { forecastId, isSnapshot: true }
```

### Create Re-forecast → 201
```powershell
$body = @{ parentForecastId = $forecastId; budgetId = $budgetId } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/forecasts/reforecast" `
    -Method POST -ContentType "application/json" -Body $body -Headers $headers
```

### Compare Forecasts (Drift Analysis) → 200
```powershell
$versionA = $forecastId
$versionB = "<new-forecast-id>"
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/forecasts/compare?versionA=$versionA&versionB=$versionB" `
    -Method GET -Headers $headers
# Expected: { forecastAId, forecastBId, totalDrift, dayVariances: [...], expenseChanges: [...] }
```

## Validation Checklist

For each endpoint exercised, verify:

| Check | Description |
|---|---|
| Status code | Matches spec (201 for creates, 200 for reads/updates, 204 for deletes) |
| Response body | Contains all expected fields |
| Data types | UUIDs are valid, decimals have 2 places, enums are strings |
| Error cases | Invalid input returns proper error object with `error` and `type` fields |
| Household scope | Users can only access their own household's data |

## Error Response Format

```json
{
  "error": "Descriptive error message",
  "type": "ValidationError | NotFound | Conflict | Forbidden"
}
```

## Cleanup

```powershell
# Stop the API (find and kill the process)
Get-Process -Name "dotnet" | Where-Object { $_.CommandLine -like "*MonthlyBudget.Api*" } | Stop-Process -Force
```
