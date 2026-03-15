# Technology Stack Reference

> Extracted from `docs/MonthlyBudget_Architecture.md` §4.7 ADRs.
> Do NOT introduce libraries not listed here without an ADR.

## Backend

| Component | Technology | Notes |
|---|---|---|
| Runtime | C# / .NET / ASP.NET Core | ADR-006 |
| CQRS + Mediator | MediatR | Command/query handlers + domain event bus (ADR-004) |
| Validation | FluentValidation | `AbstractValidator<TCommand>`, registered via `ValidationBehavior<,>` |
| ORM | Entity Framework Core | PostgreSQL provider (`Npgsql.EntityFrameworkCore.PostgreSQL`) |
| Database | PostgreSQL | Single instance, separate schemas per context (ADR-002) |
| Password hashing | BCrypt.Net-Next | `BCryptPasswordHasher` implements `IPasswordHasher` |
| JWT | System.IdentityModel.Tokens.Jwt | 15 min access tokens, refresh token rotation (ADR-005) |
| Testing | xUnit | Unit tests per context + `Testcontainers.PostgreSql` for integration |

## Frontend (not yet implemented)

| Component | Technology | Notes |
|---|---|---|
| Framework | SvelteKit | TypeScript, file-based routing (ADR-007) |
| Charts | Chart.js | Line/area charts for forecast visualization |

## Architecture Decisions Summary

| ADR | Decision |
|---|---|
| ADR-001 | Modular Monolith over Microservices |
| ADR-002 | PostgreSQL for all persistence |
| ADR-003 | Anti-Corruption Layer between Budget ↔ Forecast |
| ADR-004 | In-process MediatR event bus for cross-context communication |
| ADR-005 | JWT with household-scoped authorization |
| ADR-006 | C# + ASP.NET Core backend |
| ADR-007 | SvelteKit + Chart.js frontend |
