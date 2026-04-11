---
name: 'Backend .NET Conventions'
description: 'C#/.NET specific grounding rules and patterns for the MonthlyBudget modular monolith'
applyTo: 'src/**/*.cs,tests/**/*.cs'
---

# Backend .NET Conventions

## Grounding Rules (Backend-Specific)

Before writing any C# code:

1. **Before writing ANY method signature:** Read the existing file or patterns file to match existing patterns
2. **Before writing ANY `using` statement:** Verify the namespace exists by searching for it
3. **Before referencing ANY repository interface:** Grep for its exact declaration in `Domain/Repositories/`
4. **When writing test names:** Grep existing tests in the same project to match `Should_X_When_Y` naming
5. **When adding DI registrations:** Read `ServiceCollectionExtensions.cs` first to match grouping style
6. **When writing EF configs:** Read an existing config from `Database/Configurations/` first

## Hexagonal Layer Rules

| Layer | Location | Allowed Imports | Forbidden |
|---|---|---|---|
| Domain | `src/Modules/<Context>/Domain/` | `System.*`, `MonthlyBudget.SharedKernel.*` | MediatR, EF Core, FluentValidation, ASP.NET |
| Application | `src/Modules/<Context>/Application/` | Domain, MediatR, FluentValidation | EF Core, ASP.NET, HTTP |
| Infrastructure | `src/MonthlyBudget.Infrastructure/` | Everything | — |

## Build & Test Commands

```powershell
dotnet build                                    # Build solution
dotnet test                                     # Run all tests
dotnet test tests/<Context>.Tests/              # Run context-specific tests
dotnet ef migrations add <Name> --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api
```

## Persistence Conventions

- PostgreSQL with separate schemas per bounded context (`budget`, `forecast`, `identity`)
- Cross-context references use UUIDs — no database-level foreign keys
- Currency: `DECIMAL(12,2)`, always EUR
- `householdId` is the universal tenant identifier
- EF configs: `src/MonthlyBudget.Infrastructure/Database/Configurations/`

## Cross-Context Communication

- Only via MediatR `INotification` events (in-process event bus)
- Never direct method calls or shared domain models
- Budget → Forecast: via `IBudgetDataPort` → `BudgetManagementAcl` (anti-corruption layer)
