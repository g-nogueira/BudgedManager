---
name: task-context
description: "Gather and verify issue/task context from GitHub. Two modes: Primary Gather (full fetch + doc enrichment + write memory file) and Completeness Check (verify existing context is sufficient, fill gaps). Used by any agent that works on or creates issues."
user-invocable: true
---

# Task Context Skill

This skill provides a standardized workflow for gathering GitHub issue context and writing structured memory files that downstream agents consume. Any agent that works on an issue — or creates issues — should use this skill to ensure context is complete and accurate.

## Two Modes of Operation

### Mode 1: Primary Gather

Use when **starting work on an issue** and no `task-context-<issue-number>.md` memory file exists yet. This mode fetches the full issue from GitHub, enriches it with project docs, and writes the structured memory file.

**Typical consumers:** Backend Planner, Frontend Planner (Workflow 1, first step), any agent asked to work on an issue directly.

### Mode 2: Completeness Check

Use when a memory file **already exists** (written by a prior agent or by Mode 1). This mode evaluates the existing context against the completeness checklist and fills gaps.

**Typical consumers:** Backend Implementor, Frontend Implementor (before coding), Backend Reviewer, Frontend Reviewer (before reviewing), any agent that needs to verify context quality before proceeding.

## Mode 1: Primary Gather — Full Context Fetch

Follow these steps when gathering fresh context for an issue.

### Step 1: Fetch the Issue

Use GitHub tools to fetch the issue from `g-nogueira/BudgedManager`.

Extract:
- Issue number
- Title
- Full body (verbatim)
- Labels
- Acceptance criteria (from body — verbatim, not summarized)
- Any referenced issues or epics

### Step 2: Fetch Parent Epic (if any)

If the issue is a sub-issue or references a parent epic:
- Fetch the epic
- Extract the epic's title, body, and completion status

### Step 3: Fetch Sub-Issues (if any)

If the issue has sub-issues or a task list:
- Fetch each sub-issue
- Record their completion status (open/closed)

### Step 4: Read Project Context

Based on the bounded context identified from the issue, read the relevant docs:

1. **Identify the bounded context** from:
   - Issue labels (e.g., `budget-management`, `forecast-engine`, `identity-household`)
   - Entity names mentioned in the issue (e.g., "MonthlyBudget" → Budget Management)
   - Endpoint paths (e.g., `/api/v1/budgets/` → Budget Management)

2. **Read `docs/arch/domain-invariants.md`** — find invariants relevant to the identified bounded context / aggregate

3. **Read `docs/arch/api-contracts.md`** — find endpoints relevant to the issue

4. **Read `docs/arch/persistence-conventions.md`** — if the issue involves data model changes

5. **Identify cross-context interactions** — if the issue involves cross-context communication, note the event names and ACL boundaries

### Step 5: Scan Codebase for Existing State

Search the codebase to understand what relevant code already exists:
- Search for files in affected bounded context (`src/Modules/<Context>/`)
- Check for existing implementations that overlap with the issue
- Note any test files that already cover related functionality

### Step 6: Write Memory File

Create the memory file at: `.github/agents/memory/active/task-context-<issue-number>.md`

The memory file MUST follow this exact structure:

```markdown
# Issue Context — #<number>: <title>

## Issue Details
- **Number:** #<number>
- **Title:** <title>
- **Labels:** <label1>, <label2>
- **Status:** Open / Closed

## Parent Epic
- **Number:** #<number> (or "None")
- **Title:** <title>
- **Completion:** X of Y sub-issues completed

## Issue Body (verbatim)
<Paste the full issue body here exactly as written>

## Acceptance Criteria (verbatim)
- [ ] AC1: <exact text from issue>
- [ ] AC2: <exact text from issue>

## Sub-Issues
| # | Title | Status |
|---|---|---|
| <number> | <title> | Open/Closed |

## Architecture Context

### Bounded Context
<context name> — <brief role description>

### Relevant Invariants
| ID | Rule | Aggregate |
|---|---|---|
| INV-B3 | <exact rule text from domain-invariants.md> | MonthlyBudget |

### Relevant API Endpoints
| Method | Path | Request | Response | Status Code |
|---|---|---|---|---|
| POST | /api/v1/budgets/{id}/expenses | CreateExpenseRequest | ExpenseResponse | 201 |

### Relevant Domain Events
| Event | Published By | Consumed By |
|---|---|---|
| ExpenseAdded | MonthlyBudget | ForecastEngine (marks stale) |

### Cross-Context Interactions
<Description of any ACL or event-based communication needed, or "None">

## Completion Status
<What sibling sub-issues are already done vs pending — helps scope correctly>

## Codebase Snapshot
<Brief note of what relevant code already exists, verified via search tools>

## Decisions Made
<Any clarifications obtained from the user during context gathering>
```

