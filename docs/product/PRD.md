# Product Requirements Document (PRD)

## 1. Executive Summary & Lean Hypothesis

**Product Name:** MonthlyBudget

**Executive Summary:**
MonthlyBudget is a household budgeting tool that replaces a convoluted, manually-maintained Google Sheet with an intuitive interface for forecasting end-of-month remaining balance. It simulates daily expenses against income, provides visual graphs of the forecast curve, and — critically — enables non-destructive mid-month re-forecasting so users can compare their original plan against evolving reality without losing historical data. The tool is designed for both technically-inclined and non-technical household members.

**Lean Hypothesis:**
> If we provide a simple budgeting tool that allows users to set up monthly expenses and income, generates a day-by-day balance forecast with visual graphs, and supports non-destructive re-forecasting with snapshot comparison — then household couples will check in at least weekly, achieve forecast accuracy within 5% of the actual end-of-month balance, and report reduced financial anxiety around shared purchase decisions.

**Build-Measure-Learn Loop:**
- **Build:** MVP with expense/income setup, daily forecast simulation, visual charts, snapshot/compare capability, and a clean non-technical interface.
- **Measure:** Forecast accuracy (predicted vs. actual end-of-month balance), check-in frequency per month, dual-user engagement within the same household.
- **Learn:** Validate whether non-destructive re-forecasting and visualization meaningfully improve users' sense of financial control and reduce anxiety compared to the spreadsheet baseline.

---

## 2. Target Personas & Jobs-To-Be-Done

| Persona | Target Job | Emotional/Social Aspects | Aspirations |
| :--- | :--- | :--- | :--- |
| **Budget Owner** — Technically-inclined household member who currently maintains a complex Google Sheet to forecast monthly finances. Checks/updates the budget weekly or a few times per month. | Set up and maintain a monthly financial forecast: define expenses (fixed, subscriptions, variable), input income sources, and generate a day-by-day remaining balance simulation. Re-forecast mid-month from actual bank balance when reality drifts from the plan. | Feels burdened by the manual, error-prone spreadsheet ritual. Wants confidence that the forecast is accurate. Desires to share financial visibility with partner without having to "translate" the spreadsheet. | Achieve predictable, accurate end-of-month balance forecasting. Build a habit of controlled, confident household financial management. Free up mental energy currently spent on spreadsheet maintenance. |
| **Household Partner** — Non-technical spouse who needs to check financial status, understand it at a glance, and occasionally log an expense or update. | Quickly check whether the household is on track for the month. Understand at a glance how much is left to spend. Occasionally add or update an expense without needing technical knowledge. | Feels excluded from household financial planning due to the spreadsheet's complexity. Wants to participate in purchase decisions with shared data rather than relying on the partner's verbal summary. Experiences anxiety about unexpected financial shortfalls. | Feel equally informed and empowered in household financial decisions. Reduce money-related anxiety and disagreements. Have a shared, transparent view of "where we stand" at any moment. |

---

## 3. Problem Statement & Strategic Objectives

### Problem Statement

A household managing its monthly budget through a manually-maintained Google Sheet faces five compounding problems:

1. **No Historical Snapshots:** There is no way to go back to a specific day (e.g., the 15th) and compare what was forecasted versus what actually happened. The sheet represents only the current state.
2. **Destructive Re-Forecasting:** To re-calculate the forecast mid-month using the actual bank balance, the user must overwrite the original forecast — permanently losing the baseline for comparison.
3. **No Visualizations:** The sheet contains only raw numbers with no graphs, charts, or visual indicators to quickly convey trends, drift, or remaining runway.
4. **Inaccessible to Non-Technical Users:** The spreadsheet's formula-driven structure is incomprehensible to a non-technical household member, creating an information asymmetry that leads to exclusion from financial decisions.
5. **Painful Monthly Ritual:** Every month requires manual adjustment of expenses, income values, and recalculation of the daily simulation — a tedious, error-prone process.

