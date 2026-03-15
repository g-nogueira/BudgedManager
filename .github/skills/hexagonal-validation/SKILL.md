---
name: hexagonal-validation
description: "Validate hexagonal architecture purity, cross-context boundaries, and domain isolation rules for the MonthlyBudget modular monolith. Use when checking architecture compliance, reviewing imports, or verifying bounded context isolation."
---

# Hexagonal Architecture Validation Skill

This skill validates that the MonthlyBudget codebase follows hexagonal architecture rules and bounded context isolation.

## Architecture Layers (Inner to Outer)

```
Domain/         ← ZERO external imports. Pure C# + SharedKernel only.
Application/    ← MediatR + FluentValidation. No EF Core, no HTTP.
Infrastructure/ ← EF Core, ASP.NET, adapters. Implements ports from inner layers.
```

## Validation Checks

### 1. Domain Layer Purity

The Domain layer must have **zero** external library imports. Search for violations:

```powershell
# Check all Domain folders for forbidden imports
Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/*/Domain/" | Select-String -Pattern "using MediatR|using Microsoft\.EntityFrameworkCore|using FluentValidation|using System\.ComponentModel\.DataAnnotations|using Microsoft\.AspNetCore" | Format-Table Path, LineNumber, Line
```

**Allowed imports in Domain:**
- `System.*` (standard .NET BCL)
- `MonthlyBudget.SharedKernel.*` (HouseholdId, UserId, IDomainEvent, DomainEventBase)
- Same-context `Domain.*` namespaces

**Forbidden imports in Domain:**
- `MediatR` (belongs in Application)
- `Microsoft.EntityFrameworkCore` (belongs in Infrastructure)
- `FluentValidation` (belongs in Application)
- `System.ComponentModel.DataAnnotations` (use domain validation instead)
- `Microsoft.AspNetCore.*` (belongs in Infrastructure)
- Any other NuGet package

### 2. Application Layer Constraints

The Application layer can use MediatR and FluentValidation but must NOT use EF Core or HTTP:

```powershell
# Check Application folders for forbidden imports
Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/*/Application/" | Select-String -Pattern "using Microsoft\.EntityFrameworkCore|using Microsoft\.AspNetCore|using System\.Net\.Http" | Format-Table Path, LineNumber, Line
```

### 3. Cross-Context Boundary Isolation

Bounded contexts must **never** reference each other's Domain or Application namespaces directly:

```powershell
# Budget Management must not import Forecast or Identity domain/application
Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/MonthlyBudget.BudgetManagement/" | Select-String -Pattern "using MonthlyBudget\.ForecastEngine|using MonthlyBudget\.IdentityHousehold" | Format-Table Path, LineNumber, Line

# Forecast Engine must not import Budget or Identity domain/application
Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/MonthlyBudget.ForecastEngine/" | Select-String -Pattern "using MonthlyBudget\.BudgetManagement|using MonthlyBudget\.IdentityHousehold" | Format-Table Path, LineNumber, Line

# Identity & Household must not import Budget or Forecast
Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/MonthlyBudget.IdentityHousehold/" | Select-String -Pattern "using MonthlyBudget\.BudgetManagement|using MonthlyBudget\.ForecastEngine" | Format-Table Path, LineNumber, Line
```

### 4. Port/Adapter Pattern Compliance

- **Ports** (interfaces) must live in:
  - `Domain/Repositories/` (repository interfaces)
  - `Application/Ports/` (cross-cutting ports)
- **Adapters** (implementations) must live in:
  - `src/MonthlyBudget.Infrastructure/Repositories/` (repository impls)
  - `src/MonthlyBudget.Infrastructure/Acl/` (ACL adapters)
  - `src/MonthlyBudget.Infrastructure/EventHandlers/` (event handlers)

```powershell
# Verify no repository implementations inside module folders
Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/" | Select-String -Pattern "class\s+\w+Repository\s*:" | Format-Table Path, LineNumber, Line
```

### 5. Cross-Context Communication — Events Only

Budget Management ↔ Forecast Engine communicates **only** via:
1. MediatR `INotification` events (published by `IBudgetEventPublisher` → `MediatRBudgetEventPublisher`)
2. `IBudgetDataPort` → `BudgetManagementAcl` (Forecast Engine reading budget data)

**Never** read across DB schemas directly. The ACL translates `MonthlyBudget` → `BudgetData` record.

### 6. EF Core Configuration Location

