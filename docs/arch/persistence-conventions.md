# Persistence Conventions Reference

> Extracted from `docs/MonthlyBudget_Architecture.md` §3.1 and `AGENTS.md`.

## Schema Isolation

Each bounded context owns its own PostgreSQL schema:

| Context | Schema | Config Location |
|---|---|---|
| Budget Management | `budget` | `src/MonthlyBudget.Infrastructure/Database/Configurations/MonthlyBudgetConfiguration.cs` etc. |
| Forecast Engine | `forecast` | `src/MonthlyBudget.Infrastructure/Database/Configurations/ForecastVersionConfiguration.cs` etc. |
| Identity & Household | `identity` | `src/MonthlyBudget.Infrastructure/Database/Configurations/UserConfiguration.cs` etc. |

## Cross-Context References

- Use UUIDs only — **no database-level foreign keys** across schemas
- Application-level integrity (handlers check existence before referencing)
- `householdId` is the universal tenant identifier — every query MUST be scoped by it

## Column Conventions

| Rule | Example |
|---|---|
| Snake_case column names | `budget_id`, `year_month`, `created_at` |
| Guid PKs: `ValueGeneratedNever()` | `builder.Property(b => b.BudgetId).ValueGeneratedNever();` |
| Enums stored as strings | `builder.Property(b => b.Status).HasConversion<string>();` |
| Currency: `DECIMAL(12,2)` | Always EUR, never `float` or `double` |
| Navigation via private fields | `builder.Navigation(b => b.Expenses).UsePropertyAccessMode(PropertyAccessMode.Field);` |
| Cascade deletes for owned children | `.OnDelete(DeleteBehavior.Cascade)` |

## EF Configuration Pattern

All configurations live in `src/MonthlyBudget.Infrastructure/Database/Configurations/` (NOT in module-level folders).

```csharp
public class ExampleConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("table_name", "schema_name");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        // ... snake_case columns, string conversions for enums
        builder.HasIndex(e => new { e.Field1, e.Field2 }).IsUnique();
    }
}
```

## AppDbContext

- Location: `src/MonthlyBudget.Infrastructure/Database/AppDbContext.cs`
- Single context for all schemas
- `OnModelCreating` applies all `IEntityTypeConfiguration<T>` from the infrastructure assembly

## Migrations

- Location: `src/MonthlyBudget.Infrastructure/Migrations/`
- Add: `dotnet ef migrations add <Name> --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api`
- Apply: `dotnet ef database update --project src/MonthlyBudget.Infrastructure --startup-project src/MonthlyBudget.Api`
