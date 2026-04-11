# AI Agent Setup Guide — MonthlyBudget

> How the MonthlyBudget project uses VS Code GitHub Copilot custom agents as a full AI-driven development team. This document explains the design decisions, coordination mechanisms, and strategies for overcoming common AI limitations.

---

## 1. Overview

The MonthlyBudget project uses **12 custom Copilot agents** organized into a multi-agent pipeline that covers the full software development lifecycle: product discovery, architecture, planning, implementation, and code review. Each agent is a `.agent.md` file in `.github/agents/` with a focused role, a scoped tool set, and structured handoff instructions.

The agents operate as a **virtual engineering team** coordinated through durable file artifacts rather than ephemeral chat context. A human developer orchestrates the pipeline by triggering agents in sequence, reviewing outputs, and approving handoffs.

### The Team

| Agent | File | Role |
|---|---|---|
| Product Manager | `product-manager.agent.md` | Defines What & Why — PRDs, user stories, JTBD methodology |
| UI Designer | `ui-designer.agent.md` | Generates UI prototypes via Google Stitch MCP |
| Software Architect | `software-architect.agent.md` | Architecture decisions, bounded contexts, API contracts, ADRs |
| Issue Writer | `issue-writer.agent.md` | Converts architecture artifacts into GitHub issues |
| Issue Reader | `issue-reader.agent.md` | Fetches GitHub issue context, writes structured memory |
| Backend Planner | `backend-planner.agent.md` | Analyzes .NET codebase, produces file-level implementation plans |
| Backend Implementor | `backend-implementor.agent.md` | Executes plans: writes C# code + tests, commits, opens PRs |
| Backend Reviewer | `backend-reviewer.agent.md` | Reviews PRs against architecture, invariants, contracts |
| Frontend Planner | `frontend-planner.agent.md` | Analyzes SvelteKit codebase, plans frontend features |
| Frontend Implementor | `frontend-implementor.agent.md` | Writes SvelteKit/TypeScript code, opens PRs |
| Frontend Reviewer | `frontend-reviewer.agent.md` | Reviews frontend PRs against API contracts and patterns |
| Agent Improver | `agent-improver.agent.md` | Meta-agent: improves the other agents via structured retrospectives |

---

## 2. How Agents Work Independently

Each agent is designed to operate in isolation within a single VS Code Copilot session. Several mechanisms make this possible:

### 2.1 Focused Role Scoping

Every agent has a narrow, well-defined responsibility. The Backend Planner only plans — it never writes code. The Backend Implementor only executes plans — it never decides what to build. This prevents scope confusion and keeps each agent's context window focused.

The agent file's YAML frontmatter enforces this:

```yaml
name: Backend Planner
description: "Reads issue context from memory, analyzes the .NET codebase, and produces a precise file-level implementation plan."
tools: ['search', 'read', 'execute', 'edit/createFile', 'todo', 'vscode/askQuestions']
```

Note the tool list: the Planner has `edit/createFile` (to write plan files) but not `github/*` (to interact with PRs). The Implementor gets `github/*` because it opens PRs.

### 2.2 Context Loading Priority

Each agent has an ordered list of files to read, loaded on demand (not pre-loaded). This prevents context window bloat and ensures the agent reads the right information:

```markdown
## Context Loading Priority
1. **ALWAYS read first:** `.github/agents/memory/active/plan-<issue-number>.md`
2. **Read before writing any code:** `.github/agents/context/<context>-patterns.md`
3. **Read ON DEMAND:** Skill files — only when executing that specific step
```

The critical design choice is **lazy loading** — agents only read architecture extracts when they reach the step that needs them, not upfront.

### 2.3 Pre-flight Checks

Every agent starts with a verification step before doing any work:

```markdown
## Pre-flight Check
Before starting ANY work, verify:
1. The plan memory file exists and is non-empty
2. The plan contains: issue number, bounded context, branch name, at least one feature group
3. `git status` shows a clean working tree
4. `dotnet build` passes on the current state

If any check fails, STOP and ask the user.
```

This catches common failure modes early: missing inputs, broken build state, wrong branch.

### 2.4 Self-Verification Checkpoints

Implementor agents run verification before every commit:

1. Every new `using` statement references a namespace that actually exists
2. Every interface has an implementation registered in DI
3. Every exception thrown has a matching test
4. No files were created that aren't in the plan

This catches hallucination-induced errors before they enter version control.

### 2.5 Skills — Reusable Procedural Knowledge

Complex workflows are packaged as **skill files** (`.github/skills/*/SKILL.md`) that multiple agents reference but only read when they reach the relevant step:

| Skill | Used By | Purpose |
|---|---|---|
| `dotnet-tdd` | Backend Implementor, Reviewer | Build, test, migration commands |
| `api-exercise` | Backend Implementor, Reviewer | Start API, exercise endpoints |
| `hexagonal-validation` | Planner, Implementor, Reviewer, Architect | Check layer purity |
| `additive-review` | Backend & Frontend Reviewers | Multi-round PR review protocol |
| `sveltekit-dev` | Frontend Implementor, Reviewer | Build, lint, test commands |
| `resume` | Any agent | Resume interrupted work from memory |

Skills prevent duplication of procedural instructions across agents while keeping each agent file lean.

---

## 3. How Agents Work Together

### 3.1 Artifact-Driven Handoffs

Agents never rely on prompt text to pass context. Every handoff goes through a **durable file artifact** that the next agent reads from disk:

```
Issue Reader → writes issue-reader-75.md → Backend Planner reads it
Backend Planner → writes plan-75.md → Backend Implementor reads it
Backend Implementor → writes implementation-75.md → Backend Reviewer reads it
Backend Reviewer → writes code-reviewer-75.md → Backend Planner reads it (fix cycle)
```

This solves two problems:
- **Session boundaries:** If a session ends mid-pipeline, the next session can resume by reading the last artifact written (the `resume` skill automates this)
- **Fidelity:** File artifacts preserve exact text (acceptance criteria, invariant IDs, file paths) without the paraphrasing that happens when context passes through prompt chains

### 3.2 Structured Memory File Templates

Each memory file follows an **exact Markdown template** so the consuming agent knows where to find each piece of information. For example, the Issue Reader produces:

```markdown
# Issue Context — #75: Add Expense Validation

## Acceptance Criteria (verbatim)
- [ ] AC1: Given a spread expense, when dayOfMonth is set, then return 400

## Relevant Invariants
| ID | Rule | Aggregate |
|---|---|---|
| INV-B4 | addExpense() spread → dayOfMonth must be null | MonthlyBudget |
```

The Planner knows the acceptance criteria are always under `## Acceptance Criteria (verbatim)` and invariants are in the `## Relevant Invariants` table. This structured contract eliminates ambiguity.

### 3.3 Handoff Definitions in YAML Frontmatter

Each agent declares its available handoffs:

```yaml
handoffs:
  - label: "Hand off to Backend Implementor"
    agent: Backend Implementor
    prompt: "Implementation plan has been written to memory. Read the plan and execute it."
    send: false
```

The `send: false` flag means the user must manually trigger the handoff — agents never autonomously spawn other agents. This keeps the human in control.

### 3.4 Defined Pipelines

Five pipelines cover different development phases:

| Pipeline | Flow | When to Use |
|---|---|---|
| Product Discovery | PM → UI Designer → Architect → PM | Aligning product scope before implementation |
| Feature Implementation | Issue Reader → Planner → Implementor | Executing scoped engineering work |
| PR Review & Fix | Reviewer → Planner → Implementor | Additive review rounds |
| Project Startup | PM → Designer → Architect → Issue Writer | Going from zero to trackable work |
| Design Review | Architect → Issue Writer | Designs arrive after implementation started |

### 3.5 Activity Log — Team Awareness

All agents share an append-only activity log (`.github/agents/activity-log.md`) that functions as a standup board:

```markdown
**2026-04-05 — Backend Reviewer:** Completed review of PR #42 for issue #74.
3 CRITICAL findings, 2 WARNINGs. Memory: `code-reviewer-74.md`.
```

Every agent reads this on startup (quick scan) so it has situational awareness of recent team events — issues created, PRs opened, reviews completed — even if it wasn't directly involved.

### 3.6 Input Validation at Every Boundary

A critical lesson from multi-agent systems: **every agent must validate its inputs**, not blindly trust the upstream agent. The Implementor doesn't just execute the Planner's file list — it verifies each file path exists. The Reviewer doesn't trust the Implementor's claim of "all tests pass" — it runs `dotnet test` itself.

This breaks the error snowball effect where a small planning mistake compounds into broken implementation.

---

## 4. Overcoming Common AI Limitations

### 4.1 Context Window Decay

**Problem:** Large language models lose attention in the middle of long prompts. A 20,000-token system prompt causes the model to "forget" rules stated in the middle.

**Mitigations:**
- **Layered instruction loading:** Instead of one massive prompt, instructions are split across 3 layers that load conditionally:
  - `copilot-instructions.md` — always loaded, contains only shared rules
  - `.instructions.md` files — loaded via `applyTo` globs only when editing matching files (e.g., backend rules only when editing `.cs`)
  - `.agent.md` — loaded only when that specific agent is active
