---
name: dotnet-tdd
description: "Build, test, and manage EF Core migrations for the MonthlyBudget .NET solution. Use when running tests, building the solution, adding migrations, or following the TDD Red-Green-Refactor loop."
---

# .NET TDD Workflow Skill

This skill provides the commands and procedures for building, testing, and managing migrations in the MonthlyBudget .NET modular monolith.

## Solution Structure

| Project | Path |
|---|---|
| API Host | `src/MonthlyBudget.Api/` |
| Budget Management | `src/Modules/MonthlyBudget.BudgetManagement/` |
| Forecast Engine | `src/Modules/MonthlyBudget.ForecastEngine/` |
| Identity & Household | `src/Modules/MonthlyBudget.IdentityHousehold/` |
| Cross-cutting Infra | `src/MonthlyBudget.Infrastructure/` |
| Shared Kernel | `src/MonthlyBudget.SharedKernel/` |
| Budget Tests | `tests/MonthlyBudget.BudgetManagement.Tests/` |
| Forecast Tests | `tests/MonthlyBudget.ForecastEngine.Tests/` |
| Identity Tests | `tests/MonthlyBudget.IdentityHousehold.Tests/` |
| Integration Tests | `tests/MonthlyBudget.Integration.Tests/` |

## Build Commands

```powershell
# Build entire solution
dotnet build

# Build a specific project
dotnet build src/Modules/MonthlyBudget.BudgetManagement/
```

## Test Commands

```powershell
# Run ALL tests
dotnet test

# Run tests for a specific bounded context
dotnet test tests/MonthlyBudget.BudgetManagement.Tests/
dotnet test tests/MonthlyBudget.ForecastEngine.Tests/
dotnet test tests/MonthlyBudget.IdentityHousehold.Tests/

# Run integration tests (requires Docker for Testcontainers PostgreSQL)
dotnet test tests/MonthlyBudget.Integration.Tests/

# Run a specific test by name filter
dotnet test --filter "FullyQualifiedName~Activate_WithIncomeSources"

# Run with verbose output
dotnet test --verbosity normal
```

## EF Core Migration Commands

All migrations live in `src/MonthlyBudget.Infrastructure/Migrations/`. Always run from the solution root:

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api

# Apply migrations to database
dotnet ef database update --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api
```

## TDD Red-Green-Refactor Loop

### Red Phase: Write Failing Tests
1. Create test file in `tests/MonthlyBudget.<Context>.Tests/Domain/`
2. Write tests targeting the expected public API (methods may not exist yet)
3. Run `dotnet test` — confirm all new tests **fail**
4. Commit: `git add -A && git commit -m "test(<context>): add failing tests for #<issue>"`

### Green Phase: Implement Code
1. Implement Domain layer first (`Domain/Entities/`, `Domain/Events/`, `Domain/Exceptions/`)
2. Then Application layer (`Application/Features/<Name>/`)
3. Then Infrastructure layer (controllers, EF configs, DI wiring)
4. Run `dotnet test` — confirm all tests **pass**
5. Commit: `git add -A && git commit -m "feat(<context>): implement <description> for #<issue>"`

### Refactor Phase: Clean Up
1. Run `dotnet test` — verify Green state
2. Refactor: naming, duplication, dead code, sealed modifiers
3. Run `dotnet test` — verify still Green
4. Commit: `git add -A && git commit -m "refactor(<context>): clean up <description> for #<issue>"`

## Test Naming Convention

```
MethodUnderTest_Scenario_ExpectedBehavior
```

Examples:
- `Activate_WithIncomeSources_TransitionsToActive`
- `AddExpense_SpreadWithDaySet_ThrowsInvalidExpenseDay_INV_B3`
- `MarkAsSnapshot_NonSnapshotForecast_SetsIsSnapshotTrue`

## Test File Template

```csharp
using Xunit;

namespace MonthlyBudget.<Context>.Tests.Domain;

public class <AggregateRoot>Tests
{
    private static readonly Guid HouseholdId = Guid.NewGuid();

    [Fact]
    public void MethodUnderTest_Scenario_ExpectedBehavior()
    {
        // Arrange
        var entity = CreateTestEntity();

        // Act
        var result = entity.SomeMethod();

        // Assert
        Assert.Equal(expected, result);
    }

    private static <Entity> CreateTestEntity() { /* factory helper */ }
}
```

## Running API for Manual Testing

```powershell
# Start PostgreSQL
docker compose up -d postgres

# Run the API
dotnet run --project src/MonthlyBudget.Api
```

## Scope Guard — Before Committing

Before every commit, verify you haven't drifted outside the planned scope:

```powershell
# Check what files were changed vs what was expected
git diff --stat HEAD

# Verify no unexpected files were created
git status --short
```

Ask yourself:
1. Is every changed file listed in the implementation plan?
2. Did I add any "nice to have" code (extra logging, XML docs, comments)?
3. Did I add error handling beyond what the invariants require?

If any answer is yes, revert the unplanned changes or ask the user.

## Known Gotchas

| Problem | Solution |
|---|---|
| `dotnet test` hangs | Check if Docker is running (integration tests use Testcontainers) |
| EF migration fails | Ensure `--startup-project` points to `src/MonthlyBudget.Api` |
| Build error on SharedKernel types | Verify `using MonthlyBudget.SharedKernel.Types;` namespace |
| Test naming mismatch | Check existing tests in the same project — use `MethodUnderTest_Scenario_ExpectedBehavior` |
| `HouseholdId` type confusion | It's a value object in SharedKernel, not a raw `Guid` |

## Architecture Extracts for Reference

Instead of reading the full architecture spec, use these focused files:
- **Domain invariants:** `docs/arch/domain-invariants.md`
- **API contracts:** `docs/arch/api-contracts.md`
- **Persistence conventions:** `docs/arch/persistence-conventions.md`
- **Allowed tech stack:** `docs/arch/tech-stack.md`
- **Code patterns per context:** `.github/agents/context/<context>-patterns.md`

## Git Commit Convention

```
type(context): description for #<issue>

# Types: test, feat, refactor, fix, chore
# Contexts: budget, forecast, identity, infra, api
```
