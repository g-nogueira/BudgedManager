# UI Screen Mapping — MonthlyBudget MVP

## Stitch Project
- **Project ID:** 16333773179188670063
- **Project Title:** MonthlyBudget — PRD Screens
- **Design System:** "Violet Ledger" — unified vibrant purple (`#9116c4`) primary, off-white (`#f5f7fa`) surfaces, pill-shaped CTAs, no 1px borders, tonal layering, generous whitespace
- **Typography:** Plus Jakarta Sans (headlines), Be Vietnam Pro (body/labels)
- **Inspiration:** NuBank (bold accent, rounded cards), Datadog (playful charts), MacroFactor (clean editable lists)

## Unified Design System (Post-Unification Pass)

All screens now share a single, consistent design token set:

| Token | Value |
|---|---|
| Primary | `#9116c4` |
| Background | `#f5f7fa` |
| Primary-container | `#d67aff` |
| On-surface | `#2c2f32` |
| Headline font | Plus Jakarta Sans |
| Body/label font | Be Vietnam Pro |
| Brand name | "MonthlyBudget" (everywhere) |

### Shared Sidebar
- Fixed left, `w-64`, `bg-white`, `rounded-r-xl`, shadow `30px 0 60px -15px rgba(145,22,196,0.08)`
- Logo: text wordmark "MonthlyBudget" in `text-2xl font-black text-primary`
- Nav items (4): Dashboard, Budget, Forecast, Household Settings
- Active state: `bg-primary text-white rounded-full shadow-lg`
- Inactive: `text-slate-500 hover:bg-surface-container-low rounded-full`
- Bottom: user profile card

### Shared Top Navbar
- Fixed `top-0 right-0 w-[calc(100%-16rem)]`, `bg-transparent backdrop-blur-md`
- Left: page title in `text-2xl font-extrabold text-primary`
- Right: search input (rounded-full) + notification bell + help icon
- `py-6 px-12`

## Screens

### 1. Login / Register
- **Screen ID:** `0ac7360fc49f4fe2b9f260c6a84dda07`
- **User Stories:** US-7.1, US-7.2 (prerequisite auth)
- **Device Type:** DESKTOP
- **States Covered:** Normal (Sign In form), Create Account mode, validation errors, loading spinner
- **Key Elements:**
  - Centered floating card on `#f5f7fa` background
  - "MonthlyBudget" wordmark with wallet icon
  - Tagline: "Your household budget, simplified."
  - Email + password inputs with purple gradient "Sign In" pill button
  - Toggle between Sign In and Register modes
- **Notes:** Standalone page — no sidebar or navbar. Uses unified color palette (`#9116c4` primary).

### 2. Dashboard (Home)
- **Screen ID:** `b7008ac569064bab887d9910a86c412f` (unified version)
- **User Stories:** US-8.1, US-3.1, US-3.2
- **Device Type:** DESKTOP
- **States Covered:** Normal (with forecast data), empty (no budget), loading skeleton
- **Key Elements:**
  - Unified sidebar (Dashboard active)
  - Unified navbar with "Dashboard" title
  - Hero: end-of-month forecasted balance (display-lg, green/coral), today's balance, month/year
  - Status pill: "On Track" (green) or "Watch Out" (coral)
  - Forecast chart: line/area chart, balance vs day-of-month, today marker, multi-forecast overlay
  - Quick action cards: Re-Forecast, Save Snapshot, Compare Forecasts
  - Upcoming expenses preview
- **Notes:** Primary landing screen — zero clicks to see financial status.

### 3. Budget Setup / Detail
- **Screen ID:** `e9590ccc930c4e80a668ef08d5f94ad1` (unified version)
- **User Stories:** US-1.1, US-1.2, US-1.3, US-1.4
- **Device Type:** DESKTOP
- **States Covered:** Normal (populated data), empty (no items), loading skeleton
- **Key Elements:**
  - Unified sidebar (Budget active)
  - Unified navbar with "March 2026 Budget" title + DRAFT badge
  - Income section: name + amount list, total income, add/edit/delete
  - Expenses grouped by category (Fixed, Subscriptions, Variable)
  - Expense rows: name, day-of-month (or "Spread" badge), amount, exclusion toggle, edit/delete
  - Excluded expenses: greyed out with strikethrough
  - Bottom summary bar + "Generate Forecast" CTA
