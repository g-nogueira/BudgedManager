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
| Shared Kernel | `src/MonthlyBudget.SharedKernel/` | `HouseholdId`, `UserId`, `IDomainEvent`, `DomainEventBase` |
| Cross-cutting Infra | `src/MonthlyBudget.Infrastructure/` | `AppDbContext`, middlewares, EF configurations, repositories, ACL, event handlers |
| API Host | `src/MonthlyBudget.Api/` | `Program.cs`, `appsettings.json`, global exception handler |
| Frontend | `frontend/` | SvelteKit + TypeScript + Chart.js (**not yet implemented**) |

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
- **Infrastructure** implementations (adapters) live in the cross-cutting `src/MonthlyBudget.Infrastructure/` project (not inside each module)
- EF Core entity configurations are centralized in `src/MonthlyBudget.Infrastructure/Database/Configurations/` (module-level `Infrastructure/Persistence/Configurations/` folders exist but are empty)
- Repository implementations: `src/MonthlyBudget.Infrastructure/Repositories/` (`PostgresBudgetRepository`, `PostgresForecastRepository`, `PostgresIdentityRepositories`)
- The ACL between Forecast Engine and Budget Management is `BudgetManagementAcl.cs` (in `src/MonthlyBudget.Infrastructure/Acl/`) implementing `IBudgetDataPort` — never read across DB schemas directly
- All DI wiring is centralized in `src/MonthlyBudget.Infrastructure/ServiceCollectionExtensions.cs` → `AddInfrastructure()`

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

# Start PostgreSQL (local dev)
docker compose up -d postgres