### Step 7: Run Completeness Checklist

After writing the memory file, evaluate it against the completeness checklist (see below). If any item fails, fill the gap before finishing.

## Mode 2: Completeness Check — Verify Existing Context

Follow these steps when a `task-context-<issue-number>.md` file already exists and you need to verify it's sufficient for your work.

### Step 1: Read the Memory File

Read `.github/agents/memory/active/task-context-<issue-number>.md` in full.

### Step 2: Run Completeness Checklist

Evaluate each item. If an item fails, fetch the missing information.

### Step 3: Append Any New Context

If you fetched additional information, append it to the existing memory file under a new section:

```markdown
## Additional Context (added by <agent-name>)
<New information gathered during completeness check>
```

Do NOT overwrite existing sections — only append.

## Completeness Checklist

Evaluate the following before proceeding with any downstream work. Every item must be **Yes** or **N/A** (with justification for N/A).

| # | Check | How to verify |
|---|-------|---------------|
| 1 | **Issue body is present and verbatim** | The `## Issue Body (verbatim)` section is non-empty and matches the GitHub issue |
| 2 | **Acceptance criteria are listed verbatim** | The `## Acceptance Criteria (verbatim)` section contains copied ACs, not summaries |
| 3 | **Bounded context is identified** | The `## Architecture Context` section lists the bounded context |
| 4 | **Relevant invariants are listed (if domain issue)** | If the issue touches domain logic, the `## Relevant Invariants` table has entries from `docs/arch/domain-invariants.md` |
| 5 | **API endpoints are listed (if API issue)** | If the issue involves endpoints, the `## Relevant API Endpoints` table has entries from `docs/arch/api-contracts.md` |
| 6 | **Codebase snapshot is populated** | The `## Codebase Snapshot` section describes existing relevant files (from search, not from memory) |
| 7 | **Parent/sub-issue context is present (if applicable)** | If the issue is part of an epic, the `## Parent Epic` and `## Sub-Issues` sections are populated |
| 8 | **No placeholder text remains** | No `<placeholder>` or `TODO` markers in the memory file |

If any check fails:
1. Fetch the missing information (from GitHub, architecture docs, or codebase search)
2. Update the memory file
3. Re-run the failing check to confirm it passes

If a check genuinely doesn't apply (e.g., no domain invariants for a frontend-only issue), mark it as **N/A** with a one-line justification.

## Context Quality for Issue Writing

When an agent is about to **create** an issue (via the `github-issues` skill), it should use this skill's completeness checklist as a quality bar for what information to include in the issue body. A well-written issue should contain enough information that a future Primary Gather (Mode 1) can populate a complete memory file without needing to ask the user for clarification.

**Before creating an issue, verify the issue body contains:**
- Clear description of what needs to be done
- Acceptance criteria (specific, testable)
- Affected bounded context(s)
- Relevant invariant references (if applicable)
- References to relevant architecture doc sections
- Context about what already exists in the codebase (if known)

This ensures downstream agents (Planner, Implementor) can start working immediately from the issue alone.

## Rules

- **Verbatim copy is mandatory** for acceptance criteria and issue body — never paraphrase
- **All sections must be filled** — use "N/A" or "None" for non-applicable sections, never leave blank
- **Architecture references must be verified** — always read the focused architecture extracts, never cite from memory
- **The memory file is the communication channel** to downstream agents — if it's not in the file, they won't know about it
- **Never overwrite existing context** — in Mode 2, only append new information
- **Log cross-team events** — after writing or updating a memory file, append an entry to `.github/agents/activity-log.md`