- **Notes:** Budget Owner's primary workspace. Category sections match API `category` field.

### 4. Forecast Detail
- **Screen ID:** `804c812f23ef41a79513b270c7f60ed9` (unified version)
- **User Stories:** US-2.1, US-2.2, US-3.1, US-3.2, US-4.1
- **Device Type:** DESKTOP
- **States Covered:** Normal (single forecast), multi-version overlay, empty (no forecast), loading
- **Key Elements:**
  - Unified sidebar (Forecast active)
  - Unified navbar with "March 2026 Forecast" title
  - Forecast version selector (dropdown/pills)
  - Action buttons: Save Snapshot, Re-Forecast, Compare
  - Large chart: balance curve with gradient fill, multi-forecast overlay, legend toggles
  - Day-by-day table: Day | Expenses | Remaining Balance
  - Snapshot list panel
- **Notes:** Chart supports toggling individual forecast lines on/off (US-3.2).

### 5. Re-Forecast Flow — Step 1: Enter Balance
- **Screen ID:** `5df62cdc79f848a793e4d6dbd57efdf5` (unified via edit response)
- **User Stories:** US-4.2
- **Device Type:** DESKTOP
- **States Covered:** Normal, validation error, loading
- **Key Elements:**
  - Unified sidebar (Forecast active)
  - Unified navbar with "Re-Forecast" title
  - Horizontal step indicator: Step 1 (active) → Step 2 → Step 3
  - "What's your actual bank balance today?" headline
  - Large EUR amount input, date picker
  - Info callout: original forecast preserved as snapshot
  - Next / Cancel buttons
- **Notes:** Step 1 of 3-step wizard.

### 6. Re-Forecast Flow — Step 2: Adjust Expenses
- **Screen ID:** `7c2ce2d7f1b6447484c8e9c475843182` (unified via edit response)
- **User Stories:** US-4.3
- **Device Type:** DESKTOP
- **States Covered:** Normal with editable expense list
- **Key Elements:**
  - Unified sidebar (Forecast active)
  - Unified navbar with "Re-Forecast" title
  - Step indicator: Step 2 active
  - Editable list of future expenses with inline amount editing
  - "Changed" badges on modified items
  - "+ Add New Expense" button
  - Running total, Back / Next buttons
- **Notes:** Changes only affect new re-forecast.

### 7. Re-Forecast Flow — Step 3: Review & Generate
- **Screen ID:** `c78598fb5baa41eab652a90984e71299` (unified via edit response)
- **User Stories:** US-4.2, US-4.3
- **Device Type:** DESKTOP
- **States Covered:** Normal (preview), loading (generating), error
- **Key Elements:**
  - Unified sidebar (Forecast active)
  - Unified navbar with "Re-Forecast" title
  - Step indicator: Step 3 active
  - Summary: original vs new end-of-month balance, difference
  - Mini preview chart: original (dashed) vs new (solid)
  - List of changes, "Generate Re-Forecast" CTA
- **Notes:** Confirmation step before committing.

### 8. Forecast Comparison & Drift Analysis
- **Screen ID:** `93dac231bb1c4d96beb7836aa8699d8d` (unified via edit response)
- **User Stories:** US-5.1, US-5.2
- **Device Type:** DESKTOP
- **States Covered:** Normal (two versions compared), empty (single version), loading
- **Key Elements:**
  - Unified sidebar (Forecast active)
  - Unified navbar with "Forecast Comparison" title
  - Two version dropdowns (Version A / Version B) with swap button
  - Overlay chart: two curves, variance band, legend toggles
  - Variance table: Day | Version A | Version B | Difference
  - Expense drift: "What Changed?" — added/removed/changed with old/new/impact
- **Notes:** Aligns with API `/forecasts/compare` endpoint. Green `#00CC99` for comparison accent.

### 9. Month Rollover Review
- **Screen ID:** `92612cf39ba045039f1f515f3d1bc8d5` (unified via edit response)
- **User Stories:** US-6.1, US-6.2
- **Device Type:** DESKTOP
- **States Covered:** Normal (carried-forward data), loading, error
- **Key Elements:**
  - Unified sidebar (Budget active)
  - Unified navbar with "Month Rollover" title
  - Welcome headline: "Welcome to [Month Year]!"
  - Editable income sources, fixed expenses card, subscriptions card
  - Info note: "Variable expenses were not carried forward"
  - "+ Add Variable Expense" button
  - Summary bar + "Finalize & Generate Forecast" CTA