- **On-demand document reading:** Agents read architecture extracts only when they reach the step that needs them, not upfront
- **Focused architecture extracts:** The full architecture spec (`MonthlyBudget_Architecture.md`) is too large to load. It's decomposed into 5 focused files: domain invariants, API contracts, persistence conventions, tech stack, and design gaps
- **Context pattern files:** Per-bounded-context pattern files (e.g., `budget-patterns.md`) provide just the conventions relevant to the agent's current task

### 4.2 Hallucination — Fabricating Non-Existent Code

**Problem:** AI agents confidently cite file paths, class names, method signatures, and invariant rules that don't exist. This is the single most common and damaging failure mode.

**Mitigations:**
- **Global Grounding Rules:** 7 rules in `copilot-instructions.md` that every agent inherits:
  1. Every file path must be verified via search
  2. Every type/class name must be grepped for its exact declaration
  3. Every namespace must be confirmed to exist
  4. Every method signature must be read from the actual source file
  5. Every invariant must be looked up in `domain-invariants.md`
  6. Every API endpoint must be cross-referenced against `api-contracts.md`
  7. When something seems "missing" — search the entire codebase before claiming it doesn't exist
- **Agent-specific grounding rules** add role-targeted checks on top (e.g., "When adding DI registrations, read `ServiceCollectionExtensions.cs` first to match grouping style")
- **"Know Your Limitations" section:** The system prompt explicitly tells agents they will hallucinate, making them more cautious: *"You will confidently fabricate file paths, type names, method signatures, and invariant rules that don't exist."*
- **Plan-then-verify pattern:** Planners list every file path as CREATE or MODIFY. Implementors verify each path exists before writing code.

### 4.3 Error Snowballing in Multi-Agent Chains

**Problem:** In multi-agent pipelines, a small mistake by one agent (e.g., wrong file path in a plan) compounds through subsequent agents into completely broken output. Google's research on 180 multi-agent setups confirms this pattern.

**Mitigations:**
- **Input validation at every boundary:** Every agent validates its upstream input before proceeding. The Planner checks the Issue Reader's memory file is complete. The Implementor verifies the Plan's file paths exist. The Reviewer re-runs tests independently.
- **Pre-flight checks:** Structural verification before any work begins (file exists? non-empty? build passes?)
- **Self-verification checkpoints:** Before each commit, implementors verify their own output against the plan and codebase state
- **Scope guards:** Before pushing, implementors compare their changes against the plan — if they created >2 files not in the plan, they stop and ask
- **Mandatory build + test gates:** No commit without `dotnet build` and `dotnet test` passing — catches errors before they propagate

### 4.4 Semantic Drift Across Sessions

**Problem:** AI agents have no memory between sessions. Decisions made, patterns discovered, and lessons learned are lost when the conversation ends. The next session starts from zero, potentially re-making the same mistakes.

**Mitigations:**
- **3-Tier Memory Model:**
  - **Tier 1 — Active** (`.github/agents/memory/active/`): Current sprint's working files. Created when work starts on an issue, archived when done.
  - **Tier 2 — Distilled** (`memory/knowledge.md` + MCP Knowledge Graph): Compressed atomic facts extracted from completed work. Updated after every issue. This is the "long-term memory" agents read on startup.
  - **Tier 3 — Archive** (`memory/archive/`): Raw completed memory files. Never loaded on startup — only accessed via search when historical context is explicitly needed.
- **MCP Knowledge Graph:** Structured entity-relationship store (`@modelcontextprotocol/server-memory`) for codebase topology and cross-session decisions. Entities (issues, aggregates, patterns) connected by relations (resolves, modifies, depends_on) with evolving observations.
- **Decisions Made sections:** Every memory file has a `## Decisions Made` section that records clarifications obtained from the user. This prevents downstream agents from re-asking resolved questions.
- **Activity log:** Cross-agent awareness of recent events survives session boundaries because it's a file, not chat history.

### 4.5 Scope Creep and Over-Engineering

**Problem:** AI agents tend to add helpful-seeming features, error handling, and abstractions beyond what was requested. This wastes time and introduces untested code.

**Mitigations:**
- **"No Suppositions" rule:** Global rule prohibiting assumptions. If anything is unclear, agents must ask — not improvise.
- **MVP Focus constraint:** "Implement only what's required by the issue and architecture spec. No over-engineering."
- **Plan-driven implementation:** The Implementor follows the Planner's file list exactly. If it disagrees, it asks — it doesn't deviate silently.
- **Scope guard check:** Before pushing, implementors run `git diff --stat master` and flag any files not in the plan.
- **Feature-by-feature commits:** Each commit is a buildable, testable increment. This makes it easy to revert over-engineering.