### Strategic Objectives

| # | Objective | Rationale |
| :--- | :--- | :--- |
| O1 | Enable non-destructive forecast versioning | Eliminate the core limitation where re-forecasting destroys the original plan. Users must be able to maintain multiple forecast versions (original + re-forecasts) for the same month. |
| O2 | Provide instant visual clarity on financial trajectory | Replace raw numbers with a visual forecast curve so both personas can immediately grasp the month's financial trajectory without interpreting formulas. |
| O3 | Make the tool accessible to non-technical users | Ensure the Household Partner can independently check status, understand the forecast, and make simple updates without technical guidance. |
| O4 | Reduce monthly setup effort | Minimize the repetitive manual work required to start each new month's forecast by carrying forward recurring expenses and allowing quick adjustments. |
| O5 | Enable drift diagnosis | When the forecast diverges from reality, the user should be able to identify which specific expenses caused the drift. |

---

## 4. Actionable Success Metrics (AARRR Framework)

| AARRR Stage | Metric | Target | Measurement Method |
| :--- | :--- | :--- | :--- |
| **Acquisition** | New household sign-ups per month | Baseline to be established post-launch | Count of new accounts created |
| **Activation** | % of new users who complete first monthly budget setup and generate a forecast within 7 days of sign-up | ≥ 70% | Track completion of first forecast generation event |
| **Retention** | % of active users who check or update their forecast ≥ 3 times per month | ≥ 60% | Count of distinct session days per user per month |
| **Retention (Household)** | % of households where both members access the tool in the same month | ≥ 40% | Track distinct user logins per household per month |
| **Revenue (Value Proxy)** | Forecast accuracy — average absolute deviation between forecasted and actual end-of-month balance | ≤ 5% deviation | User-reported actual end-of-month balance vs. final forecast value |
| **Referral** | % of users who share or recommend the tool | Baseline to be established | In-app referral or NPS survey |

**North Star Metric:** Forecast Accuracy — the average deviation between the forecasted end-of-month remaining balance and the actual end-of-month balance, measured across all active users.

---

## 5. Scope Boundaries (Strict Non-Goals for MVP)

The following are explicitly **out of scope** for the MVP and must not be built, designed, or planned for in the initial release:

| # | Non-Goal | Rationale |
| :--- | :--- | :--- |
| NG1 | Multi-month trend analysis or year-over-year comparisons | MVP focuses on mastering the single-month forecast cycle first. |
| NG2 | Bank account integration or automatic transaction imports | Adds significant complexity, security concerns, and third-party dependencies. Users will manually input data as they do today. |
| NG3 | Expense categorization analytics or spending insights | The core job is forecasting, not retrospective spending analysis. |
| NG4 | Savings goals or investment tracking | Out of scope for the core forecasting job-to-be-done. |
| NG5 | Mobile-native application | Web-first approach for MVP. Mobile optimization may follow but native apps are not in scope. |
| NG6 | Multi-currency support | The user operates in a single currency (EUR). |
| NG7 | AI-powered expense predictions or anomaly detection | Adds complexity without validating the core hypothesis first. |
| NG8 | Export/import from Google Sheets | Manual migration is acceptable for MVP validation. |

---

## 6. Functional Requirements Overview

### FR1: Monthly Budget Setup
The user can define a monthly budget consisting of:
- **Income sources** — Name and value of each source of money (e.g., paycheck, bank account balances).
- **Fixed expenses** — Recurring expenses assigned to a specific day of the month (e.g., rent on day 1, insurance on day 5).
- **Subscription expenses** — Recurring subscriptions with their billing day and amount.
- **Variable expenses** — Expected expenses assigned to a specific day (e.g., electricity on day 19) or spread across the month (e.g., groceries, online shopping).
- **Spread expenses** — Expenses not tied to a single day but distributed evenly across the month's daily simulation (currently marked as "TRUE" / daily expense in the sheet).
- **Temporarily excluded expenses** — Ability to define an expense but exclude it from the current month's calculation (equivalent to moving the day to Column C in the current sheet), preserving it for future months.