- **Notes:** Only FIXED and SUBSCRIPTION expenses carry forward.

### 10. Household Settings
- **Screen ID:** `4f4df1bad0644d3a81741c2511cd0342` (unified via edit response)
- **User Stories:** US-7.1, US-7.2
- **Device Type:** DESKTOP
- **States Covered:** Normal (solo owner), pending invitation, full (2 members), loading, error
- **Key Elements:**
  - Unified sidebar (Household Settings active)
  - Unified navbar with "Household Settings" title
  - Household name (editable), members list with owner badge
  - Empty partner slot with invite form
  - "Only 1 invitation allowed per household" note
  - Pending invitation status card
- **Notes:** Max 2 members (INV-H1). Owner badge permanent (INV-H2).

## Unification Pass — Changes Applied

The following inconsistencies were fixed across all 10 screens on 2026-03-20:

| Issue | Before | After |
|---|---|---|
| Color palette | 2 palettes: Group A (`#9116c4`) and Group B (`#670090`) | Unified to `#9116c4` primary, `#f5f7fa` background |
| Sidebar | 9 different implementations (various widths, bg colors, roundings) | Single shared sidebar: w-64, bg-white, rounded-r-xl |
| Navbar | 6 different patterns (fixed, sticky, inline, none) | Single shared navbar: fixed, backdrop-blur, page title + search/icons |
| Brand name | 4 names: "MonthlyBudget", "The Lucid Flow", "The Fluid Ledger", "Lucid Flow" | "MonthlyBudget" everywhere |
| Nav items | 3 different sets (some had "Analytics", mismatched icons) | 4 standard items: Dashboard, Budget, Forecast, Household Settings |
| Fonts | 2 setups (single-typeface vs dual-typeface) | Dual: Plus Jakarta Sans (headlines) + Be Vietnam Pro (body) |

## User Story Traceability Matrix

| User Story | Screen(s) |
|---|---|
| US-1.1 (Income Sources) | Budget Setup / Detail |
| US-1.2 (Fixed Expenses) | Budget Setup / Detail |
| US-1.3 (Exclude Expense) | Budget Setup / Detail |
| US-1.4 (Categorize Expenses) | Budget Setup / Detail |
| US-2.1 (Day-by-Day Forecast) | Forecast Detail |
| US-2.2 (Spread Expenses) | Forecast Detail |
| US-3.1 (Forecast Chart) | Dashboard, Forecast Detail |
| US-3.2 (Multi-Forecast Overlay) | Dashboard, Forecast Detail |
| US-4.1 (Save Snapshot) | Forecast Detail |
| US-4.2 (Re-Forecast) | Re-Forecast Steps 1-3 |
| US-4.3 (Adjust Future Expenses) | Re-Forecast Steps 2-3 |
| US-5.1 (Compare Versions) | Forecast Comparison |
| US-5.2 (Identify Drift) | Forecast Comparison |
| US-6.1 (Month Carry-Forward) | Month Rollover Review |
| US-6.2 (Quick Adjust) | Month Rollover Review |
| US-7.1 (Invite Member) | Household Settings |
| US-7.2 (Partner Access) | All screens (shared access) |
| US-8.1 (At-a-Glance Status) | Dashboard |

## PRD Compliance Notes

All 16 user stories are covered. Two minor polish items remain:
- **US-2.2 (Spread Expenses):** Daily total column exists but "spread" portion not explicitly labeled per day
- **US-4.1 (Save Snapshot):** AC mentions "optional actual bank balance" entry during snapshot — not yet shown as input field

---

## Visual Review — Inconsistency Log (2026-03-20)

Issues found during browser review of rendered HTML screens. All items below have been **resolved in the HTML files** unless marked otherwise.

### CRITICAL — Fixed