### 4.6 Inconsistency in Generated Content

**Problem:** When generating multiple related outputs (UI screens, code across files, test suites), each generation is independent. Identifiers, conventions, and styles drift between outputs.

**Mitigations:**
- **Pattern files:** Per-bounded-context convention files (e.g., `budget-patterns.md`) provide a consistent reference for naming, folder structure, and code patterns
- **Structured templates:** Memory file templates enforce consistent structure so generated artifacts don't drift in format
- **Cross-screen consistency checklist:** The UI Designer agent has an 11-item checklist run after all screens are generated, checking for persona drift, role label invention, scope violations, broken assets, and date inconsistencies
- **Code style from existing examples:** Agents are instructed to grep existing files and match their patterns rather than inventing new conventions

---

## 5. Instruction Architecture

Instructions follow a 3-layer model designed to minimize duplication while maximizing context relevance:

```
┌─────────────────────────────────┐
│  copilot-instructions.md        │  Always loaded for ALL agents
│  (Shared rules, persona, memory)│
├─────────────────────────────────┤
│  backend.instructions.md        │  Loaded when editing src/**/*.cs
│  frontend.instructions.md       │  Loaded when editing frontend/**
│  (Stack-specific conventions)   │
├─────────────────────────────────┤
│  <agent>.agent.md               │  Loaded when agent is active
│  (Role-specific workflow)       │
└─────────────────────────────────┘
```

### Layer 1: Global (`copilot-instructions.md`)

The "Staff Engineer" persona. Contains rules every agent must follow regardless of role:
- AI limitation awareness (context decay, hallucination, error snowballing, semantic drift)
- No Suppositions policy
- 7 global Grounding Rules
- Git Discipline
- Architectural Constraints (hexagonal purity, MVP focus, approved stack, cross-context isolation)
- Architecture reference tables (what to read, when to read it)
- 3-Tier Memory Model specification
- MCP Knowledge Graph conventions
- Activity Log rules
- Agent workflow summaries
- Blocker Protocol

### Layer 2: Stack-Specific (`.instructions.md`)

Conditional instructions loaded based on which files are being edited:

- **`backend.instructions.md`** (`applyTo: src/**/*.cs,tests/**/*.cs`) — Hexagonal layer table, backend grounding rules, build/test commands, persistence conventions
- **`frontend.instructions.md`** (`applyTo: frontend/**/*.ts,frontend/**/*.svelte`) — TypeScript strictness rules, component patterns, state handling requirements, build commands

This prevents frontend agents from loading backend rules and vice versa.

### Layer 3: Agent-Specific (`.agent.md`)

Each agent file contains only what is unique to that role:
- YAML frontmatter (name, description, model, tools, handoffs)
- Context Loading Priority (what to read and in what order)
- Agent-Specific Grounding Rules (role-targeted verification checks)
- Execution Steps (the step-by-step workflow)
- Memory file templates (exact Markdown structure for outputs)
- Critical Rules (agent-specific guardrails)

---

## 6. Memory Architecture

### 6.1 Why Tiered Memory?

Flat, append-only memory systems grow linearly and waste context window:
- After 10 completed issues, agents load 10 issue-reader files, 10 plans, 10 implementations, 10 reviews = 40 files of stale data on startup
- Critical learnings are buried in verbose narrative files
- No distinction between "need now" and "historical reference"

The 3-tier model solves this by separating concerns:

### 6.2 Tier 1 — Active Context (Working Memory)

**Location:** `.github/agents/memory/active/`

Current sprint's work files. Small, focused, temporary.

| File Type | Writer | Consumer |
|---|---|---|
| `issue-reader-<N>.md` | Issue Reader | Planner |
| `plan-<N>.md` | Planner | Implementor |
| `implementation-<N>.md` | Implementor | Reviewer |
| `code-reviewer-<N>.md` | Reviewer | Planner (fix cycle) |

**Lifecycle:** Created when work starts → archived when issue closes.

### 6.3 Tier 2 — Distilled Knowledge (Semantic Memory)

**Location:** `memory/knowledge.md` + MCP Knowledge Graph

Compressed, reusable facts extracted from completed work. Organized by category:
- Architecture decisions
- Codebase patterns learned
- Cross-context communication rules
- Common review findings
- Agent customization patterns

**Rule:** After completing any issue, extract the generalizable insight and add it here. Delete verbose details. If a fact is already captured, don't duplicate it.

### 6.4 Tier 3 — Cold Archive (Episodic Memory)

