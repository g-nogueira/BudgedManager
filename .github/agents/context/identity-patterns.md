# Codebase Patterns — Identity & Household Context

> Real code patterns extracted from the existing codebase. Copy these conventions exactly.

## Namespace Root
`MonthlyBudget.IdentityHousehold`

## Aggregate Root — Household

**File:** `src/Modules/MonthlyBudget.IdentityHousehold/Domain/Entities/Household.cs`

Public API surface (verify against actual file before using):
```csharp
public static Household Create(Guid ownerId, string name)
public Invitation CreateInvitation(string email, TimeSpan ttl)
public void AddMember(Guid userId, MemberRole role)
public void GuardPendingInvitation()
public IReadOnlyList<IDomainEvent> GetDomainEvents()
public void ClearDomainEvents()
```

Properties: `HouseholdId`, `Name`, `Members` (readonly), `Invitations` (readonly), `CreatedAt`.

## Standalone Entities

### User
**File:** `Domain/Entities/User.cs`
- `UserId`, `Email`, `DisplayName`, `PasswordHash`, `CreatedAt`
- Static factory: `User.Create(email, displayName, passwordHash)`

### Invitation
**File:** `Domain/Entities/Invitation.cs`
- `InvitationId`, `HouseholdId`, `InvitedEmail`, `Token`, `Status` (enum), `CreatedAt`, `ExpiresAt`
- `Accept()` method checks `ExpiresAt > now()`
- `Expire()` method transitions to EXPIRED

## Value Objects

- `Member` — `UserId`, `Role` (enum `MemberRole { OWNER, PARTNER }`), `JoinedAt`

## Enums

```csharp
public enum MemberRole { OWNER, PARTNER }
public enum InvitationStatus { PENDING, ACCEPTED, EXPIRED }
```

## Repository Interfaces

**File:** `Domain/Repositories/IUserRepository.cs`
```csharp
Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default);
Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
Task SaveAsync(User user, CancellationToken ct = default);
```

**File:** `Domain/Repositories/IHouseholdRepository.cs`
```csharp
Task<Household?> FindByIdAsync(Guid householdId, CancellationToken ct = default);
Task<Household?> FindByUserIdAsync(Guid userId, CancellationToken ct = default);
Task SaveAsync(Household household, CancellationToken ct = default);
```

**File:** `Domain/Repositories/IInvitationRepository.cs`
```csharp
Task<Invitation?> FindByTokenAsync(string token, CancellationToken ct = default);
Task<Invitation?> FindPendingByHouseholdAsync(Guid householdId, CancellationToken ct = default);
Task SaveAsync(Invitation invitation, CancellationToken ct = default);
```

## Application Ports

- `IPasswordHasher` — `string Hash(string plain)`, `bool Verify(string plain, string hash)`
- `ITokenService` — `string GenerateAccessToken(User user, Guid? householdId)`, `string GenerateRefreshToken()`
- `IEmailService` — `Task SendInvitationAsync(string email, string token, CancellationToken ct)`
- `IHouseholdEventPublisher` — `Task PublishAsync(IDomainEvent evt, CancellationToken ct)`

## Controllers

**File:** `Infrastructure/Controllers/AuthHouseholdController.cs` (two controllers in one file)

### AuthController
- Route: `[Route("api/v1/auth")]`
- `[AllowAnonymous]` on register and login
- No householdId extraction needed (pre-auth)

### HouseholdController
- Route: `[Route("api/v1/households")]`
- `[Authorize]`
- UserId extraction: `Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? throw ...)`

## EF Config — Schema: `identity`

Tables: `users`, `households`, `household_members`, `invitations`

## DI registrations
```csharp
services.AddScoped<IUserRepository, PostgresUserRepository>();
services.AddScoped<IHouseholdRepository, PostgresHouseholdRepository>();
services.AddScoped<IInvitationRepository, PostgresInvitationRepository>();
services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
services.AddScoped<ITokenService, JwtTokenService>();
services.AddScoped<IEmailService, ConsoleEmailService>();
services.AddScoped<IHouseholdEventPublisher, MediatRHouseholdEventPublisher>();
```

## Background Service

**File:** `src/MonthlyBudget.Infrastructure/ExpireStaleInvitationsService.cs`
- `BackgroundService` that runs daily
- Transitions PENDING invitations past `expiresAt` to EXPIRED

## Existing Feature Folders
```
Application/Features/
  RegisterUser/
  AuthenticateUser/
  CreateHousehold/
  InviteMember/
  JoinHousehold/
  GetHousehold/
```