| # | Screen | Issue | Resolution |
|---|---|---|---|
| V-1 | Re-Forecast Step 3 | **Broken icons** | Fixed — corrected Material Symbols font URL axis format |
| V-2 | Comparison | **Wrong year "March 2024"** | Fixed — changed to "March 2026" |
| V-3 | All screens | **Inconsistent user persona** | Fixed — unified to "Guilherme Nogueira" across all 10 screens |
| V-4 | All screens | **Inconsistent role labels** | Fixed — unified to "Owner" across all screens |

### HIGH — Out-of-Scope Features (Fixed)

| # | Screen | Feature | Resolution |
|---|---|---|---|
| V-5 | Dashboard | "Smart Insight" + "Apply Suggestion" | Fixed — replaced with "Monthly Summary" card showing forecast balance + upcoming expenses count, CTA changed to "View Full Forecast" |
| V-6 | Forecast Detail | "AI INSIGHTS" card | Fixed — replaced with "Forecast Tip" card using `lightbulb` icon, factual rent percentage insight |
| V-7 | Forecast Detail | "Upgrade Flow" link in sidebar | Fixed — removed entirely |
| V-8 | Household Settings | "Premium Plan" label | Fixed — changed to "Owner" (done as part of V-4) |
| V-9 | Household Settings | "SAFE SYNC" badge, savings pots, shared targets | Fixed — badge changed to "Shared" with `group` icon; bullets rewritten to PRD scope (shared forecast visibility, expense editing, reduced financial anxiety) |
| V-10 | Comparison | "Export Detailed Analysis" button | Fixed — removed entirely |
| V-11 | Login | Google / Apple social login buttons | Fixed — removed "Or continue with" divider and both social buttons |

### MEDIUM — Design Inconsistencies (Fixed)

| # | Screen(s) | Issue | Resolution |
|---|---|---|---|
| V-12 | Various | **Search placeholder varies** | Fixed — unified to "Search..." across all screens |
| V-13 | Various | **Avatar style varies** | Fixed — standardized to purple circle (`bg-[#9116c4]`) with "GN" white initials across all 9 app screens |
| V-14 | Rollover | **CTA button style** | No action — already `rounded-full` pill shape; visual appearance in browser is from text wrapping in floating footer |
| V-15 | Comparison | **Floating action button** | Fixed — FAB removed from bottom-right corner |
| V-16 | Rollover | **Truncated "MERCI..." text** | No action — text not found in HTML; render artifact from floating summary bar clipping |
| V-17 | Forecast Detail | **Currency label "EUR (€)"** | Fixed — removed from chart area |
| V-18 | Comparison | **Breadcrumbs** | Kept — per design decision to retain breadcrumbs on Comparison for navigational context |
| V-19 | Household Settings | **3D illustration** | Fixed — replaced with gradient card using `diversity_3` Material icon and "Better together" text |

---

## UX Improvement Recommendations

| # | Area | Recommendation | Priority |
|---|---|---|---|
| UX-1 | Empty states | No screen demonstrates the "no data yet" empty state. Each data-displaying screen needs an empty state design (e.g., Dashboard with no budget, Forecast Detail with no forecast). | High |
| UX-2 | Loading states | No screen shows a loading skeleton or spinner. The architecture requires every page to handle loading state. | High |
| UX-3 | Error states | No screen demonstrates error handling (API failure, validation errors, network issues). | High |
| UX-4 | Month navigation | There is no visible mechanism for navigating between months. How does the user switch from March to February to review historical data, or navigate forward to April? | High |
| UX-5 | Budget lifecycle actions | The PRD defines statuses DRAFT → ACTIVE → CLOSED. The "Activate Budget" button is present on Budget Setup, but there's no visible "Close Budget" action. | Medium |
| UX-6 | Excluded expense visual | On Budget Setup, the excluded expense (Electricity, grey toggle) does not show the strikethrough treatment described in the design spec and required by US-1.3 AC. | Medium |
| UX-7 | Re-Forecast Step 2 past expenses | "Electricity — Day 19" shows as "Completed" but today is Day 20. A past expense appearing in the "adjust future expenses" list is confusing. It should either be clearly separated or hidden. | Medium |
| UX-8 | Snapshot save UX | US-4.1 AC requires "optional user-entered actual bank balance" when saving a snapshot. The Forecast Detail "Create New Snapshot" action doesn't show an input field for this. | Medium |
| UX-9 | Forecast Detail profile mismatch | Sidebar shows "The Miller Home / Family Plan" + "Upgrade Flow" — completely different persona and implies premium tiers that don't exist. | Medium |
| UX-10 | Dashboard "Subscription Renewal" date | Shows "APRIL 01" — should upcoming expenses only show the current month, or is cross-month lookahead intentional? | Low |