### FR2: Day-by-Day Forecast Simulation
The system generates a daily simulation from day 0 (starting balance = total income) through day 31 (or last day of month), calculating:
- **Remaining balance** for each day: previous day's remaining balance minus that day's total expenses (only if the current date has not yet passed that day).
- **Daily expenses**: sum of all expenses assigned to that day, plus the daily portion of spread expenses.
- The final row shows the **forecasted end-of-month remaining balance**.

### FR3: Forecast Visualization
A visual chart displays the forecast curve showing remaining balance over the days of the month. The chart must clearly show:
- The forecasted balance trajectory (line/area chart).
- Key inflection points where large expenses occur.
- When re-forecasts exist: overlay of original forecast vs. re-forecast on the same chart.

### FR4: Forecast Snapshots
At any point during the month, the user can **save a snapshot** of the current forecast state. A snapshot captures:
- The date it was taken.
- The full day-by-day forecast as calculated at that moment.
- The actual bank balance entered by the user at that point.
This snapshot is immutable and preserved for later comparison.

### FR5: Non-Destructive Mid-Month Re-Forecasting
The user can, at any point mid-month:
1. Enter their **actual current bank balance**.
2. Optionally adjust future expenses (add, remove, or change amounts).
3. Generate a **new forecast** from the current day forward using the actual balance as the new starting point.
4. The **original forecast is preserved** as a baseline snapshot.
5. Multiple re-forecasts can coexist for the same month.

### FR6: Forecast Comparison & Drift Analysis
The user can compare any two forecast versions (original vs. re-forecast, or any two snapshots) and see:
- A side-by-side or overlay visualization of the two forecast curves.
- The **variance** (difference) between the two forecasts at each day.
- Identification of **which specific expenses** caused the drift — i.e., expenses that were added, removed, or changed between the two versions.

### FR7: Month Rollover with Expense Carry-Forward
When starting a new month, the system:
- Carries forward all recurring expenses (fixed, subscriptions) from the previous month with their existing values and assigned days.
- Clears variable/one-time expenses.
- Prompts the user to review and adjust values as needed (e.g., updated subscription prices, changed income).
- Allows quick start: the user only needs to update what changed, not rebuild from scratch.

### FR8: Household Shared Access
Two users can access the same monthly budget:
- Both can view the forecast, charts, and snapshots.
- Both can add or update expenses.
- The interface must be simple enough for a non-technical user to navigate without guidance.

### FR9: Simple, Intuitive Interface
The interface must prioritize:
- **At-a-glance status**: the user sees the current forecasted end-of-month balance and a visual chart immediately upon opening.
- **Clear expense list**: expenses are displayed in a readable list/table grouped by type (fixed, subscriptions, variable).
- **Easy editing**: adding, editing, or removing an expense requires minimal steps.
- **Non-technical language**: no formulas, cell references, or spreadsheet concepts are exposed.

---

# User Stories

### Epic 1: Monthly Budget Setup

**US-1.1: Define Income Sources**
As a **Budget Owner**, I want to add income sources with their name and value, so that the system knows my total available funds for the month.

*Acceptance Criteria:*
- The user can add one or more income sources, each with a name (text) and a value (number in EUR).
- The user can edit or remove any income source.
- The total income is displayed as a sum of all income source values.
- At least one income source must exist before a forecast can be generated.

---

**US-1.2: Define Fixed Expenses**
As a **Budget Owner**, I want to add fixed monthly expenses with their name, expected day, and amount, so that they appear on the correct day in the forecast simulation.

*Acceptance Criteria:*
- The user can add an expense with a name, day of month (1–31), and value in EUR.
- The user can mark an expense as a "spread" expense (distributed daily) instead of a specific-day expense.
- The expense appears in the expense list grouped under its type.
- The user can edit or remove any expense.