**Location:** `memory/archive/`

Raw, unedited memory files from completed issues. Exists for:
- Human auditing ("what did the agent do on issue #41?")
- Rare agent retrieval via search when historical context is explicitly requested

**Rule:** Never loaded on startup. Only accessed via targeted file search.

### 6.5 MCP Knowledge Graph

A structured entity-relationship store running via `@modelcontextprotocol/server-memory`. Configured in `.vscode/mcp.json`.

Unlike the flat knowledge.md file, the graph supports:
- Querying relationships ("what entities does issue #75 modify?")
- Evolving observations on entities over time
- Cross-referencing bounded contexts, aggregates, invariants, and API endpoints

Agents write to the graph after making decisions. Agents read from the graph before planning or reviewing.

### 6.6 Activity Log

An append-only "standup board" at `.github/agents/activity-log.md` for cross-agent awareness. Not a memory file — a broadcast channel.

Every agent scans it on startup to understand recent team events. Every agent writes to it after cross-team events (PRs opened, issues created, reviews completed).

When the log exceeds ~100 entries, old entries are summarized and moved to archive.

---

## 7. Quality Gates

Quality is enforced at multiple points throughout the pipeline:

| Gate | Who | When | What's Checked |
|---|---|---|---|
| Pre-flight check | Every agent | Before starting | Inputs exist, complete, build passes |
| Self-verification | Implementors | Before each commit | Namespaces exist, DI registered, tests cover exceptions, no rogue files |
| Build + test | Implementors | Before each commit | `dotnet build` + `dotnet test` (all tests, not just new) |
| Scope guard | Implementors | Before push | `git diff --stat master` — no files outside plan |
| Hexagonal validation | Implementors + Reviewers | After all features | No forbidden imports in Domain/Application layers |
| API exercise | Implementors + Reviewers | After endpoint changes | Every endpoint returns correct status code and body shape |
| Architecture review | Reviewers | During review | Invariant enforcement, API contract compliance, persistence conventions |
| Additive review | Reviewers | Round 2+ | Only changed files re-reviewed, prior findings tracked |
| Consistency checklist | UI Designer | After all screens | 11-point cross-screen verification |

---

## 8. Evolving the System

The Agent Improver agent runs structured retrospectives to improve other agents:

1. **Interview:** Structured Keep/Stop/Start retrospective with the user
2. **Analysis:** Cross-reference with official VS Code docs, related agent files, and community patterns
3. **Proposal:** Present specific changes with traceability to user feedback
4. **Edit:** Apply minimal, atomic changes to the target file
5. **Changelog:** Every session appends to `.github/agents/changelog.md`

The changelog serves as the system's evolution history — what was changed, why, what was learned. New lessons are distilled into `knowledge.md` to prevent future agents from repeating past mistakes.

---

## 9. File Map

```
.github/
├── copilot-instructions.md          ← Global rules (always loaded)
├── instructions/
│   ├── backend.instructions.md      ← .NET rules (loaded for *.cs)
│   └── frontend.instructions.md     ← SvelteKit rules (loaded for *.ts/*.svelte)
├── agents/
│   ├── backend-implementor.agent.md ← Agent definitions (12 files)
│   ├── backend-planner.agent.md
│   ├── backend-reviewer.agent.md
│   ├── frontend-implementor.agent.md
│   ├── frontend-planner.agent.md
│   ├── frontend-reviewer.agent.md
│   ├── issue-reader.agent.md
│   ├── issue-writer.agent.md
│   ├── product-manager.agent.md
│   ├── software-architect.agent.md
│   ├── ui-designer.agent.md
│   ├── agent-improver.agent.md
│   ├── activity-log.md              ← Team standup board
│   ├── changelog.md                 ← Agent customization history
│   ├── context/                     ← Per-bounded-context pattern files
│   │   ├── shared-patterns.md
│   │   ├── budget-patterns.md
│   │   ├── forecast-patterns.md
│   │   ├── identity-patterns.md
│   │   └── frontend-patterns.md
│   └── memory/
│       ├── active/                  ← Tier 1: Current sprint working files
│       ├── archive/                 ← Tier 3: Completed issue files
│       └── knowledge.md            ← Tier 2: Distilled cross-session facts
├── skills/
│   ├── additive-review/SKILL.md
│   ├── api-exercise/SKILL.md
│   ├── dotnet-tdd/SKILL.md
│   ├── hexagonal-validation/SKILL.md
│   ├── resume/SKILL.md
│   └── sveltekit-dev/SKILL.md
.vscode/
└── mcp.json                         ← MCP Knowledge Graph server config
```