---

## User Journeys

### Journey 1: First-Time Setup (Budget Owner)

**Goal:** Set up the first monthly budget and generate a forecast.

```
Login ─→ Dashboard (empty state)
  │
  ├─→ Budget Setup
  │     ├─ Add income sources (US-1.1)
  │     ├─ Add fixed expenses with day-of-month (US-1.2)
  │     ├─ Add subscription expenses
  │     ├─ Add variable expenses (day-specific or spread) (US-1.2)
  │     ├─ Optionally exclude any expense (US-1.3)
  │     ├─ Review grouped expense list (US-1.4)
  │     └─ Click "Activate Budget" → status changes DRAFT → ACTIVE
  │
  └─→ Click "Generate Forecast" (US-2.1)
        │
        └─→ Dashboard (populated)
              ├─ End-of-month balance displayed (US-8.1)
              ├─ Forecast chart visible (US-3.1)
              └─ Quick actions: Re-Forecast, Save Snapshot, Compare
```

**Key screens:** Login → Dashboard → Budget Setup → Dashboard

---

### Journey 2: Quick Check-In (Household Partner)

**Goal:** See current financial status at a glance — zero friction.

```
Login ─→ Dashboard
           ├─ See end-of-month forecasted balance (hero number)
           ├─ See "On Track" / "Watch Out" status pill
           ├─ Glance at forecast chart (US-3.1)
           ├─ See upcoming expenses
           │
           └─ (Optional) Click chart or "View Full Budget" →
                 ├─ Forecast Detail (US-2.1, US-2.2)
                 └─ Budget Setup (view expense list)
```

**Key screens:** Login → Dashboard (one-screen experience)

---

### Journey 3: Mid-Month Re-Forecast (Budget Owner)

**Goal:** Reality has drifted from the plan — enter actual balance and re-forecast.

```
Dashboard ─→ Forecast Detail
               ├─ Notice chart divergence
               └─ Click "Re-Forecast"
                     │
                     ├─→ Step 1: Enter Balance (US-4.2)
                     │     ├─ Enter actual bank balance (€3,100)
                     │     ├─ Confirm date (March 20)
                     │     ├─ See info: original auto-saved as snapshot
                     │     └─ Click "Next Step"
                     │
                     ├─→ Step 2: Adjust Expenses (US-4.3)
                     │     ├─ Review upcoming expenses from today onward
                     │     ├─ Modify amounts inline
                     │     ├─ Add new expense / remove planned expense
                     │     ├─ See "CHANGED" badges on edits
                     │     └─ Click "Next"
                     │
                     └─→ Step 3: Review & Generate (US-4.2, US-4.3)
                           ├─ See original vs new end-of-month comparison
                           ├─ See diff: -€427.50
                           ├─ Review list of changes
                           ├─ Preview chart: dashed (original) vs solid (new)
                           └─ Click "Generate Re-Forecast"
                                 │
                                 └─→ Forecast Detail (updated)
                                       ├─ Two lines on chart (US-3.2)
                                       └─ New snapshot listed
```

**Key screens:** Dashboard → Forecast Detail → Re-Forecast Steps 1–3 → Forecast Detail

---

### Journey 4: Snapshot & Comparison (Budget Owner)

**Goal:** Save the current state and compare forecast versions to diagnose drift.

```
Forecast Detail
  ├─ Click "Save Snapshot" (US-4.1)
  │     └─ Snapshot appears in right panel
  │
  └─ Click "Compare" (US-5.1)
        │
        └─→ Forecast Comparison
              ├─ Select Version A (Original Mar 1) and Version B (Re-forecast Mar 15)
              ├─ See overlay chart with two curves (US-5.1)
              ├─ See projected end balance: €2,847.50 vs €2,420.00
              ├─ See "What Changed?" section: (US-5.2)
              │     ├─ Electricity: €85 → €95 (+€10)
              │     ├─ Car Repair: NEW (+€200)
              │     └─ Dining Out: €400 → €350 (-€50)
              └─ See "Daily Drift" table: day-by-day variance
```

