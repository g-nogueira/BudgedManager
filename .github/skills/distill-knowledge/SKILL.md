---
name: distill-knowledge
description: "Compress completed issue memory files into reusable knowledge. Reads all Tier 1 (active) memory files for an issue, extracts decisions, patterns, conventions, and gotchas, writes atomic facts to knowledge.md, and archives raw files to Tier 3. Use after an issue is merged/closed or when the user asks to distill learnings."
user-invocable: true
---

# Distill Knowledge Skill

This skill compresses verbose Tier 1 memory files into reusable Tier 2 knowledge after an issue is completed. It also archives the raw files to Tier 3.

**Trigger:** Invoke after an issue is merged/closed, or when the user explicitly asks to distill learnings. Can also be invoked periodically to clean up accumulated active memory files.

## Step 1: Identify the Issue

If the issue number isn't provided, check:
```powershell
# List active memory files
Get-ChildItem .github/agents/memory/active/ -Name
```

Ask the user which issue to distill if multiple are present.

## Step 2: Read All Tier 1 Memory Files

Read every memory file for the issue:

| File | Agent | Key content to extract |
|---|---|---|
| `task-context-<N>.md` | Task Context Skill | Issue scope, acceptance criteria, bounded context mapping |
| `plan-<N>.md` | Planner | Implementation approach, file structure decisions, open questions resolved |
| `implementation-<N>.md` | Implementor | Patterns discovered, conventions learned, gotchas encountered |
| `code-reviewer-<N>.md` | Reviewer | Review findings, common mistakes, quality patterns |

Not all files will exist for every issue. Read what's available.

## Step 3: Read the Learnings Sections

Each agent appends a `## Learnings` section to its memory file during work. These are your primary extraction source. Look for:

- **Decisions made** — choices between alternatives, with rationale
- **Patterns discovered** — codebase conventions, naming patterns, file organization
- **Conventions confirmed** — things that worked and should be repeated
- **Gotchas encountered** — surprising behavior, workarounds, things that didn't work
- **Domain insights** — invariant edge cases, cross-context communication patterns
- **Tooling/build insights** — dotnet/pnpm tricks, test patterns that worked

## Step 4: Deduplicate Against Existing Knowledge

Read `.github/agents/memory/knowledge.md` and check if any extracted facts already exist. Skip duplicates. If an existing entry needs updating (new nuance discovered), update it rather than adding a duplicate.

## Step 5: Write to knowledge.md

Append new facts to `.github/agents/memory/knowledge.md` using this format:

```markdown
## <Category>

### <Date> — <Short title> (from #<issue>)
- <Atomic fact 1>
- <Atomic fact 2>
```

**Categories** (use existing ones, create new only if needed):

| Category | What goes here |
|---|---|
| `Architecture Decisions` | Bounded context design choices, hexagonal layer trade-offs |
| `Domain Invariants` | Invariant edge cases, enforcement patterns, gotchas |
| `Codebase Conventions` | Naming patterns, file organization, namespace structure |
| `Build & Tooling` | dotnet/pnpm configs, migration tricks, test patterns |
| `API Patterns` | Controller patterns, error response format, auth integration |
| `Persistence Patterns` | EF Core config patterns, schema rules, migration learnings |
| `Cross-Context Communication` | MediatR event patterns, ACL insights, event handler conventions |
| `Review Findings` | Common mistakes, quality patterns, things reviewers should watch for |
| `Frontend Patterns` | SvelteKit patterns, store conventions, component patterns |
| `Gotchas` | Surprising behavior, workarounds, things that look right but aren't |

**Rules for writing facts:**
- **Atomic:** One fact per bullet point — no compound sentences
- **Actionable:** Write as instructions, not observations ("Use X" not "We found that X")
- **Source-linked:** Include issue number for traceability
- **No narratives:** Delete context, keep only the reusable insight

**Example:**
```markdown
## Domain Invariants

### 2026-04-15 — Expense day validation edge case (from #42)
- INV-B3 validation must use `DateTime.DaysInMonth(year, month)` not hardcoded 31 — February budgets fail otherwise
- Spread expenses (INV-B4) must explicitly set `dayOfMonth = null` in the aggregate, not just omit it from the command

## Codebase Conventions

### 2026-04-15 — Handler registration (from #42)
- New MediatR handlers must be registered in `ServiceCollectionExtensions.AddInfrastructure()` — auto-discovery is not enabled
- Validator classes must follow `<CommandName>Validator.cs` naming in the same folder as the command
```

## Step 6: Archive Tier 1 Files

Move all memory files for the issue from `active/` to `archive/`:

```powershell
Move-Item .github/agents/memory/active/*-<issue-number>.md .github/agents/memory/archive/
```

## Step 7: Report

Present a summary to the user:

```markdown
## Distillation Summary — Issue #<N>

**Files processed:** <count>
**Facts extracted:** <count>
**Facts deduplicated (skipped):** <count>
**Facts written to knowledge.md:** <count>
**Files archived:** <list>
```

## Lightweight Inline Distillation (For Working Agents)

Working agents don't run this full skill. Instead, they append a `## Learnings` section to their own memory file at the end of their workflow. This section is the raw material this skill later compresses.

**Format agents should use in their memory files:**

```markdown
## Learnings

### Decisions
- <decision>: <rationale>

### Patterns
- <pattern observed>

### Gotchas
- <thing that was surprising or didn't work as expected>
```

Agents should write facts here as they work — not batch them at the end. If no learnings were generated, write `## Learnings\nNone.` to confirm the section was considered.

## When knowledge.md Gets Too Large

If `knowledge.md` exceeds ~200 lines, consider:
1. Consolidating related entries under fewer headings
2. Removing entries that are now obvious from the codebase itself (e.g., naming conventions that are self-evident from existing files)
3. Moving rarely-referenced categories to a separate file (e.g., `knowledge-persistence.md`)
