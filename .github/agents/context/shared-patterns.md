# Codebase Patterns — Shared Conventions

> Auto-extracted from the actual codebase. Use these as copy-paste references when implementing new features.
> These are REAL patterns from the codebase — do not deviate from them.

## Command + Result (co-located in same file)

```csharp
// File: Application/Features/{FeatureName}/{FeatureName}Command.cs
using MediatR;
namespace MonthlyBudget.{Context}.Application.Features.{FeatureName};

public sealed record {FeatureName}Command(/* params */) : IRequest<{FeatureName}Result>;
public sealed record {FeatureName}Result(/* result fields */);
```

## Handler

```csharp
// File: Application/Features/{FeatureName}/{FeatureName}Handler.cs
using MediatR;
namespace MonthlyBudget.{Context}.Application.Features.{FeatureName};

public sealed class {FeatureName}Handler : IRequestHandler<{FeatureName}Command, {FeatureName}Result>
{
    private readonly IRepository _repo;
    private readonly IEventPublisher _pub;

    public {FeatureName}Handler(IRepository repo, IEventPublisher pub)
    { _repo = repo; _pub = pub; }

    public async Task<{FeatureName}Result> Handle({FeatureName}Command cmd, CancellationToken ct)
    {
        // 1. Load aggregate from repository
        // 2. Verify household ownership
        // 3. Call domain method
        // 4. Save via repository
        // 5. Publish domain events:
        //    foreach (var evt in entity.GetDomainEvents()) await _pub.PublishAsync(evt, ct);
        //    entity.ClearDomainEvents();
        // 6. Return result
    }
}
```

## Validator

```csharp
// File: Application/Features/{FeatureName}/{FeatureName}Validator.cs
using FluentValidation;
namespace MonthlyBudget.{Context}.Application.Features.{FeatureName};

public sealed class {FeatureName}Validator : AbstractValidator<{FeatureName}Command>
{
    public {FeatureName}Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
```

## Domain Event

```csharp
// File: Domain/Events/{EventName}.cs
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.{Context}.Domain.Events;

public sealed class {EventName} : DomainEventBase
{
    public Guid EntityId { get; }
    // ... get-only properties
    public {EventName}(Guid entityId /*, ... */)
    {
        EntityId = entityId;
    }
}
```

Base: `DomainEventBase` (from `SharedKernel.Events`) implements `IDomainEvent` + `INotification`. Auto-generates `EventId` and `OccurredAt`.

## Domain Exception

```csharp
// File: Domain/Exceptions/{ExceptionName}.cs
namespace MonthlyBudget.{Context}.Domain.Exceptions;

public class {ExceptionName} : DomainException
{
    public {ExceptionName}(/* context param */)
        : base($"Descriptive message with '{contextParam}'.") { }
}
```

Base: `DomainException` (abstract, extends `Exception`) lives in same `Domain/Exceptions/` folder.

## Repository Interface

```csharp
// File: Domain/Repositories/I{Entity}Repository.cs
namespace MonthlyBudget.{Context}.Domain.Repositories;

public interface I{Entity}Repository
{
    Task<Entity?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(Entity entity, CancellationToken ct = default);
    Task<bool> ExistsAsync(/* params */, CancellationToken ct = default);
    Task<IReadOnlyList<Entity>> FindAllByAsync(/* params */, CancellationToken ct = default);
}
```

Convention: All async, `CancellationToken ct = default`, returns `Task<T?>` for finds, `IReadOnlyList<T>` for lists.

## Event Publisher Port

```csharp
// File: Application/Ports/I{Context}EventPublisher.cs
using MonthlyBudget.SharedKernel.Events;
namespace MonthlyBudget.{Context}.Application.Ports;

public interface I{Context}EventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
```

## Controller

```csharp
// File: Infrastructure/Controllers/{Entity}Controller.cs
[ApiController]
[Route("api/v1/{resource}")]
[Authorize]
public sealed class {Entity}Controller : ControllerBase
{
    private readonly IMediator _mediator;
    public {Entity}Controller(IMediator mediator) { _mediator = mediator; }

    // Extract householdId from JWT claim:
    private Guid HouseholdId => Guid.Parse(User.FindFirstValue("householdId")!);

    // Extract userId from JWT claim:
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User ID claim not found"));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequestDto req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCommand(HouseholdId, req.Field), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

## EF Configuration

```csharp
// File: src/MonthlyBudget.Infrastructure/Database/Configurations/{Entity}Configuration.cs
public class {Entity}Configuration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("table_name", "schema_name"); // snake_case, schema per context
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        // Navigation: UsePropertyAccessMode(PropertyAccessMode.Field) for private backing fields
    }
}
```

## DI Registration

All in `src/MonthlyBudget.Infrastructure/ServiceCollectionExtensions.cs` → `AddInfrastructure()`.
Grouped by bounded context with comment separators. All `AddScoped<IPort, Adapter>()`.

## Test File

```csharp
// File: tests/MonthlyBudget.{Context}.Tests/Domain/{Aggregate}Tests.cs
using Xunit;
using MonthlyBudget.{Context}.Domain.Entities;
using MonthlyBudget.{Context}.Domain.Events;
using MonthlyBudget.{Context}.Domain.Exceptions;
using Alias = MonthlyBudget.{Context}.Domain.Entities.{Entity}; // if naming conflict

namespace MonthlyBudget.{Context}.Tests.Domain;

public class {Aggregate}Tests
{
    private static readonly Guid HouseholdId = Guid.NewGuid();

    [Fact]
    public void MethodUnderTest_Scenario_ExpectedBehavior()
    {
        // Arrange
        var entity = CreateTestEntity();
        // Act
        entity.SomeAction();
        // Assert
        Assert.Equal(expected, entity.Property);
        Assert.Single(entity.GetDomainEvents().OfType<SomeEvent>());
    }

    [Theory]
    [InlineData("value1")]
    [InlineData("value2")]
    public void MethodUnderTest_InvalidInput_Throws(string input)
    {
        Assert.Throws<SomeException>(() => entity.SomeAction(input));
    }

    // --- Helpers ---
    private static Entity CreateTestEntity() { /* factory */ }
    private static Entity CreateActiveEntity()
    {
        var entity = CreateTestEntity();
        // ... setup
        entity.ClearDomainEvents();
        return entity;
    }
}
```

Naming: `MethodUnderTest_Scenario_ExpectedBehavior`. Organized by section comments. No mocking — pure domain tests.