All EF Core entity configurations must be in `src/MonthlyBudget.Infrastructure/Database/Configurations/`, NOT in module-level folders:

```powershell
# Check for misplaced EF configs inside module folders
Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/*/Infrastructure/Persistence/Configurations/" | Format-Table FullName
```

### 7. Schema Isolation

Each bounded context uses a separate PostgreSQL schema:
- Budget Management → `budget` schema
- Forecast Engine → `forecast` schema
- Identity & Household → `identity` schema

Cross-context references use UUIDs with **no database-level foreign keys**.

### 8. DI Registration Completeness

All services must be registered in `src/MonthlyBudget.Infrastructure/ServiceCollectionExtensions.cs` → `AddInfrastructure()`:

```powershell
# List all port interfaces across modules
Get-ChildItem -Recurse -Filter "I*.cs" -Path "src/Modules/*/Domain/Repositories/","src/Modules/*/Application/Ports/" | ForEach-Object { $_.FullName }

# Check what's registered
Get-Content "src/MonthlyBudget.Infrastructure/ServiceCollectionExtensions.cs" | Select-String -Pattern "AddScoped|AddTransient|AddSingleton"
```

## Domain Invariant Enforcement Location

Invariants must be enforced **in the Aggregate Root**, not in handlers or controllers.

**Always refer to `docs/arch/domain-invariants.md` for the canonical list of invariants.** Do not rely on this summary — the architecture extract is the source of truth.

Quick reference (see architecture extract for full details):

| Invariant | Aggregate | Rule |
|---|---|---|
| INV-B1 | MonthlyBudget | `activate()` throws if no income sources |
| INV-B2/B3/B4 | MonthlyBudget | `addExpense()` spread/day validation |
| INV-B6 | MonthlyBudget | Status: DRAFT → ACTIVE → CLOSED only |
| INV-B8 | MonthlyBudget | Mutations rejected unless ACTIVE |
| INV-F2 | ForecastVersion | REFORECAST requires parentForecastId |
| INV-F3 | ForecastVersion | ORIGINAL: startDay=0, startBalance=totalIncome |
| INV-F4 | ForecastVersion | Snapshots are immutable |
| INV-H1 | Household | Max 2 members |
| INV-H2 | Household | Exactly one OWNER, never removable |

## Quick Validation Checklist

Run all checks in sequence:

```powershell
Write-Host "=== Domain Purity ===" -ForegroundColor Cyan
$domainViolations = Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/*/Domain/" | Select-String -Pattern "using MediatR|using Microsoft\.EntityFrameworkCore|using FluentValidation|using System\.ComponentModel\.DataAnnotations|using Microsoft\.AspNetCore"
if ($domainViolations) { $domainViolations | Format-Table Path, LineNumber, Line } else { Write-Host "PASS" -ForegroundColor Green }

Write-Host "`n=== Application Layer ===" -ForegroundColor Cyan
$appViolations = Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/*/Application/" | Select-String -Pattern "using Microsoft\.EntityFrameworkCore|using Microsoft\.AspNetCore|using System\.Net\.Http"
if ($appViolations) { $appViolations | Format-Table Path, LineNumber, Line } else { Write-Host "PASS" -ForegroundColor Green }

Write-Host "`n=== Cross-Context Boundaries ===" -ForegroundColor Cyan
$crossViolations = @()
$crossViolations += Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/MonthlyBudget.BudgetManagement/" | Select-String -Pattern "using MonthlyBudget\.ForecastEngine|using MonthlyBudget\.IdentityHousehold"
$crossViolations += Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/MonthlyBudget.ForecastEngine/" | Select-String -Pattern "using MonthlyBudget\.BudgetManagement|using MonthlyBudget\.IdentityHousehold"
$crossViolations += Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/MonthlyBudget.IdentityHousehold/" | Select-String -Pattern "using MonthlyBudget\.BudgetManagement|using MonthlyBudget\.ForecastEngine"
if ($crossViolations) { $crossViolations | Format-Table Path, LineNumber, Line } else { Write-Host "PASS" -ForegroundColor Green }

Write-Host "`n=== Misplaced EF Configs ===" -ForegroundColor Cyan
$misplaced = Get-ChildItem -Recurse -Filter "*.cs" -Path "src/Modules/*/Infrastructure/Persistence/Configurations/" -ErrorAction SilentlyContinue
if ($misplaced) { $misplaced | Format-Table FullName } else { Write-Host "PASS" -ForegroundColor Green }
```