---

**US-1.3: Temporarily Exclude an Expense**
As a **Budget Owner**, I want to temporarily exclude an expense from this month's calculation without deleting it, so that I can keep it for future months when it will apply.

*Acceptance Criteria:*
- The user can toggle an expense as "excluded this month."
- An excluded expense is visually distinct in the list (e.g., greyed out, strikethrough).
- An excluded expense is not included in the forecast simulation.
- The excluded expense is still visible in the expense list for re-inclusion.

---

**US-1.4: Categorize Expenses by Type**
As a **Budget Owner**, I want to organize expenses by type (Fixed, Subscriptions, Variable), so that I can quickly find and manage them.

*Acceptance Criteria:*
- Expenses can be assigned to one of three categories: Fixed, Subscriptions, Variable.
- The expense list displays expenses grouped by category with clear section headers.
- The user can change an expense's category after creation.

---

### Epic 2: Forecast Simulation

**US-2.1: Generate Day-by-Day Forecast**
As a **Budget Owner**, I want to generate a day-by-day forecast of my remaining balance for the current month, so that I can see how my balance changes over the month.

*Acceptance Criteria:*
- Upon completing budget setup (at least one income source and one expense), the user can generate a forecast.
- The forecast displays a row for each day of the month (day 0 through last day of month).
- Day 0 shows the total income as the starting balance.
- Each subsequent day shows: the remaining balance, and the total expenses on that day.
- Expenses on days that have already passed (before today) are deducted from the balance.
- The last day shows the forecasted end-of-month remaining balance.

---

**US-2.2: View Spread Expenses in Daily Simulation**
As a **Budget Owner**, I want spread expenses to be evenly distributed across all days of the month in the simulation, so that the daily forecast reflects their gradual impact.

*Acceptance Criteria:*
- Spread expenses are divided by the number of days in the current month.
- The daily portion is added to each day's total expense in the simulation.
- The full amount of the spread expense is reflected across the month.

---

### Epic 3: Forecast Visualization

**US-3.1: View Forecast Chart**
As a **Household Partner**, I want to see a visual chart of the forecasted remaining balance over the month, so that I can instantly understand our financial trajectory without reading numbers.

*Acceptance Criteria:*
- A line or area chart is displayed showing remaining balance (Y-axis) vs. day of month (X-axis).
- The chart clearly shows the starting balance and the end-of-month forecasted balance.
- Days with large expense drops are visually identifiable.
- The chart is visible on the main/home screen of the tool.

---

**US-3.2: Overlay Multiple Forecasts on Chart**
As a **Budget Owner**, I want to see the original forecast and re-forecasts overlaid on the same chart, so that I can visually compare how my plan evolved.

*Acceptance Criteria:*
- When multiple forecast versions exist for the same month, they are shown as separate lines on the chart.
- Each forecast line is visually distinct (different color or style) with a legend.
- The user can toggle individual forecast lines on or off.

---

### Epic 4: Snapshots & Re-Forecasting

**US-4.1: Save a Forecast Snapshot**
As a **Budget Owner**, I want to save a snapshot of my current forecast at any point in the month, so that I have a preserved baseline to compare against later.

*Acceptance Criteria:*
- The user can trigger a "Save Snapshot" action.
- The snapshot records: the date taken, the full day-by-day forecast data, and an optional user-entered actual bank balance.
- The snapshot is immutable — it cannot be edited after creation.
- All saved snapshots for the current month are listed and accessible.

---

**US-4.2: Re-Forecast from Actual Balance**
As a **Budget Owner**, I want to enter my actual bank balance mid-month and generate a new forecast from that day forward, so that my forecast reflects reality without losing my original plan.

*Acceptance Criteria:*
- The user can enter their actual bank balance and the date.
- The system generates a new forecast starting from that date using the entered balance as the starting point.
- Days before the entered date retain their values from the original forecast.
- The original forecast is automatically preserved as a snapshot if not already saved.
- The new re-forecast appears in the forecast list and on the chart.

