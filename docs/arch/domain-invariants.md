# Domain Invariants Reference

> Extracted from `docs/MonthlyBudget_Architecture.md` §1.3.
> All invariants MUST be enforced in the **Aggregate Root**, not in handlers or controllers.

## MonthlyBudget Aggregate (Budget Management)

| ID | Rule | Enforcement |
|---|---|---|
| INV-B1 | At least one income source must exist before budget activation | `activate()` throws `InsufficientIncomeException` if `incomeSources` is empty |
| INV-B2 | Expense day must be valid for the budget month | `addExpense()` validates `dayOfMonth <= lastDayOfMonth(yearMonth)` |
| INV-B3 | Spread expenses must not have a specific day | `addExpense()` rejects if `isSpread = true AND dayOfMonth != null` |
| INV-B4 | Specific-day expenses must have a day assigned | `addExpense()` rejects if `isSpread = false AND dayOfMonth == null` |
| INV-B5 | Amount must be strictly positive | `addExpense()` and `addIncome()` reject if `amount <= 0` |
| INV-B6 | Budget status transitions are unidirectional | State machine: `DRAFT → ACTIVE → CLOSED`. No reverse transitions. |
| INV-B7 | Only one budget per household per month | Repository enforces unique constraint on `(householdId, yearMonth)` |
| INV-B8 | Only ACTIVE budgets can be modified | Mutations rejected if `status != ACTIVE` (except initial setup in DRAFT) |

### Domain Events

| Event | Payload Keys | Published When |
|---|---|---|
| `BudgetCreated` | `budgetId`, `householdId`, `yearMonth` | New budget instantiated |
| `BudgetActivated` | `budgetId`, `householdId`, `yearMonth` | Budget transitions to ACTIVE |
| `IncomeSourceAdded` | `budgetId`, `incomeId`, `name`, `amount` | Income added |
| `IncomeSourceUpdated` | `budgetId`, `incomeId`, `name`, `amount` | Income modified |
| `IncomeSourceRemoved` | `budgetId`, `incomeId` | Income removed |
| `ExpenseAdded` | `budgetId`, `expenseId`, `name`, `category`, `dayOfMonth`, `isSpread`, `amount` | Expense added |
| `ExpenseUpdated` | `budgetId`, `expenseId`, changed fields | Expense modified |
| `ExpenseRemoved` | `budgetId`, `expenseId` | Expense deleted |
| `ExpenseExclusionToggled` | `budgetId`, `expenseId`, `isExcluded` | Exclusion toggled |
| `BudgetRolledOver` | `sourceBudgetId`, `targetBudgetId`, `targetYearMonth` | Month rollover executed |
| `BudgetClosed` | `budgetId`, `householdId`, `yearMonth` | Budget finalized |

---

## ForecastVersion Aggregate (Forecast Engine)

| ID | Rule | Enforcement |
|---|---|---|
| INV-F1 | Forecast must have daily entries covering startDay through end of month | `generate()` validates completeness |
| INV-F2 | REFORECAST must reference a valid parent forecast | `createReforecast()` rejects if `parentForecastId` is null or invalid |
| INV-F3 | ORIGINAL forecast must have `startDay = 0` | Enforced at creation |
| INV-F4 | Snapshot forecasts are immutable | All mutations rejected if `isSnapshot = true` → `SnapshotImmutableException` |
| INV-F5 | Daily entries must be chronologically ordered | `generate()` ensures ordering |
| INV-F6 | Start balance for ORIGINAL equals total income | Validated against budget income sum at generation time |
| INV-F7 | Only one ORIGINAL forecast per budget | Repository enforces unique constraint on `(budgetId, forecastType=ORIGINAL)` |
| INV-F8 | Reforecast expense adjustments must reference valid parent snapshots | `MODIFY` and `REMOVE` actions require `originalExpenseId` matching an existing parent `ExpenseSnapshot`. `ADD` must provide all required expense fields. Validated in `ReforecastValidator`. |

### Corrective Policies

| Policy | Trigger | Action |
|---|---|---|
| AutoSnapshotOnReforecast | User initiates re-forecast | Auto-save parent as snapshot before proceeding if not already snapshotted |
| PropagateExpenseChanges | `ExpenseUpdated`/`ExpenseAdded` event from Budget context | Flag affected forecasts as stale; do NOT auto-regenerate |

### Domain Events

| Event | Payload Keys | Published When |
|---|---|---|
| `ForecastGenerated` | `forecastId`, `budgetId`, `householdId`, `forecastType`, `forecastDate` | New forecast simulation completed |
| `SnapshotSaved` | `forecastId`, `budgetId`, `snapshotDate` | Forecast saved as snapshot |
| `ReforecastCreated` | `forecastId`, `parentForecastId`, `budgetId`, `actualBalance`, `startDay` | Re-forecast generated |
| `ForecastStaleMarked` | `forecastId`, `reason` | Budget changes invalidate forecast |

---

## Household Aggregate (Identity & Household)

| ID | Rule | Enforcement |
|---|---|---|
| INV-H1 | Maximum 2 members per household | `addMember()` rejects if `members.size >= 2` |
| INV-H2 | Exactly one OWNER per household | `addMember()` rejects role=OWNER if one exists; OWNER removal is forbidden |
| INV-H3 | Email must be globally unique across users | Repository enforces unique constraint on `email` |
| INV-H4 | Only one pending invitation per household | `createInvitation()` rejects if a PENDING invitation exists |
| INV-H5 | Expired invitations cannot be accepted | `acceptInvitation()` checks `expiresAt > now()` |

### Corrective Policies

| Policy | Trigger | Action |
|---|---|---|
| ExpireStaleInvitations | Scheduled job (daily) | Transition PENDING invitations past `expiresAt` to EXPIRED |

### Domain Events

| Event | Payload Keys | Published When |
|---|---|---|
| `HouseholdCreated` | `householdId`, `ownerId` | Household instantiated |
| `MemberInvited` | `householdId`, `invitationId`, `invitedEmail` | Invitation created |
| `MemberJoined` | `householdId`, `userId`, `role` | Invited user accepted and joined |
