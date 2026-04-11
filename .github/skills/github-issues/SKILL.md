---
name: github-issues
description: "Create GitHub issues using standardized templates. Covers template selection, enrichment from project docs, duplicate detection, label assignment, and HITL confirmation. Used by any agent that needs to create issues (Planner, Implementor, Architect, Reviewer)."
user-invocable: false
---

# GitHub Issues Skill

This skill provides a standardized workflow for creating GitHub issues on `g-nogueira/BudgedManager`. Any agent that needs to create issues should follow this skill instead of ad-hoc issue creation.

## Issue Templates

Issue templates are in [`.github/ISSUE_TEMPLATE/`](../../../.github/ISSUE_TEMPLATE/). Always use the appropriate template:

| Template | When to use |
|---|---|
| [feature.md](../../../.github/ISSUE_TEMPLATE/feature.md) | New functionality from PRD, user story, or planning decomposition |
| [bug.md](../../../.github/ISSUE_TEMPLATE/bug.md) | Defect found during review, testing, or development |
| [tech-debt.md](../../../.github/ISSUE_TEMPLATE/tech-debt.md) | Architecture improvement, cleanup, refactoring — from design review or code review |
| [spike.md](../../../.github/ISSUE_TEMPLATE/spike.md) | Timeboxed investigation with defined research questions |

## Label Conventions

Apply labels from these categories:

**Bounded Context** (at least one required):
- `budget-management` — MonthlyBudget aggregate, income, expenses, rollover
- `forecast-engine` — Simulation, snapshots, re-forecast, drift
- `identity-household` — Users, JWT auth, household management
- `shared-kernel` — SharedKernel types, cross-cutting infrastructure
- `frontend` — SvelteKit UI, components, stores, API clients

**Type** (exactly one required):
- `feature` — New functionality
- `bug` — Defect
- `tech-debt` — Refactor or improvement
- `spike` — Research / investigation

**Priority** (for features, from PRD):
- `priority:must-have`
- `priority:should-have`
- `priority:nice-to-have`

**Source** (for traceability):
- `user-story` — Originated from PRD user story
- `design-gap` — Originated from architecture design review
- `review-finding` — Originated from PR review

Create labels if they don't exist yet.

## Issue Creation Workflow

Follow this workflow regardless of which agent is creating the issue.

### Step 1: Select Template

Based on the context, pick the right template:
- **Planner decomposing user stories** → `feature.md`
- **Architect finding design gaps** → `tech-debt.md` or `feature.md` (depending on whether it's a gap or new work)
- **Reviewer finding follow-up issues** → `bug.md` or `tech-debt.md`
- **Implementor discovering scope** → `bug.md`, `tech-debt.md`, or `spike.md`

### Step 2: Read the Template

Read the selected template file from `.github/ISSUE_TEMPLATE/`. Fill in every section — leave no `<placeholder>` text in the final issue.

### Step 3: Enrich with Project Context

Before creating the issue, read relevant project docs to fill in technical details:
- `docs/arch/domain-invariants.md` — for invariant references (INV-B*, INV-F*, INV-H*)
- `docs/arch/api-contracts.md` — for endpoint references
- `docs/arch/persistence-conventions.md` — for persistence details (when relevant)
- `docs/product/` files — for PRD/user story references (if applicable)

### Step 3b: Verify Context Quality

Before proceeding, check that the issue body you're about to create meets the context quality bar defined in the **task-context** skill (`.github/skills/task-context/SKILL.md`, section "Context Quality for Issue Writing"). A well-written issue should contain enough information that a future agent can gather complete context from the issue alone — without needing to ask the user for clarification.

Verify the issue body contains:
- Clear description of what needs to be done
- Acceptance criteria (specific, testable)
- Affected bounded context(s)
- Relevant invariant references (if applicable)
- References to relevant architecture doc sections

### Step 4: Search for Duplicates

Before creating any issue, search the repository for existing issues that may already cover the same topic:
```
Search by title keywords and any reference IDs (US-X, GAP-N, RP-X)
```
If a duplicate or near-duplicate exists, do NOT create a new issue. Instead, note the existing issue number and inform the user.

### Step 5: Present Issue Plan (HITL Gate)

Before creating issues, present a summary table to the user via `vscode/askQuestions`:

| # | Title | Template | Bounded Context | Labels |
|---|-------|----------|-----------------|--------|
| 1 | ... | feature | budget-management | `feature`, `budget-management`, `priority:must-have` |

**Wait for explicit confirmation.** Do not create issues without user approval.

### Step 6: Create Issues

Use GitHub API tools to create each confirmed issue on `g-nogueira/BudgedManager`:
1. Set the title
2. Set the body (filled template)
3. Apply labels (create missing labels first)
4. Add to GitHub Project #6
5. Do **NOT** assign users — leave for human triage

### Step 7: Report Created Issues

After creation, present a summary:

| # | Issue | Title | URL |
|---|-------|-------|-----|
| 1 | #NN | ... | link |

Log cross-team events to `.github/agents/activity-log.md` if creating issues as part of a workflow handoff.

## Batch Issue Creation

When creating multiple issues (e.g., Planner decomposing a feature into 5+ tasks):
1. Prepare ALL issues first (Steps 1-4 for each)
2. Present the full batch in a single HITL confirmation (Step 5)
3. Create all confirmed issues in sequence (Step 6)
4. Present the full summary once at the end (Step 7)

## Issue Title Conventions

- **Feature from user story:** `[US-X] <user story title>`
- **Design gap:** `[GAP-N] <concise gap summary>`
- **Bug from review:** `[RP-X] <defect description>`
- **Tech debt:** `<concise description of the improvement>`
- **Spike:** `[Spike] <research question>`

## Rules

- **Every issue must trace to a source** — PRD, design gap doc, review point, or explicit user request
- **Copy acceptance criteria verbatim** when they come from a PRD — never paraphrase
- **Never assign issues** — leave for human triage
- **Always HITL before creation** — no exceptions
- **Search for duplicates first** — never create duplicate issues