---

**US-4.3: Adjust Future Expenses During Re-Forecast**
As a **Budget Owner**, I want to adjust, add, or remove future expenses when I re-forecast, so that the new forecast reflects updated plans.

*Acceptance Criteria:*
- During the re-forecast flow, the user sees upcoming expenses (from the re-forecast date onward).
- The user can modify amounts, add new expenses, or remove planned expenses.
- Changes only affect the new re-forecast; the original forecast and its expenses remain unchanged.

---

### Epic 5: Drift Analysis

**US-5.1: Compare Two Forecast Versions**
As a **Budget Owner**, I want to compare any two forecast versions side by side, so that I can see the variance between them.

*Acceptance Criteria:*
- The user can select two forecast versions (snapshots or re-forecasts) for comparison.
- The comparison shows the remaining balance for each version on each day and the difference (variance).
- The comparison is viewable both as a data table and as an overlay chart.

---

**US-5.2: Identify Expenses Causing Drift**
As a **Budget Owner**, I want to see which specific expenses changed between two forecast versions, so that I understand why the forecast drifted.

*Acceptance Criteria:*
- When comparing two forecast versions, the system highlights expenses that were added, removed, or changed in value.
- Each changed expense shows the old value, new value, and the difference.
- The total impact of all changes is summarized.

---

### Epic 6: Month Rollover

**US-6.1: Start a New Month with Carry-Forward**
As a **Budget Owner**, I want recurring expenses to automatically carry forward to the next month, so that I don't have to re-enter everything from scratch.

*Acceptance Criteria:*
- When starting a new month, all fixed and subscription expenses from the previous month are pre-populated with their existing names, days, and values.
- Variable/one-time expenses are not carried forward.
- Previously excluded expenses remain in the list but stay excluded.
- The user is prompted to review and adjust the carried-forward data before generating the first forecast.

---

**US-6.2: Quickly Adjust Carried-Forward Expenses**
As a **Budget Owner**, I want to quickly update amounts or toggle expenses on/off when starting a new month, so that setup takes minutes, not an hour.

*Acceptance Criteria:*
- The review screen shows all carried-forward expenses in an editable list.
- The user can inline-edit values, toggle exclusion, or remove expenses.
- The user can add new expenses during this review.
- A single action finalizes the setup and generates the month's first forecast.

---

### Epic 7: Household Shared Access

**US-7.1: Invite a Household Member**
As a **Budget Owner**, I want to invite my partner to access our shared monthly budget, so that we can both stay informed.

*Acceptance Criteria:*
- The Budget Owner can send an invitation (via email or link) to another person.
- The invited person can create an account and access the shared budget.
- Only one invitation per household budget is allowed in MVP.

---

**US-7.2: View and Update Budget as Household Partner**
As a **Household Partner**, I want to view the forecast, charts, and expense list, and add or update an expense, so that I can participate in our financial planning without needing technical help.

*Acceptance Criteria:*
- The Household Partner sees the same forecast, chart, and expense list as the Budget Owner.
- The Household Partner can add a new expense or edit an existing expense's value.
- The interface uses plain language and requires no knowledge of formulas or spreadsheet concepts.
- Changes made by either user are immediately visible to the other.

---

### Epic 8: At-a-Glance Dashboard

**US-8.1: See Financial Status Immediately**
As a **Household Partner**, I want to see the forecasted end-of-month balance and the forecast chart as soon as I open the tool, so that I can do a quick checkpoint without navigating.

*Acceptance Criteria:*
- The home/landing screen displays: the current forecasted end-of-month remaining balance (prominently), the forecast chart, and today's date with today's remaining balance.
- If the forecasted end-of-month balance is negative, it is highlighted with a warning indicator.
- No navigation or clicks are needed to see this information.
