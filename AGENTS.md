# AGENTS.md — MonthlyBudget Codebase Guide

> Authoritative reference for AI coding agents. Architecture source: `docs/MonthlyBudget_Architecture.md`.

---

## Architecture Overview

**Modular Monolith** (ADR-001) — single ASP.NET Core process with three strictly-isolated bounded contexts as class library projects:

| Module | Path | Role |
|---|---|---|
| Budget Management | `src/Modules/MonthlyBudget.BudgetManagement/` | Core: budgets, income, expenses, rollover |
| Forecast Engine | `src/Modules/MonthlyBudget.ForecastEngine/` | Core: simulation, snapshots, re-forecast, drift |
| Identity & Household | `src/Modules/MonthlyBudget.IdentityHousehold/` | Supporting: users, JWT auth, household (max 2 members) |
| Shared Kernel | `src/MonthlyBudget.SharedKernel/` | `HouseholdId`, `UserId`, `IDomainEvent` only |
| Cross-cutting Infra | `src/MonthlyBudget.Infrastructure/` | `AppDbContext`, `HouseholdScopeMiddleware`, EF Migrations |
| API Host | `src/MonthlyBudget.Api/` | `Program.cs`, `appsettings.json` |
| Frontend | `frontend/` | SvelteKit + TypeScript + Chart.js |

Contexts communicate **only** via MediatR `INotification` events (in-process event bus). Direct cross-context method calls or shared domain models are forbidden.

---

## Hexagonal Architecture Rules

Every bounded context follows the same three-layer structure — violations break the architecture:

```
Domain/         ← No external library imports. Pure C# only.
Application/    ← MediatR handlers + FluentValidation. No EF Core, no HTTP.
Infrastructure/ ← EF Core, controllers, adapters, email, JWT. Implements ports.
```

- **Domain** interfaces (ports) live in `Domain/Repositories/` and `Application/Ports/`
- **Infrastructure** implementations (adapters) live in `Infrastructure/`
- The ACL between Forecast Engine and Budget Management is `BudgetContextAclAdapter.cs` implementing `IBudgetDataQueryPort` — never read across DB schemas directly

---

## Development Workflow (TDD — Mandatory)

Follow strict **Red → Green → Refactor** with a git commit after each phase:

```powershell
# Run all tests
dotnet test

# Run tests for one context
dotnet test tests/MonthlyBudget.BudgetManagement.Tests/

# Build solution
dotnet build

# Add EF Core migration (run from solution root)
dotnet ef migrations add <MigrationName> --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api
dotnet ef database update --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api

# Frontend
cd frontend ; npm install ; npm run dev
```

**Git discipline:** Commit after every phase — failing tests (Red), domain implementation (Green), application layer, infrastructure, and refactor. Use descriptive messages referencing the bounded context (e.g., `feat(budget): implement MonthlyBudget aggregate with INV-B1 through INV-B8`).

---

## Critical Domain Invariants

These must be enforced in the Aggregate Root, not in handlers or controllers:

**MonthlyBudget:**
- `INV-B2/B3/B4`: `addExpense()` — if `isSpread=true` then `dayOfMonth` must be `null`; if `isSpread=false` then `dayOfMonth` must be set and `≤ lastDayOfMonth(yearMonth)`
- `INV-B1`: `activate()` throws `InsufficientIncomeException` if `incomeSources` is empty
- `INV-B6`: Status is unidirectional: `DRAFT → ACTIVE → CLOSED` only
- `INV-B8`: Mutations (add/update/remove income or expense) are rejected unless `status == ACTIVE` (DRAFT allows initial setup)

**ForecastVersion:**
- `INV-F3`: ORIGINAL forecasts always have `startDay = 0`, `startBalance = total income`
- `INV-F4`: Snapshots are immutable — all mutations throw `SnapshotImmutableException`
- `INV-F2`: REFORECAST type requires a non-null `parentForecastId`
- `AutoSnapshotOnReforecast` policy: auto-save parent as snapshot before creating a re-forecast if not already snapshotted

**Household:**
- `INV-H1`: Max 2 members; `INV-H2`: exactly one OWNER, never removable

---

## Cross-Context Communication Pattern

Budget Management → Forecast Engine uses **events only**:

1. Budget Management publishes (e.g., `ExpenseUpdated`) via `IBudgetEventPublisher` → `MediatRBudgetEventPublisher`
2. Forecast Engine's `BudgetEventHandler` (in `Infrastructure/Events/`) handles the notification and marks affected forecasts stale (`ForecastStaleMarked`) — it does **not** auto-regenerate
3. Forecast Engine reads budget data via `IBudgetDataQueryPort` → `BudgetContextAclAdapter` which translates `MonthlyBudget` → `BudgetDataDto` (no shared domain models)

---

## Persistence Conventions

- **PostgreSQL** with **separate schemas per bounded context** (logical isolation, single instance)
- EF Core entity configurations in `Infrastructure/Persistence/Configurations/`
- Cross-context references use UUIDs with **no database-level foreign keys** (application-level integrity)
- `householdId` is the universal tenant identifier — every query must be scoped by it via `HouseholdScopeMiddleware`
- Currency stored as `DECIMAL(12,2)` — always EUR, never float

---

## Authentication & Authorization

- JWT access tokens (15 min) + refresh token rotation — tokens embed both `userId` and `householdId` claims
- All API endpoints enforce household scope: users may only access their own household's data
- Password hashing: `Argon2PasswordHasher` via `Konscious.Security.Cryptography` (never bcrypt)
- JWT wiring: `JwtAuthHandler` + `JwtOptions` in `Infrastructure/Auth/`

---

## Key Rollover Behaviour

`POST /api/v1/budgets/{budgetId}/rollover` carries forward only `FIXED` and `SUBSCRIPTION` expenses — `VARIABLE` expenses are dropped. Exclusion flags are preserved. The new budget is created in `DRAFT` status.

---

## Tech Stack (exact versions must match architecture spec)

| Layer | Technology |
|---|---|
| Backend | C# / ASP.NET Core, MediatR, FluentValidation, EF Core, PostgreSQL |
| Auth | JWT (`System.IdentityModel.Tokens.Jwt`), Argon2id (`Konscious.Security.Cryptography`) |
| Frontend | SvelteKit, TypeScript, Chart.js |
| Tests | xUnit (unit), separate `Integration.Tests` project |

> ⚠️ Do not introduce libraries not listed above without an ADR. See `docs/MonthlyBudget_Architecture.md` §4.7.

