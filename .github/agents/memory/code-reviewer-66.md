# Code Review Output — PR #66 / Issue #2

## Verdict
⚠️ CHANGES REQUESTED

## Architecture Compliance
| Check | Status | Details |
|---|---|---|
| Hexagonal layer purity (Domain) | ✅ | Zero forbidden imports in `Domain/`. Only `System.*` and `MonthlyBudget.SharedKernel.*`. |
| Hexagonal layer purity (Application) | ✅ | No EF Core or HTTP in `Application/`. MediatR + FluentValidation only. |
| No cross-context imports | ✅ | `IdentityHousehold` references no `BudgetManagement` or `ForecastEngine` namespaces. |
| Port/Adapter pattern | ✅ | `IHouseholdEventPublisher` in `Application/Ports/`; `MediatRHouseholdEventPublisher` in `Infrastructure/Events/`. |
| Event-based communication | ✅ | `HouseholdCreated`, `MemberInvited`, `MemberJoined` published via `IHouseholdEventPublisher` → MediatR. |
| DI registration | ✅ | `IHouseholdEventPublisher` and `ValidationBehavior<,>` registered in `ServiceCollectionExtensions.cs`. |

## Issues Found
| # | Severity | File | Description | Fix |
|---|---|---|---|---|
| 1 | WARNING | `Application/Features/InviteMember/InviteMemberHandler.cs` L21 | INV-H2 "Only OWNER can invite" check is in Application handler, not Aggregate Root — violates arch mandate (AGENTS.md + arch spec §1.3) | Add `Household.AuthorizeInvite(Guid actorUserId)` domain method throwing `InsufficientRoleException`, call from handler |
| 2 | WARNING | `Application/Features/InviteMember/InviteMemberCommand.cs` | `InviteMemberResult` returns `{invitationId, token}` — spec §2.5 says `{invitationId}` only; token should travel via email only | Remove `Token` from `InviteMemberResult` |
| 3 | WARNING | `Application/Features/JoinHousehold/JoinHouseholdCommand.cs` | `JoinHouseholdResult` returns `{householdId, accessToken}` — spec §2.5 says `{householdId}` only | Align with spec or raise ADR to document the deviation |
| 4 | WARNING | PR body — Phase 6 section | Register response documented as `{userId, accessToken, refreshToken}`; actual code returns `{userId, email, displayName}` | Correct PR body Phase 6 documentation |
| 5 | INFO | `tests/MonthlyBudget.IdentityHousehold.Tests/Domain/IdentityHouseholdTests.cs` | No unit test for INV-H2 rejection (non-OWNER calling invite) | Add handler-level test with mock repos; becomes domain-level test once Issue #1 is fixed |
| 6 | INFO | `src/MonthlyBudget.Infrastructure/Repositories/PostgresIdentityRepositories.cs` | `SaveAllAsync` uses N+1 queries (`FindAsync` inside loop) | Batch-lookup all IDs, then upsert |
| 7 | INFO | `src/Modules/MonthlyBudget.IdentityHousehold/Infrastructure/Controllers/AuthHouseholdController.cs` L62 | Invite endpoint uses `StatusCode(201)` — no `Location` header | Use `CreatedAtAction` or `Created(uri, result)` |

## Task Completeness
| Acceptance Criterion | Met | Notes |
|---|---|---|
| Register/login + JWT with `householdId` claim | ✅ | |
| Budget Owner creates household, invites partner via email | ✅ | Token also returned in API response (Issue #2) |
| Partner accepts invitation, creates account, accesses household | ✅ | Register-first-then-join flow |
| INV-H1 (max 2 members) enforced | ✅ | `HouseholdFullException` |
| INV-H2 (OWNER-only invite, OWNER non-removable) | ⚠️ | OWNER check in handler not aggregate (Issue #1) |
| INV-H3 (unique emails) enforced | ✅ | Repository unique constraint + `DuplicateEmailException` |
| INV-H4 (one pending invite) enforced | ✅ | `Household.GuardPendingInvitation()` domain method |
| INV-H5 (expired invite rejected) enforced | ✅ | `Invitation.Accept()` checks `expiresAt` |
| Expired invitations auto-transitioned by scheduled job | ✅ | `ExpireStaleInvitationsService` — daily `BackgroundService` |

## Build & Test Results
- `dotnet build`: ✅ PASS — 0 errors, 2 NU1603 warnings (pre-existing)
- `dotnet test` (Identity unit): ✅ 21 passed, 0 failed
- `dotnet test` (Budget unit): ✅ 29 passed, 0 failed
- `dotnet test` (Forecast unit): ✅ 14 passed, 0 failed
- `dotnet test` (Integration): Not run locally — requires Docker/Testcontainers; 23 reported in PR body