**Key screens:** Forecast Detail → Comparison

---

### Journey 5: New Month Rollover (Budget Owner)

**Goal:** Start a new month without rebuilding from scratch.

```
Dashboard (end of March / start of April trigger)
  │
  └─→ Month Rollover Review (US-6.1)
        ├─ "Welcome to April 2026!"
        ├─ Review carried-forward income sources (edit inline)
        ├─ Review fixed expenses (toggle on/off, edit amounts) (US-6.2)
        ├─ Review subscriptions (toggle, edit)
        ├─ Note: "Variable expenses were not carried forward"
        ├─ Add new variable expenses if needed
        ├─ Review summary bar: income / expenses / remaining
        └─ Click "Finalize & Generate Forecast"
              │
              └─→ Dashboard (new month, populated)
```

**Key screens:** Dashboard → Rollover → Dashboard

---

### Journey 6: Invite Household Partner (Budget Owner)

**Goal:** Share the budget with a partner.

```
Dashboard ─→ Household Settings (via sidebar)
               ├─ See household name + owner info
               ├─ See "1 of 2 spots filled"
               ├─ Enter partner's email address (US-7.1)
               └─ Click "Send Invitation"
                     │
                     └─ Invitation sent → status card shows "Pending"

Partner receives email ─→ Login (Create Account) ─→ Dashboard
  └─ Partner sees same data as Owner (US-7.2)
```

**Key screens:** Household Settings (Owner) → Login (Partner) → Dashboard (Partner)

---

### Journey 7: Day-by-Day Forecast Review (Either User)

**Goal:** Understand exactly when expenses hit and what the balance looks like each day.

```
Dashboard ─→ Forecast Detail (via sidebar or chart click)
               ├─ View forecast version selector (Original, Re-forecast)
               ├─ View chart with tooltip: hover to see balance + expense per day
               ├─ Scroll to Day-by-Day table (US-2.1)
               │     ├─ Day 0: €5,200 (starting balance)
               │     ├─ Day 5: -€450 Groceries → €4,750
               │     ├─ Day 12: -€1,200 Rent → €3,430 (TODAY marker)
               │     └─ Day 31: €2,847.50 (end of month)
               ├─ Toggle between forecast versions on chart (US-3.2)
               └─ View snapshot list in right panel
```

**Key screens:** Dashboard → Forecast Detail

---

## Design Decisions
- **Color system:** Unified vibrant purple (`#9116c4`) primary, green (`#00CC99`) for positive signals, coral for warnings
- **No borders:** Container boundaries via tonal surface shifts and ambient shadows
- **Pill-shaped CTAs:** Full rounding with purple gradient fills
- **Typography:** Plus Jakarta Sans (headlines) + Be Vietnam Pro (body) dual-typeface strategy
- **Sidebar:** Fixed w-64 with text wordmark, purple pill active state
- **Navbar:** Fixed backdrop-blur with page title and utility icons
- **Re-Forecast as wizard:** 3-step flow reduces cognitive load
- **Excluded expenses:** Greyed out + strikethrough

## Open Questions
- Should the sidebar collapse to hamburger on smaller desktop viewports (< 1024px)?
- Should the forecast chart default to line or area — or let user toggle?
- Should the Re-Forecast wizard be a single scrollable page instead of 3 separate screens?
- Should snapshot names be auto-generated or user-customizable?
- How does the user navigate between months (month picker, arrows, dropdown)?
- What triggers the rollover flow — manual action, or auto-prompt on first login in a new month?
- Should the search bar in the navbar be functional for MVP, or a future placeholder?

## Downloaded HTML Files
All screen HTML files are stored in `docs/product/screens/`:
- [login.html](screens/login.html)
- [dashboard.html](screens/dashboard.html)
- [budget-setup.html](screens/budget-setup.html)
- [forecast-detail.html](screens/forecast-detail.html)
- [reforecast-step1.html](screens/reforecast-step1.html)
- [reforecast-step2.html](screens/reforecast-step2.html)
- [reforecast-step3.html](screens/reforecast-step3.html)
- [comparison.html](screens/comparison.html)
- [rollover.html](screens/rollover.html)
- [household-settings.html](screens/household-settings.html)