# Run the API
dotnet run --project src/MonthlyBudget.Api
```

> **Note:** Integration tests (`tests/MonthlyBudget.Integration.Tests/`) use **Testcontainers.PostgreSql** — Docker must be running but no manual DB setup is needed. The fixture (`IntegrationTestFixture.cs`) spins up a disposable PostgreSQL container per test collection.

**Git discipline:** Commit after every phase — failing tests (Red), domain implementation (Green), application layer, infrastructure, and refactor. Use descriptive messages referencing the bounded context (e.g., `feat(budget): implement MonthlyBudget aggregate with INV-B1 through INV-B8`).

---

## Custom Agent Workflows

The repository now uses two distinct custom-agent loops under `.github/agents/`:

### 1. Product Discovery & UI Alignment

`Product Manager → UI Designer → Software Architect → Product Manager`

- **Product Manager** writes PRDs and user stories to `docs/product/<feature>-prd.md`
- **UI Designer** uses Google Stitch MCP to generate full UI screens and records them in `docs/product/<feature>-screens.md`
- **Software Architect** validates feasibility, API/data-model alignment, and bounded-context fit before implementation starts

These agents must use `docs/product/` artifacts as the source of truth for product/design decisions rather than chat-only context.

### 3. Project Startup (Architecture → Issues)

`Product Manager → UI Designer → Software Architect → Planner (creates issues via github-issues skill) → Implementor`

Full pipeline from product definition to trackable implementation work:
1. **Product Manager** writes PRD → **UI Designer** generates screens → **Software Architect** creates architecture docs
2. **Planner** reads PRD user stories + arch docs → creates one GitHub issue per user story on Project #6 (using the `github-issues` skill)
3. **Planner** picks up an issue, gathers context via the `task-context` skill, and plans implementation

### 4. Post-Implementation Design Review (Gaps → Issues)

`Software Architect → creates issues directly (using github-issues skill) → Planner → Implementor`

Used when UI designs are created after backend implementation has started:
1. **Software Architect** reviews designs vs. codebase → produces/updates `docs/arch/design-gaps.md`
2. **Software Architect** reads `design-gaps.md` → creates one issue per GAP on Project #6 (using the `github-issues` skill)
3. **Planner** picks up a gap issue and feeds it into the delivery pipeline

### 2. Delivery Pipeline

`Backend/Frontend Planner (gathers context via task-context skill) → Backend/Frontend Implementor → Backend/Frontend Reviewer`

**PR Review & Fix (Additive Model):**
`Reviewer → Implementor (addresses feedback via address-pr-feedback skill, consults Planner sub-agent if needed)`
The Implementor is the primary agent for addressing PR feedback. For straightforward code fixes, the Implementor handles them directly. For design-level questions, the Implementor invokes the Planner as a sub-agent. The Reviewer then runs the next additive review round.

- Memory files live in `.github/agents/memory/active/` (Tier 1 — Working Memory)
- Naming convention:
	- `task-context-<issue-number>.md`
	- `plan-<issue-number>.md`
	- `implementation-<issue-number>.md`
	- `code-reviewer-<issue-number>.md`
- Completed memory files are archived to `.github/agents/memory/archive/` when an issue is closed/merged

Use the product/design loop first when the team needs to define or align the UX before implementation. Use the delivery pipeline when executing scoped engineering work from an issue or review. Use the project startup flow (3) when going from zero to implementation. Use the design review flow (4) when designs arrive after implementation has started.

### Team Activity Log

All agents share a single append-only log at `.github/agents/activity-log.md`. This is the team's "standup board" — it records cross-team events so every agent has situational awareness of what's been happening.

- **Every agent reads the log on startup** (quick scan, not deep read)
- **Agents write after cross-team events:** creating architecture artifacts, opening GitHub issues, opening PRs, completing reviews, writing plans
- **Entry format:** Date + agent name + 1–2 sentence summary + list of artifacts (files, issues, PRs)
- **Tone:** Brief, like a 30-second standup update — point to artifacts, don't duplicate content
- **Rotation:** When the log exceeds ~100 entries, summarize oldest into a sprint summary and move raw entries to `archive/`

### Memory Architecture — 3-Tier Model

| Tier | Location | Purpose | Lifecycle |
|---|---|---|---|
| **1 — Active** | `.github/agents/memory/active/` | Current sprint’s working files (issue contexts, plans, reviews) | Created when work starts; archived when issue closes |
| **2 — Distilled** | `.github/agents/memory/knowledge.md` | Compressed rules, decisions, and patterns from completed work | Updated after each issue via `distill-knowledge` skill; atomic facts only |
| **3 — Archive** | `.github/agents/memory/archive/` | Raw completed memory files for auditing | Moved from active/ on issue close; never loaded on startup |

The MCP Knowledge Graph (`@modelcontextprotocol/server-memory` configured in `.vscode/mcp.json`) stores structured entity-relationship data for codebase topology and cross-session decisions.

### Instruction Layering

Agent instructions are organized in a layered system to minimize duplication:

| Layer | File | Scope | Always Loaded |
|---|---|---|---|
| Global | `.github/copilot-instructions.md` | All agents: grounding rules, git discipline, arch constraints, memory model | Yes |
| Backend | `.github/instructions/backend.instructions.md` | C#/.NET files: hexagonal rules, build commands, persistence conventions | When editing `src/**/*.cs` or `tests/**/*.cs` |
| Frontend | `.github/instructions/frontend.instructions.md` | SvelteKit/TS files: TypeScript rules, component patterns, build commands | When editing `frontend/**` |
| Agent | `.github/agents/<name>.agent.md` | Per-agent: role-specific workflow, grounding rules, handoffs | When agent is active |

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
2. Forecast Engine's `BudgetEventHandler` (in `src/MonthlyBudget.Infrastructure/EventHandlers/`) handles the notification and marks affected forecasts stale (`ForecastStaleMarked`) — it does **not** auto-regenerate
3. Forecast Engine reads budget data via `IBudgetDataPort` → `BudgetManagementAcl` which translates `MonthlyBudget` → `BudgetData` record (no shared domain models)

---

## Persistence Conventions

- **PostgreSQL** with **separate schemas per bounded context** (logical isolation, single instance)
- EF Core entity configurations in `src/MonthlyBudget.Infrastructure/Database/Configurations/` — schemas: `budget`, `forecast`, `identity`
- Cross-context references use UUIDs with **no database-level foreign keys** (application-level integrity)
- `householdId` is the universal tenant identifier — every query must be scoped by it via `HouseholdScopeMiddleware`
- Currency stored as `DECIMAL(12,2)` — always EUR, never float

---

## Authentication & Authorization

- JWT access tokens (15 min) + refresh token rotation — tokens embed both `userId` and `householdId` claims
- All API endpoints enforce household scope: users may only access their own household's data
- Password hashing: `BCryptPasswordHasher` via `BCrypt.Net-Next` (in `IdentityHousehold/Infrastructure/Auth/`)
- JWT wiring: `JwtTokenService` implementing `ITokenService` (in `IdentityHousehold/Infrastructure/Auth/`); JWT bearer config in `Program.cs`

---

## Key Rollover Behaviour

`POST /api/v1/budgets/{budgetId}/rollover` carries forward only `FIXED` and `SUBSCRIPTION` expenses — `VARIABLE` expenses are dropped. Exclusion flags are preserved. The new budget is created in `DRAFT` status.

---

## Tech Stack (exact versions must match architecture spec)

| Layer | Technology |
|---|---|
| Backend | C# / ASP.NET Core, MediatR, FluentValidation, EF Core, PostgreSQL |
| Auth | JWT (`System.IdentityModel.Tokens.Jwt`), BCrypt (`BCrypt.Net-Next`) |
| Frontend | SvelteKit, TypeScript, Chart.js |
| Tests | xUnit (unit), separate `Integration.Tests` project |

> ⚠️ Do not introduce libraries not listed above without an ADR. See `docs/MonthlyBudget_Architecture.md` §4.7.

