# Agent Customization Changelog

> Versioned record of all changes to agent customization files (`.agent.md`, `.instructions.md`, `SKILL.md`, `copilot-instructions.md`, `AGENTS.md`).
> Each entry captures **what** changed, **why**, **what worked/didn't** from the retro, and **lessons learned** for future sessions.

---

<!-- 
## Entry Template (copy this for each new entry)

### YYYY-MM-DD — [Agent/File Name] — [Short Title]

**Retro trigger:** [What prompted this change — user feedback, observed failure, workflow gap]

**Files modified:**
- `path/to/file.md` — [brief description of change]

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | [section] | [what changed] | [why — traced to retro feedback] |

**What was working (kept):**
- [Behaviors/rules preserved because they were effective]

**What wasn't working (fixed):**
- [Specific symptoms and how the change addresses them]

**Lessons learned:**
- [Insights about what makes agents more effective — generalizable takeaways]

**Risks & watch items:**
- [Things to monitor after this change]

---
-->

### 2026-04-18 — All Agents + copilot-instructions.md + AGENTS.md — Port PR feedback flow, sub-agents, and COMMENT-only rule from thermo-replacer

**Retro trigger:** Four improvements validated in the `thermo-replacer` project (Genial-T31 Bridge) needed to be ported back to MonthlyBudget. These were project-agnostic pipeline improvements discovered during that project's implementation: (1) Reviewer repeatedly hit REQUEST_CHANGES API errors on own PR. (2) PR feedback routing through Planner was an unnecessary intermediary. (3) Agents couldn't consult each other mid-task because `disable-model-invocation: true` blocked sub-agent calls. (4) `vscode/askQuestions` tool truncated long content, breaking HITL usability.

**Files modified:**
- `.github/skills/address-pr-feedback/SKILL.md` — **Created.** 10-step workflow for addressing PR review feedback: classify review points (direct-fix vs. escalate-to-planner), HITL confirmation, fix code, build verification, reply to GitHub threads, commit/push, update memory, request re-review. Includes escalation signals checklist and GitHub thread reply conventions. Adapted for both .NET and SvelteKit stacks.
- `.github/agents/backend-reviewer.agent.md` — Removed `disable-model-invocation: true`. Added `agent` tool and `agents: ['Backend Planner']`. Added "Always use COMMENT event" as top Critical Rule. Updated handoff from Backend Planner → Backend Implementor. Added Sub-agent Invocation section. Updated pre-flight to use task-context completeness check. Updated HITL gate to reference COMMENT-only.
- `.github/agents/frontend-reviewer.agent.md` — Same changes as backend-reviewer, adapted for frontend (agents: Frontend Planner, handoff to Frontend Implementor).
- `.github/agents/backend-implementor.agent.md` — Removed `disable-model-invocation: true`. Added `agent` tool and `agents: ['Backend Planner', 'Backend Reviewer']`. Added `address-pr-feedback` skill reference. Added Sub-agent Invocation section with escalation criteria, anti-chattiness rules, HITL inheritance note. Updated pre-flight to use task-context completeness check.
- `.github/agents/frontend-implementor.agent.md` — Same changes as backend-implementor, adapted for frontend.
- `.github/agents/backend-planner.agent.md` — Removed `disable-model-invocation: true` (enables sub-agent invocation by Implementor/Reviewer).
- `.github/agents/frontend-planner.agent.md` — Same as backend-planner.
- `.github/agents/product-manager.agent.md` — Removed `disable-model-invocation: true`.
- `.github/agents/software-architect.agent.md` — Removed `disable-model-invocation: true`.
- `.github/agents/ui-designer.agent.md` — Removed `disable-model-invocation: true`.
- `.github/copilot-instructions.md` — Updated Workflow 2 to route Reviewer→Implementor (was Reviewer→Planner→Implementor). Added HITL table entries for Implementor PR feedback gate and Reviewer COMMENT note. Added "Sub-agent Invocation Convention" section with allowed pairs table, anti-chattiness rule, and HITL inheritance note. Added "`vscode/askQuestions` Tool — Long Content Rule" section.
- `AGENTS.md` — Updated Delivery Pipeline section to document the PR Review & Fix additive model with Implementor as primary fixer.
- `.github/agents/agent-improver.agent.md` — Added `address-pr-feedback/SKILL.md` to skill inventory.

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | New skill | Created `address-pr-feedback` with full workflow | Implementor needs structured workflow for addressing review comments with escalation criteria |
| 2 | Reviewer Critical Rules | Added "Always use COMMENT event" with explanation | Reviewer hits GitHub API error when using REQUEST_CHANGES on own PR — permanent setup constraint |
| 3 | Reviewer handoff | Target: Planner → Implementor | Developers fix their own code; Planner available as sub-agent for design questions |
| 4 | Agent frontmatter | Removed `disable-model-invocation: true` from all 9 agents | Enables sub-agent invocation between agents |
| 5 | Implementor/Reviewer frontmatter | Added `agent` tool + `agents` whitelist | Controlled sub-agent invocation with specific allowed pairs |
| 6 | Sub-agent guidelines | Added to Implementors, Reviewers, and global instructions | Clear rules about when to use sub-agents vs. handoffs vs. just doing it yourself |
| 7 | Workflow 2 | `Reviewer → Implementor` (was `Reviewer → Planner → Implementor`) | Removes unnecessary Planner intermediary for straightforward PR fixes |
| 8 | HITL table | Added Implementor PR feedback gate; added COMMENT note on Reviewer | Reflects new skill's HITL gate and COMMENT constraint |
| 9 | Global askQuestions rule | Added long-content workaround | Tool truncates long text; writing to chat first solves visibility problem |
| 10 | Pre-flight checks | Implementors and Reviewers now run task-context completeness check | Downstream agents should verify context, not blindly trust |

**What was working (kept):**
- Additive review model (baseline, delta, review points) — preserved, skill integrates with it
- 3-tier memory architecture — preserved
- Pre-flight checks and self-verification checkpoints — preserved
- Handoff buttons for full context switches — preserved alongside new sub-agent capability
- HITL gates at all decision points — preserved, new ones added
- Git discipline conventions — preserved
- Skill-based capability pattern — extended with new skill
- All existing Critical Rules on reviewers — preserved, new rules added
- Learnings capture across all agents — preserved (already present from earlier sync)

**What wasn't working (fixed):**
- Reviewer repeatedly got "can't request changes on own PR" API error — now hardcoded to always use COMMENT event with clear explanation of why
- PR feedback addressing required unnecessary Planner intermediary — now Implementor handles directly with Planner available as sub-agent for design questions
- All agents had `disable-model-invocation: true` preventing any inter-agent consultation — now all agents can be invoked as sub-agents, with Implementors and Reviewers having controlled `agents` whitelists
- `vscode/askQuestions` tool questions were too long to read — now agents write full context to chat first and use the tool for a short summary question only
- Implementors and Reviewers didn't verify context completeness — now pre-flight includes task-context Mode 2 check

**Lessons learned:**
- **Permanent setup constraints should be elevated to Critical Rules, not left as runtime discoveries.** When an agent encounters the same error repeatedly (like "can't REQUEST_CHANGES on own PR"), the constraint should be baked into its instructions so it never attempts the failing action.
- **Sub-agent invocation requires a three-part framework: (1) who can invoke whom, (2) when to invoke vs. handle yourself, (3) what the sub-agent should do vs. what the parent continues.** Without all three, agents either never use sub-agents or over-use them.
- **The "developer addresses reviewer feedback" pattern from real teams maps cleanly to "Implementor addresses PR feedback with Planner sub-agent for escalation."** The Planner-as-intermediary model was an over-formalization that didn't match how real engineering teams work.
- **Tool limitations should be addressed with explicit workaround conventions, not vague guidance.** The `vscode/askQuestions` truncation issue was causing real HITL failures — the "write to chat first, then ask short question" pattern is a concrete, testable convention.
- **Cross-project improvement porting is effective when changes are project-agnostic.** The pipeline structural improvements (sub-agents, COMMENT rule, PR feedback flow, askQuestions rule) transferred cleanly because they didn't depend on the tech stack.

**Risks & watch items:**
- The `agents` property and sub-agent invocation are marked **experimental** in VS Code docs (as of April 2026) — API could change in future updates
- Agents might **over-invoke sub-agents** instead of making decisions independently — monitor for chattiness and add stricter guidelines if needed
- The `address-pr-feedback` skill's **escalation checklist** needs validation against real PR review rounds
- Removing `disable-model-invocation: true` from all agents means any agent with the `agent` tool could invoke them — the `agents` whitelist on the calling agents is the control
- The **long-content workaround** relies on agent judgment about "too long" — monitor if agents are consistent
- The Reviewer's handoff now goes to **Implementor instead of Planner** — if the user prefers the Planner to plan fixes first, the workflow may need an alternative path

---

### 2026-03-21 — UI Designer — Add post-generation consistency self-review

**Retro trigger:** Stitch-generated screens were handed off with missing loading/error/empty states and style/layout drift between screens. No systematic quality gate existed before handoff.

**Files modified:**
- `.github/agents/ui-designer.agent.md` — Added Step 6 (Self-Review for Consistency) with download, visual inspection, checklist, and fix sub-steps. Renumbered Steps 6→7, 7→8. Added critical rule blocking handoff until review passes.

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | Execution Workflow | Added Step 6: Self-Review for Consistency (6a–6e) | No cross-screen quality gate existed; screens were handed off with inconsistencies |
| 2 | Execution Workflow | Renumbered Document Screen Mapping to Step 7, Hand Off to Step 8 | Accommodate the new step |
| 3 | Critical Rules | Added "Never hand off without passing the consistency review" | Enforce the new quality gate as a hard rule |

**What was working (kept):**
- Per-screen review in Step 5 (individual screen iteration with Stitch)
- Screen plan confirmation before generation
- One-screen-at-a-time generation workflow
- Stitch MCP tool usage patterns

**What wasn't working (fixed):**
- Missing loading/error/empty states in generated screens went undetected until architect/PM review
- Style/layout drift between screens (colors, spacing, component patterns) was not caught
- No API field name cross-check against `docs/arch/api-contracts.md`

**Lessons learned:**
- Per-screen review (Step 5) catches issues within a single screen but misses cross-screen drift — a holistic review step after all screens are generated is essential
- Using a structured checklist table forces the agent to evaluate every dimension systematically rather than relying on ad-hoc visual impression
- Giving the agent a two-tier fix strategy (direct HTML edits for small issues, Stitch re-prompts for large ones) avoids both over-reliance on regeneration and manual micro-editing of structural problems
- Downloading HTMLs locally and inspecting via Chrome DevTools MCP creates a concrete verification loop — the agent can see exactly what it's reviewing

**Risks & watch items:**
- The download + browser inspection adds time to the workflow — monitor whether it causes excessive back-and-forth
- The `downloadUrl` field from `get_screen` must exist in the Stitch API response — if it changes, the download sub-step breaks
- Large fix re-prompts to Stitch may introduce new inconsistencies — the re-verify loop (6e) mitigates this but could theoretically cycle

---

### 2026-03-21 — Agent Improver — Add hard gate for retrospective step

**Retro trigger:** The Agent Improver skipped its own Step 2 (Conduct the Retrospective Interview) during the UI Designer improvement session. It asked a few ad-hoc clarifying questions but did not run the structured Keep/Stop/Start retrospective or produce a summary. The user caught this violation.

**Files modified:**
- `.github/agents/agent-improver.agent.md` — Added warning banner to Step 2 clarifying that ad-hoc questions ≠ the retro. Added new Step 2b (Retro Summary Gate) requiring a structured summary be written to chat before Step 3 can begin.

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | Step 2 | Added warning banner: step is not optional, ad-hoc questions don't count | Agent rationalized skipping the retro by asking targeted questions instead |
| 2 | New Step 2b | Added Retro Summary Gate with mandatory output format (Keep/Stop/Start/Workflow) | No enforcement mechanism existed — the retro was a soft instruction with no checkpoint |
| 3 | Step 2b | Summary is declared as "contract" — every Step 5 change must trace to it | Creates traceability and makes skipping visible |

**What was working (kept):**
- Official docs consultation before editing
- Changelog requirement after every session
- Change proposal via `vscode/askQuestions` before editing
- Grounding rules / anti-hallucination checks

**What wasn't working (fixed):**
- Agent jumped directly to ad-hoc clarifying questions instead of running the full structured retrospective
- No enforcement mechanism — the retro step was indistinguishable from general clarification
- No required output artifact — the agent could claim it "gathered feedback" without producing a structured summary

**Lessons learned:**
- Soft instructions ("always do X") are easily rationalized away by agents — hard gates with required output artifacts are more reliable
- The distinction between "asking questions" and "conducting the retrospective" must be explicitly called out, because both use the same tool (`vscode/askQuestions`) and agents will conflate them
- Traceability requirements (every change must trace to a retro item) create accountability and make skipped steps visible in the output
- When an agent violates its own rules, the fix should add structural enforcement (output gates), not just stronger wording

**Risks & watch items:**
- The retro gate adds a mandatory step — monitor whether it feels too rigid for small/obvious changes where the user's intent is already clear
- If the user provides all Keep/Stop/Start info unprompted in their initial message, the agent still must write the Retro Summary (it can be brief) — watch for complaints about redundancy

---

### 2026-03-21 — Issue Writer (new) + Workflow Documentation — Close the architecture-to-issues gap

**Retro trigger:** After the Software Architect produced `design-gaps.md` and updated architecture docs, there was no agent to turn those artifacts into GitHub issues. The user also described a project startup flow (PM → Designer → Architect → ???) that similarly lacked an issue creation step. Both flows were undocumented and invented ad-hoc.

**Files modified:**
- `.github/agents/issue-writer.agent.md` — **Created** new agent with two modes: Mode A (Startup: PRD user stories → issues) and Mode B (Design Review: design-gaps → issues)
- `.github/agents/software-architect.agent.md` — Added handoff to Issue Writer
- `.github/copilot-instructions.md` — Documented Workflow 4 (Project Startup) and Workflow 5 (Post-Implementation Design Review); renamed "Shared Agent" to "Shared Agents" and added Issue Writer
- `AGENTS.md` — Added sections 3 (Project Startup) and 4 (Post-Implementation Design Review) to Custom Agent Workflows; added usage guidance for when to use each flow

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | New agent | Created `issue-writer.agent.md` with Mode A and Mode B | No agent existed to convert architecture artifacts into GitHub issues — manual bottleneck |
| 2 | Software Architect handoffs | Added "Hand off to Issue Writer" | Architect is the upstream producer; needs a path to the issue creation step |
| 3 | copilot-instructions.md | Added Workflow 4 (Startup) and Workflow 5 (Design Review) | Both flows were undocumented — users had to invent them ad-hoc |
| 4 | copilot-instructions.md | Renamed "Shared Agent" → "Shared Agents", added Issue Writer | Issue Writer is a shared agent used by both flows |
| 5 | AGENTS.md | Added sections 3 and 4 with pipeline diagrams | Codebase guide must reflect all documented workflows |
| 6 | AGENTS.md | Added flow selection guidance | Users need to know when to use which flow |

**What was working (kept):**
- The `design-gaps.md` format (GAP-N with priority, files, changes, verification) — actionable and structured enough for automated issue creation
- Updating `domain-invariants.md` and `api-contracts.md` in-place during architect review
- The timing of post-implementation design review (identifies real gaps vs. hypothetical ones)
- The Product Discovery loop (PM → Designer → Architect) as a standalone alignment loop

**What wasn't working (fixed):**
- No agent to convert design-gaps.md into GitHub issues — manual bottleneck after every architect review
- The project startup flow (PM → Designer → Architect → issues) was not documented and had no issue creation step
- The post-implementation design review flow was entirely ad-hoc — no one knew it existed as a formal workflow
- "Shared Agent" (singular) in copilot-instructions.md didn't account for the Issue Writer

**Lessons learned:**
- When a workflow gap appears at a handoff boundary (architect produces artifacts but no one consumes them into the project board), the fix is a dedicated agent — not extending the upstream agent's scope, which would overload it
- Two-mode agents (Mode A/Mode B) work well when the same core capability (issue creation) serves different input sources — keeps the agent focused while avoiding agent sprawl
- Documenting flows in both `copilot-instructions.md` (for AI agents) and `AGENTS.md` (for human developers) prevents drift between what agents do and what humans expect
- A confirmation step before bulk issue creation is critical — it lets the human review the plan before GitHub state is modified (hard to reverse)
- Label conventions should be explicit in the agent file (not left to agent judgment) to ensure consistency across runs

**Risks & watch items:**
- Mode A (Startup) is untested — the original project didn't go through this flow. Monitor the first usage for missing steps or wrong issue granularity
- The Issue Writer assumes `design-gaps.md` follows the current format (GAP-N sections). If the Architect changes the format, the Issue Writer may fail silently
- Label creation (e.g., `priority:p0`, `design-gap`) requires GitHub permissions — verify the agent can create labels if they don't exist
- The Issue Writer doesn't produce memory files — it creates GitHub issues directly. Downstream agents (Issue Reader) consume GitHub state, not filesystem artifacts. This is a deliberate design choice but differs from other agents that use memory files

---

### 2026-03-21 — UI Designer — Add style discovery & expand consistency checklist

**Retro trigger:** User reported that Stitch-generated screens had critical inconsistencies (broken icons, wrong dates, 5 different personas, 6 invented role labels, out-of-scope features) and that the agent never asked about the user's visual style preferences beyond the PRD.

**Files modified:**
- `.github/agents/ui-designer.agent.md` — Added style discovery sub-step to Step 1; expanded Step 6c checklist from 6 to 11 checks.

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | Step 1 | Added mandatory style discovery (reference apps, visual mood, design constraints) | Agent generated screens without understanding user's aesthetic preferences — all style decisions were ungrounded |
| 2 | Step 6c | Added check #7: Persona consistency | Screens used 5 different fake user names (Alex Rivera, Alex River, John Doe, etc.) |
| 3 | Step 6c | Added check #8: Role/label compliance | Screens invented 6 role labels when PRD defines only "Owner" and "Partner" |
| 4 | Step 6c | Added check #9: PRD scope compliance | Screens included out-of-scope features (AI insights, social login, premium tiers, export, savings goals) |
| 5 | Step 6c | Added check #10: Asset integrity | Material Icons rendered as raw text instead of icons on one screen |
| 6 | Step 6c | Added check #11: Content accuracy | Dates showed 2024 instead of 2026; placeholder data was inconsistent |

**What was working (kept):**
- Stitch MCP integration and screen generation workflow
- Per-screen review loop (Step 5)
- Chrome DevTools visual inspection (Step 6b)
- Hybrid fix strategy: small fixes via direct HTML edits, large fixes via Stitch re-prompts (Step 6d)
- Re-verify loop until all checks pass (Step 6e)

**What wasn't working (fixed):**
- Agent never asked about visual style preferences — screens were generated with ungrounded aesthetic choices
- Consistency checklist missed critical cross-screen issues: persona drift, label invention, scope violations, broken assets, date errors

**Lessons learned:**
- Generative AI tools like Stitch are excellent at individual screen quality but poor at cross-screen consistency — every screen generation uses independent context, so identifiers (names, roles, dates) drift unless explicitly constrained
- Asking users about style preferences before generation is far cheaper than fixing aesthetic mismatches after — front-loading one question round saves multiple fix-and-re-verify cycles
- Consistency checklists should be derived from real failure modes, not theoretical categories — the original 6 checks were reasonable in theory but missed the actual issues (persona, labels, scope, assets, dates) because they were written before any screens were generated
- PRD scope compliance is a non-obvious but critical check — generative tools will happily invent features that sound reasonable but violate scope boundaries

**Risks & watch items:**
- Style discovery adds friction before generation — if users consistently skip it, consider making it optional with a default style preset
- 11-item checklist is comprehensive but takes longer to execute — monitor whether the agent short-circuits later checks after finding early failures
- Persona consistency check relies on the agent reading all screens' HTML content — if screens are complex, this may hit context window limits

---

### 2026-03-22 — All Agents + Global Instructions — Add shared activity log for cross-team awareness

**Retro trigger:** After the Software Architect found design gaps and the Issue Writer created issues from them, other agents (e.g., Issue Reader) had no awareness these events happened. When the user referenced "gap 2," agents without direct memory context didn't understand. Cross-team events were invisible to uninvolved agents — there was no broadcast mechanism, only 1:1 memory file handoffs.

**Files modified:**
- `.github/agents/activity-log.md` — **Created** append-only team activity log with entry format template and instructions
- `.github/copilot-instructions.md` — Added Global Rule #6 (Activity Log) defining read-on-startup and write-after-cross-team-events conventions
- All 11 `.agent.md` files — Added "Scan on startup" to Context Loading Priority and "Log cross-team events" to Critical Rules
- `frontend-planner.agent.md` — Created missing Critical Rules section (needed as anchor for the write instruction)
- `AGENTS.md` — Added "Team Activity Log" section under Custom Agent Workflows

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | New file | Created `.github/agents/activity-log.md` with entry template | No shared awareness mechanism existed — agents were siloed |
| 2 | copilot-instructions.md | Added Global Rule #6 (Activity Log) | Needed a universal convention all agents inherit |
| 3 | All agents — Context Loading Priority | Added "Scan on startup" line before numbered list | Agents must read the log early to understand recent team context |
| 4 | All agents — Critical Rules | Added agent-specific "Log cross-team events" bullet | Each agent needs to know what triggers a log write for their role |
| 5 | frontend-planner.agent.md | Created Critical Rules section | Was the only agent missing this section |
| 6 | AGENTS.md | Added "Team Activity Log" subsection | Human developers need to understand the convention too |

**What was working (kept):**
- Memory files (`.github/agents/memory/`) for 1:1 handoffs — still the primary communication channel for structured data
- Product artifacts (`docs/product/`) as source of truth for product decisions
- Changelog for tracking agent customization changes — kept separate from the activity log
- Context Loading Priority ordering in each agent — activity log is a pre-list scan, not a displacement

**What wasn't working (fixed):**
- Cross-team events (gaps found, issues created from gaps) were invisible to uninvolved agents
- When user referenced prior work (e.g., "gap 2"), agents without direct memory had no context
- No broadcast mechanism existed — only point-to-point memory files used for handoffs

**Lessons learned:**
- 1:1 memory files solve the handoff problem but not the awareness problem — agents need both structured handoffs AND lightweight team awareness
- The "standup board" metaphor works well for AI agents: brief, event-driven, append-only, with artifact references instead of duplicated content
- Adding the read instruction to Context Loading Priority is more reliable than relying only on the global rule — agents follow their own file's priority list most faithfully
- Agent-specific write triggers (e.g., "after producing architecture artifacts" vs "after opening a PR") are more actionable than generic "write when relevant" instructions
- A shared log is a low-cost, high-value addition — it doesn't require structural changes to the pipeline, just awareness layered on top

**Risks & watch items:**
- The activity log grows indefinitely — if it gets too large, agents may waste context scanning old entries. Consider periodic archival or "last N entries" guidance
- Write compliance is a soft instruction (no structural gate like the Retro Summary Gate) — monitor whether agents actually write to the log
- Log quality may drift — entries could become too verbose or too terse. The "standup" framing and entry template should help constrain this

---

### 2026-04-20 — All Agents + Global Instructions — Instruction deduplication, 3-tier memory, Staff Engineer persona

**Retro trigger:** User identified massive instruction duplication across all 12 agent files — No Suppositions, Repository, Grounding Rules, Git Rules, and Hexagonal Rules were copy-pasted identically in every agent. Memory architecture was flat (37+ files in a single directory) with no tiering or compression. The MCP knowledge graph was configured but the `.vscode/mcp.json` was empty. Context window was wasted on repeated boilerplate instead of agent-specific content.

**Files modified:**
- `.github/copilot-instructions.md` — Full rewrite as "Staff Engineer" persona. Consolidated: No Suppositions, Repository info, 7 global Grounding Rules, Git Discipline, Architectural Constraints, Architecture Reference tables, 3-Tier Memory Model, Knowledge Graph (MCP) conventions, Activity Log rotation rules, Artifact-Driven Handoffs, 5 Agent Workflows, Agent Conventions, Blocker Protocol. Added "Know Your Limitations" section (context window decay, hallucination risk, error snowballing, semantic drift).
- `.github/instructions/backend.instructions.md` — **Created** with `applyTo: 'src/**/*.cs,tests/**/*.cs'`. Contains: 6 backend-specific grounding rules, hexagonal layer table, build/test commands, persistence conventions, cross-context communication rules.
- `.github/instructions/frontend.instructions.md` — **Created** with `applyTo: 'frontend/**/*.ts,frontend/**/*.svelte,frontend/**/*.js'`. Contains: 4 frontend-specific grounding rules, TypeScript rules, state handling, build commands, "Never Load Backend Files" list.
- All 12 `.agent.md` files — Stripped: ⛔ No Suppositions section, Repository section, "Scan on startup: activity-log" line, "NEVER pre-load: Architecture.md" line. Trimmed Grounding Rules to agent-specific only (renamed to "Agent-Specific Grounding Rules"). Removed Git Rules and Hexagonal/Frontend Architecture quick-reference tables from implementor agents (now in `.instructions.md` files). Updated all memory paths from `memory/<type>-` to `memory/active/<type>-`.
- `.vscode/mcp.json` — Added `@modelcontextprotocol/server-memory` configuration.
- `AGENTS.md` — Updated Delivery Pipeline memory paths to `memory/active/`. Added "Memory Architecture — 3-Tier Model" section with tier table and MCP note. Added "Instruction Layering" section with 4-layer table. Added activity log rotation rule.

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | copilot-instructions.md | Full rewrite as Staff Engineer with AI limitations awareness | Prior version used XML tags and duplicated agent-level content; needed a clear persona and centralized shared rules |
| 2 | New: backend.instructions.md | Created conditional backend rules (applyTo glob) | Hexagonal rules, build commands, persistence conventions were duplicated in backend agents; conditional loading saves context window |
| 3 | New: frontend.instructions.md | Created conditional frontend rules (applyTo glob) | Frontend architecture rules, TS rules, build commands were duplicated in frontend agents |
| 4 | All agents: No Suppositions | Removed (now in global) | Identical block copy-pasted in all 12 agents |
| 5 | All agents: Repository | Removed (now in global header) | Identical 4-line block in all 12 agents |
| 6 | All agents: Grounding Rules | Trimmed to agent-specific only | Generic rules (verify paths, types, namespaces) now in global; agent keeps only role-specific rules |
| 7 | Implementors: Git/Arch Rules | Removed quick-reference sections | Now in conditional .instructions.md files loaded automatically |
| 8 | All agents: Memory paths | Changed `memory/` to `memory/active/` | 3-tier model requires active files in `active/` subdirectory |
| 9 | .vscode/mcp.json | Added memory server config | Was empty; knowledge graph tools now available to agents |
| 10 | AGENTS.md | Added Memory Architecture + Instruction Layering | Human developers need to understand the new organization |

**What was working (kept):**
- Agent-specific execution workflows (Step 1-7 per agent) — untouched, these are role-specific
- Memory file templates (exact markdown structure for issue-reader, plan, implementation, code-reviewer) — preserved
- Context Loading Priority ordering per agent — only removed scan/never-preload lines that moved to global
- Skills integration (dotnet-tdd, api-exercise, hexagonal-validation, additive-review, sveltekit-dev) — untouched
- Handoff chain definitions in agent frontmatter — preserved
- Critical Rules section per agent — preserved with agent-specific content
- Activity log convention — preserved, added rotation rule

**What wasn't working (fixed):**
- 12 copies of No Suppositions (4-6 bullet points each) wasted context window in every agent invocation
- 12 copies of Repository block (4 lines each) consumed tokens for static info
- Generic grounding rules (verify file paths, types, namespaces, invariants, endpoints) repeated in every agent — ~7 rules × 12 agents = ~84 rule instances that were identical
- Git Rules in implementor agents duplicated copilot-instructions.md Git Discipline
- Hexagonal Architecture and Frontend Architecture quick-reference tables duplicated backend/frontend .instructions.md content
- Memory files stored flat in single directory — no lifecycle management, no archival, no tiered access
- MCP knowledge graph configured in copilot-instructions.md but .vscode/mcp.json was empty — tools were unavailable
- No conditional instruction loading — all rules loaded regardless of whether editing .cs or .svelte files

**Lessons learned:**
- Instruction deduplication follows the DRY principle but for prompts: shared rules in `copilot-instructions.md` (always loaded), stack-specific rules in `.instructions.md` (conditionally loaded via `applyTo` globs), and only role-specific rules in `.agent.md`. This 3-layer instruction model mirrors the 3-tier memory model.
- The `applyTo` glob pattern in `.instructions.md` files is powerful for context-aware loading — backend rules only load when editing `.cs` files, saving ~30 lines of context window during frontend work.
- AI agent limitations (context decay, hallucination, error snowballing, semantic drift) should be stated explicitly in the system prompt — agents that "know their weaknesses" can self-correct more effectively than agents given only positive instructions.
- Memory tiering (active → distilled → archive) prevents the indefinite linear growth problem that append-only systems suffer. The key insight from cognitive memory research: working memory should be small and fast, semantic memory should be compressed facts, episodic archives should never load on startup.
- Renaming "Grounding Rules" to "Agent-Specific Grounding Rules" in each agent file makes the layering explicit — agents understand that generic rules come from globals, and their section adds agent-unique rules on top.
- Existing files at the old `memory/` root need manual triage to move to `archive/` — the structural change in references means new files go to the right place, but legacy files require a one-time cleanup.

**Risks & watch items:**
- 37+ legacy memory files still at `memory/` root (not in `active/` or `archive/`) — agents referencing `memory/active/` paths won't find these old files. Manual triage needed to move completed issue files to `archive/`.
- Agent-specific grounding rules may be too thin for some agents — if an agent skips generic verification because it assumes globals handle it, but globals aren't loaded in that context, the agent may hallucinate. Monitor for this failure mode.
- The `applyTo` glob in `.instructions.md` depends on VS Code's file context detection — verify it works when agents read files outside the glob pattern.
- MCP knowledge graph tools are now available but no entity/relation data exists yet — agents need to start populating the graph during their next execution runs.

---

### 2026-04-27 — All Agents + Skills — Replace issue-writer agent with shared github-issues skill

**Retro trigger:** Backport from thermo-replacer repo. The Issue Writer was a dedicated agent that only created GitHub issues — too narrow for a full agent. The same capability is better expressed as a shared skill invocable by any agent (Planner, Software Architect, PM) that needs to create issues, eliminating the handoff bottleneck of routing through a dedicated agent.

**Files modified:**
- `.github/skills/github-issues/SKILL.md` — **Created** shared skill with 7-step workflow: select template → read template → enrich with project docs → verify context quality → search duplicates → HITL gate → create issues. Supports batch creation and Project #6 integration. Labels use bounded context conventions (`budget-management`, `forecast-engine`, `identity-household`, `shared-kernel`, `frontend`).
- `.github/ISSUE_TEMPLATE/feature.md` — **Created** user story format with bounded context, invariant, and API endpoint references
- `.github/ISSUE_TEMPLATE/bug.md` — **Created** repro steps template with bounded context reference
- `.github/ISSUE_TEMPLATE/tech-debt.md` — **Created** with affected components checklist
- `.github/ISSUE_TEMPLATE/spike.md` — **Created** with goal, timebox, research questions, expected output
- `.github/agents/software-architect.agent.md` — Removed "Hand off to Issue Writer" from YAML frontmatter handoffs; added `github-issues` skill reference for creating issues directly from design gaps
- `.github/agents/backend-planner.agent.md` — Added `github-issues` skill to skills list
- `.github/agents/frontend-planner.agent.md` — Added `github-issues` skill to skills list
- `.github/agents/backend-implementor.agent.md` — Added `github-issues` skill for follow-up issue creation
- `.github/agents/frontend-implementor.agent.md` — Added `github-issues` skill for follow-up issue creation
- `.github/agents/backend-reviewer.agent.md` — Added `github-issues` skill for filing issues from reviews
- `.github/agents/frontend-reviewer.agent.md` — Added `github-issues` skill for filing issues from reviews
- `.github/copilot-instructions.md` — Updated Workflow 4 and 5 to reference `github-issues` skill instead of Issue Writer agent
- `AGENTS.md` — Updated sections 3 (Project Startup) and 4 (Design Review) to replace Issue Writer with skill references
- `.github/agents/agent-improver.agent.md` — Marked `issue-writer.agent.md` as "Pending deletion"; added `github-issues/SKILL.md` to inventory

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | New skill | Created `github-issues` skill with template-driven workflow | Issue creation is a capability, not a role — any agent that discovers work should be able to file issues without a dedicated handoff |
| 2 | Issue templates | Created 4 templates (feature, bug, tech-debt, spike) | Standardize issue format across all creation paths — agents and humans use the same templates |
| 3 | Architect handoffs | Removed Issue Writer handoff; added skill reference | Architect can now create issues directly via skill instead of handing off to a separate agent |
| 4 | All planners/implementors/reviewers | Added `github-issues` skill | Any agent that discovers follow-up work can file issues inline |
| 5 | Workflows 4 & 5 | Replaced "Issue Writer" with "github-issues skill" | Pipeline is shorter — no dedicated agent hop for issue creation |

**What was working (kept):**
- HITL confirmation gate before bulk issue creation (carried forward from Issue Writer)
- Label conventions for bounded contexts
- Duplicate detection before creation
- Software Architect's design-gaps.md format as input for issue creation

**What wasn't working (fixed):**
- Dedicated Issue Writer agent was a bottleneck — every issue creation required routing through it
- Agents that discovered follow-up work (reviewers finding bugs, implementors discovering tech debt) had no mechanism to file issues inline
- Issue format was ad-hoc when different agents created issues — no shared templates

**Lessons learned:**
- When a capability is used by multiple agents but doesn't require persistent state or a unique execution workflow, it's a skill — not an agent. The litmus test: if the "agent" has no memory files, no context loading priority, and no multi-step workflow beyond its single capability, it should be a skill.
- Shared issue templates ensure consistency regardless of which agent (or human) creates the issue — the template is the contract, not the creator.
- Removing a handoff hop (Agent A → Issue Writer → downstream) in favor of inline skill invocation (Agent A uses skill directly) reduces latency and error propagation in multi-agent chains.

**Risks & watch items:**
- `issue-writer.agent.md` is marked "Pending deletion" — should be deleted once confirmed no other references exist
- Agents may create issues with inconsistent quality if they skip the skill's quality checks — the HITL gate mitigates this
- Label creation (e.g., `priority:p0`) requires GitHub permissions — verify agents can create labels if they don't exist

---

### 2026-04-27 — All Agents + Skill — Add distill-knowledge skill and agent learnings capture

**Retro trigger:** Backport from thermo-replacer repo. Knowledge was accumulating in Tier 1 memory files but never compressed into Tier 2 (`knowledge.md`). Agents had no structured convention for recording what they learned during execution — decisions, patterns, and gotchas evaporated between sessions. The MCP knowledge graph was available but Tier 2 had no defined lifecycle.

**Files modified:**
- `.github/skills/distill-knowledge/SKILL.md` — **Created** 7-step procedure: identify issue → read Tier 1 files → extract learnings → dedup against knowledge.md → write new entries → archive raw files → report. Categories adapted for BudgetManager: Architecture Decisions, Domain Invariants, Codebase Conventions, Build & Tooling, API Patterns, Persistence Patterns, Cross-Context Communication, Review Findings, Frontend Patterns, Gotchas.
- `.github/agents/backend-planner.agent.md` — Added Step 6 "Record Learnings" with Decisions/Patterns/Gotchas format
- `.github/agents/frontend-planner.agent.md` — Added "Record Learnings" section at end of workflow
- `.github/agents/backend-implementor.agent.md` — Added Step 9 "Record Learnings" after memory file write step
- `.github/agents/frontend-implementor.agent.md` — Added Step 6 "Record Learnings" after memory file template
- `.github/agents/backend-reviewer.agent.md` — Added "Record Learnings" section after Hand Off
- `.github/agents/frontend-reviewer.agent.md` — Added "Record Learnings" section after Critical Rules
- `.github/agents/software-architect.agent.md` — Added "Record Learnings" section before Critical Rules
- `.github/agents/product-manager.agent.md` — Added "Record Learnings" section before Handoff Chain
- `.github/agents/ui-designer.agent.md` — Added Step 8 "Record Learnings"; renumbered Hand Off to Step 9
- `.github/copilot-instructions.md` — Updated Tier 2 definition to reference `distill-knowledge` skill; added agent Learnings convention to Agent Conventions section
- `AGENTS.md` — Updated Tier 2 lifecycle description

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | New skill | Created `distill-knowledge` skill | No mechanism existed to compress Tier 1 → Tier 2; knowledge evaporated between sessions |
| 2 | All 9 non-improver agents | Added "Record Learnings" section | Agents need a structured way to capture decisions, patterns, and gotchas during execution — raw material for the distill skill |
| 3 | copilot-instructions.md | Updated Tier 2 definition + agent conventions | Tier 2 lifecycle was vague ("updated after each sprint"); now concrete: "updated after each issue via distill-knowledge skill" |
| 4 | AGENTS.md | Updated Tier 2 lifecycle | Human-facing docs must match agent-facing instructions |

**What was working (kept):**
- 3-Tier memory model structure (Active → Distilled → Archive)
- Memory file templates for each agent role
- Tier 1 naming conventions (task-context, plan, implementation, code-reviewer)
- knowledge.md as the distilled knowledge store

**What wasn't working (fixed):**
- No convention for agents to record learnings during execution — decisions and gotchas were lost
- No compression step from Tier 1 → Tier 2 was defined beyond "updated after each sprint"
- knowledge.md had no structured entry format — agents wouldn't know what to write

**Lessons learned:**
- Knowledge capture must be lightweight enough to not disrupt flow — a simple Decisions/Patterns/Gotchas list at the end of each agent's output is the minimum viable capture. Heavier formats (full narratives, structured JSON) would be skipped.
- The distill step should be a separate invocation (user-triggered) rather than automatic — agents shouldn't self-distill because they may over-compress or mis-categorize their own learnings.
- Categories in knowledge.md should map to the project's actual architecture (bounded contexts, hexagonal layers, API patterns) rather than generic software categories — agents can then grep for relevant knowledge by domain area.

**Risks & watch items:**
- Agents may produce low-quality Learnings sections (too vague, too verbose, or restating the obvious) — review the first few runs and refine the prompt if needed
- The distill-knowledge skill deduplication step requires reading the full knowledge.md — if the file grows very large, this may hit context window limits
- Learnings sections add ~5-10 lines to each agent's output — monitor whether this creates noise in the memory files

---

### 2026-04-27 — All Agents + Skill — Replace issue-reader agent with shared task-context skill

**Retro trigger:** Backport from thermo-replacer repo. The Issue Reader was a single-purpose agent that fetched GitHub issue context and wrote a memory file. It had no multi-step workflow, no persistent state, and no unique execution strategy — it was a context-gathering function dressed as an agent. Converting it to a skill allows any agent (Planner, Reviewer, Implementor resuming work) to gather task context inline without a dedicated handoff.

**Files modified:**
- `.github/skills/task-context/SKILL.md` — **Created** with two modes: Mode 1 (Primary Gather: fetch issue, read codebase, write task-context file) and Mode 2 (Completeness Check: validate existing file against 8-item checklist). Architecture Context section includes bounded context, invariants table, API endpoints table, domain events table, cross-context interactions. Memory file naming: `task-context-<issue-number>.md`.
- `.github/agents/backend-planner.agent.md` — Replaced issue-reader-* references with task-context-*; updated pre-flight to use task-context skill (Mode 1 or Mode 2); added `task-context` to skills list
- `.github/agents/frontend-planner.agent.md` — Same pattern as backend-planner
- `.github/agents/backend-implementor.agent.md` — Added `task-context` skill (Mode 2) to skills list
- `.github/agents/frontend-implementor.agent.md` — Added `task-context` skill (Mode 2) to skills list
- `.github/agents/backend-reviewer.agent.md` — Changed context loading from `issue-reader-*` to `task-context-*`; added `task-context` skill (Mode 2)
- `.github/agents/frontend-reviewer.agent.md` — Same pattern as backend-reviewer
- `.github/copilot-instructions.md` — Updated Workflow 1 to remove Issue Reader; updated Tier 1 naming example
- `AGENTS.md` — Updated Delivery Pipeline to remove Issue Reader; changed naming convention from `issue-reader-*` to `task-context-*`
- `.github/skills/resume/SKILL.md` — Updated memory file paths from `issue-reader-*` to `task-context-*` and from `memory/` to `memory/active/`
- `.github/agents/agent-improver.agent.md` — Marked `issue-reader.agent.md` as "Pending deletion"; added `task-context/SKILL.md` to inventory

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | New skill | Created `task-context` skill with Mode 1 (gather) and Mode 2 (check) | Issue Reader was a single-function agent — same litmus test as Issue Writer: no memory, no multi-step workflow, no unique execution strategy |
| 2 | Planner pre-flights | Check for task-context file, invoke Mode 1 or Mode 2 | Planners no longer wait for a separate agent — they gather context inline |
| 3 | All agents | Renamed issue-reader-* → task-context-* in references | New naming reflects the skill's role (gathering task context) not the retired agent's name |
| 4 | Implementors/Reviewers | Added task-context Mode 2 | Downstream agents can validate context completeness without re-gathering from scratch |
| 5 | Resume skill | Updated memory file paths | Must reference new naming convention to find files correctly |

**What was working (kept):**
- The task-context memory file structure (metadata, issue body, codebase context, architecture context)
- Completeness checklist (8 items covering bounded context, invariants, API endpoints, etc.)
- GitHub issue fetching workflow (read issue, parse body, extract labels)
- Mode 2 as a quality gate for downstream agents

**What wasn't working (fixed):**
- Issue Reader required a dedicated handoff hop before every planning session — added latency
- Only planners could trigger context gathering — implementors and reviewers that needed fresh context had no mechanism
- "Issue Reader" naming implied the agent only read issues — the actual capability was broader (codebase exploration, architecture cross-referencing)

**Lessons learned:**
- The agent-vs-skill litmus test is clear: if the unit has no persistent state, no multi-step execution workflow, and no unique tool requirements beyond reading/writing files, it's a skill. Both Issue Writer and Issue Reader failed this test.
- Dual-mode skills (Mode 1: create, Mode 2: validate) are effective for context files — the creator mode runs once, the validator mode runs every time a downstream agent loads the file, catching staleness without re-gathering.
- Renaming memory files during a skill migration requires updating every reference across all agents, skills, and documentation — a grep for the old name across `.md` files is essential to avoid orphaned references.
- Converting agents to skills shortens the pipeline by removing handoff hops — this directly reduces error compounding in multi-agent chains (fewer handoffs = fewer opportunities for context loss or misinterpretation).

**Risks & watch items:**
- `issue-reader.agent.md` is marked "Pending deletion" — should be deleted once confirmed no other references exist
- Legacy `issue-reader-*.md` memory files in `memory/active/` or `memory/archive/` still use the old naming — the resume skill now looks for `task-context-*`, so old files won't be found automatically. Manual renaming or a compatibility note may be needed.
- Planners now have more responsibility (context gathering + planning) — monitor whether this overloads their context window compared to the previous split

---

### 2026-04-27 — All Agents + Global Instructions — Add HITL (Human In The Loop) gates

**Retro trigger:** Backport from thermo-replacer repo. All thermo-replacer agents had explicit HITL gate steps — confirmation points where agents must pause and get user approval via `vscode/askQuestions` before proceeding. BudgetManager agents were missing these gates entirely (planners, implementors, reviewers, architect) or had only informal confirmation language without explicit HITL labeling (PM, UI Designer). Without structured gates, agents could execute entire workflows without any human checkpoint.

**Files modified:**
- `.github/copilot-instructions.md` — Added `## HITL (Human In The Loop) Gates` section with per-agent gate table; added HITL agent convention line
- `.github/agents/backend-planner.agent.md` — Added Step 5 "Present Plan & Get Confirmation (HITL Gate)"; renumbered Steps 6-8; added HITL critical rule
- `.github/agents/frontend-planner.agent.md` — Added Step 5 "Present Plan & Get Confirmation (HITL Gate)"; added HITL critical rule
- `.github/agents/backend-implementor.agent.md` — Added Step 0.5 "Confirm High-Level Approach (HITL Gate)"; added Step 7 "Confirm Before Opening PR (HITL Gate)"; renumbered Steps 8-10; added HITL critical rules
- `.github/agents/frontend-implementor.agent.md` — Added Step 0.5 "Confirm High-Level Approach (HITL Gate)"; added Step 4 "Confirm Before Opening PR (HITL Gate)"; renumbered Steps 5-7; added HITL critical rules
- `.github/agents/backend-reviewer.agent.md` — Added Step 11 "Confirm Findings (HITL Gate)"; added HITL critical rule
- `.github/agents/frontend-reviewer.agent.md` — Added Step 10 "Confirm Findings (HITL Gate)"; added HITL critical rule
- `.github/agents/software-architect.agent.md` — Added Phase 3.5 "Confirm Architecture Plan (HITL Gate)"; added Phase 3.6 "Suggest PRD/Design Changes (HITL Gate)"; added Mode B confirm findings step; added HITL critical rule
- `.github/agents/ui-designer.agent.md` — Added explicit HITL labels to screen plan confirmation (Step 2), between-screen confirmation (Step 4), and new Step 8.5 "Confirm Before Handoff (HITL Gate)"; updated critical rules with HITL labels
- `.github/agents/product-manager.agent.md` — Added explicit HITL labels to State 2 confirmation and State 4 finalization; updated critical rules with `vscode/askQuestions` reference

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | copilot-instructions.md | Added HITL Gates section with per-agent table + enforcement rule | No centralized definition of where human checkpoints should occur |
| 2 | copilot-instructions.md | Added HITL agent convention | HITL gates were not part of the shared agent conventions |
| 3 | Backend/Frontend Planner | Added "Present Plan & Get Confirmation" step before writing to memory | Planners wrote plans to memory without user review — incorrect plans propagated downstream |
| 4 | Backend/Frontend Implementor | Added "Confirm Approach" before coding + "Confirm Before PR" before pushing | Implementors could execute an entire plan and open a PR without any human checkpoint |
| 5 | Backend/Frontend Reviewer | Added "Confirm Findings" before posting to GitHub | Reviewers could post findings to GitHub without human verification — false positives create noise |
| 6 | Software Architect | Added architecture plan + design changes HITL gates | Architect could produce artifacts based on misunderstood requirements without checkpoint |
| 7 | UI Designer | Added explicit HITL labels + confirm-before-handoff | Had informal confirmation language but no structured gates; missing handoff confirmation |
| 8 | Product Manager | Added explicit HITL labels + `vscode/askQuestions` refs | Had informal "ask for confirmation" but no explicit HITL labeling or tool reference |

**What was working (kept):**
- All existing execution workflows (step structures preserved, only insertion/labeling added)
- Product Manager's State 2 synthesis confirmation (now labeled as HITL gate)
- UI Designer's screen plan confirmation (now labeled as HITL gate)
- All critical rules sections (HITL rules added, existing rules unchanged)

**What wasn't working (fixed):**
- Planners wrote plans to memory without user confirmation — incorrect plans propagated to implementors
- Implementors could run the entire plan and open PRs without any human checkpoint
- Reviewers could post findings to GitHub PR threads without human verification
- Software Architect had no confirmation step between analysis and artifact production
- UI Designer had no explicit handoff confirmation and no between-screen HITL label
- Product Manager had informal confirmation language without `vscode/askQuestions` tool reference

**Lessons learned:**
- HITL gates must be explicit steps in the workflow with the "(HITL Gate)" label in the heading — informal language like "ask for confirmation" is too easily skipped or reinterpreted by agents
- The `vscode/askQuestions` tool must be named explicitly in HITL instructions — without it, agents may "confirm" by stating their plan in chat and proceeding without waiting for a response
- A centralized HITL table in global instructions creates accountability — agents can cross-reference their own gates, and the Agent Improver can audit compliance
- HITL gates at handoff boundaries (before writing to memory, before opening PR, before posting review) are the most critical — they prevent incorrect state from propagating to downstream agents

**Risks & watch items:**
- HITL gates add latency to every workflow — monitor whether users find them too frequent or want to batch-approve multiple gates
- Implementors now have two HITL gates (before coding + before PR) — for small fixes, this may feel excessive. Consider adding a "skip HITL for trivial changes" escape hatch if users complain
- Some agents may interpret "wait for confirmation" as blocking indefinitely — ensure agents present their question clearly and don't proceed until an explicit response arrives

---

### 2026-04-19 — Frontend Implementor — Add mandatory browser testing with DevTools MCP

**Retro trigger:** The Frontend Implementor was completing features (code + tests passing) without ever opening a browser to visually verify the pages. Despite having `chrome-devtools-mcp/*` in its tool list and the plan noting "UI implementation PRs must include screenshot evidence," the agent's workflow had no step requiring browser testing. Code that passes tests but looks broken in the browser was being committed and pushed.

**Files modified:**
- `.github/agents/frontend-implementor.agent.md` — Added Step 1d-bis (Visual Verification), updated Self-Verification Checkpoint, added Critical Rule, updated PR body template

**What changed & why:**

| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | Step 1 (feature loop) | Added Step **1d-bis: Visual Verification (Browser Testing)** between "Run Tests" (1d) and "Self-Verification Checkpoint" (1e) | Core fix — agent had DevTools MCP tool but no workflow step requiring its use. Includes 6-step procedure: start dev server, navigate to pages, interact with features, take screenshots, save to `artifacts/issue-<N>/`, fix visual issues before committing. Also lists what to verify visually (layout, colors, interactions, error states, loading states). |
| 2 | Self-Verification Checkpoint (1e) | Added item #7: "Visual verification screenshots exist in `artifacts/issue-<N>/` for this feature group" with redirect back to 1d-bis if missing | Cross-check that browser testing actually happened — prevents the agent from skipping 1d-bis and proceeding to commit |
| 3 | Critical Rules | Added: "Browser testing is mandatory — never commit a feature without first opening it in a real browser via DevTools MCP (preferred) or Playwright MCP (fallback), interacting with it, and saving screenshots to `artifacts/issue-<N>/`" | Hard enforcement at the rules level, not just a workflow step that could be skipped |
| 4 | PR Body template (Step 5) | Added `## Screenshots` section with instruction to embed images from `artifacts/issue-<N>/` | Screenshots committed to the repo are referenced in the PR body so reviewers see visual evidence alongside code changes |


**What was working (kept):**
- Existing workflow structure (HITL gates, commit discipline, pre-flight checks, type check → lint → test loop)
- All existing Critical Rules preserved (never force push, never merge, follow plan, etc.)
- Self-Verification Checkpoint items 1-6 preserved
- PR body template existing sections (Summary, Changes, Test Results) preserved
- `chrome-devtools-mcp/*` was already in the tool list — no tool access change needed

**What wasn't working (fixed):**
- Agent had browser testing tools available but zero workflow steps requiring their use — tools alone don't drive behavior, workflow steps do
- Code was committed after passing type check + lint + tests but without any visual verification — "green tests ≠ working UI"
- PR had no visual evidence section — reviewers couldn't assess UI quality from the PR alone
- The plan's "Patterns" section noted screenshot requirements, but plan-level notes don't override agent workflow — the agent follows its own `.agent.md` steps

**Lessons learned:**
- **Tools in the frontmatter are necessary but not sufficient.** An agent with `chrome-devtools-mcp/*` in its tool list will never use it unless a workflow step explicitly tells it to. Tool access is permission; workflow steps are instructions.
- **Visual verification must be a blocking workflow step, not a suggestion.** Agents optimize for completing steps in order — if browser testing isn't a numbered step between "tests pass" and "commit," it won't happen.
- **The Self-Verification Checkpoint is the right place for a cross-check** — adding a "screenshots exist" item creates a safety net if the agent somehow skips the visual verification step.
- **Screenshot evidence in PRs benefits both the agent and human reviewers.** The agent is forced to produce concrete artifacts, and reviewers get visual context alongside code diffs.

**Risks & watch items:**
- DevTools MCP requires Chrome to be running/accessible — if the browser isn't available, the agent may get stuck. The instruction specifies Playwright MCP as fallback, but Playwright MCP is not currently configured in `.vscode/mcp.json`
- Starting the dev server (`pnpm dev`) and taking screenshots adds time to each feature group — monitor whether this significantly slows implementation
- The agent may take low-quality screenshots (wrong page, empty state, no interaction) — the "interact with the page" instruction and "what to verify visually" checklist mitigate this, but quality depends on agent judgment
- `artifacts/issue-<N>/` directory must be git-committed for PR image references to work — the agent's `git add -A` already covers this

---

### 2026-05-01 — Frontend Implementor — Enforce browser testing with hard gates at commit and PR

**Retro trigger:** PR #91 (issue #56 — Forecast Chart & Multi-Version Overlay) confirmed that the 2026-04-19 addition of Step 1d-bis (Visual Verification) was insufficient. The agent skipped browser testing entirely — no dev server was opened, no screenshots were taken. The PR's `## Browser Testing` section contained only text bullets ("Missing budgetId guard verified", "API error state verified") with no embedded screenshots. The `## Screenshots` section from the PR template was absent from the PR body. Also identified that fix cycles (address-pr-feedback) had the same gap: the agent didn't re-verify UI changes after addressing reviewer comments.

**Files modified:**
- `.github/agents/frontend-implementor.agent.md` — Promoted Visual Verification from sub-step (1d-bis) to first-class step (1e); shifted 1e→1f, 1f→1g, 1f-bis→1g-bis, 1g→1h, 1h→1i; added `⛔ STOP` block at commit step (1g); strengthened Self-Verification Checkpoint item 7 to a HARD BLOCK; added screenshot checklist items to HITL gate (Step 4); added `⛔ STOP` block before PR push (Step 5); added "Re-verify after feedback" to Critical Rules.
- `.github/skills/address-pr-feedback/SKILL.md` — Added Step 6b (Visual Re-verification) with 6-step procedure, required/skippable criteria, and `⛔ STOP` before commit; added `⛔ STOP` guard on Step 8 commit; added "Re-verify visually on frontend fix cycles" Critical Rule with thread reply convention.

**What changed & why:**
| # | Section | Change | Rationale |
|---|---------|--------|-----------|
| 1 | Step 1 | Renamed `1d-bis` → `1e`; added "REQUIRED" to heading | "bis" signals optional/secondary — agent treated it as skippable. Numbered steps carry more weight. |
| 2 | Step 1e header | Added "non-negotiable" + "NOT ready to commit" language | Previous text said "not optional" but agent still skipped it; stronger action-blocking language |
| 3 | Self-Verification Checkpoint (1f) item 7 | Changed "go back to Step 1d-bis" → "**HARD BLOCK**: return to Step 1e immediately. You may NOT proceed past this checkpoint without screenshots on disk." | Soft redirect was being ignored; blocking language makes skipping undeniable |
| 4 | Commit step (1g) | Added `⛔ STOP` block before `git commit` command | No enforcement existed at the commit action — agent could run `git commit` without screenshots |
| 5 | HITL gate (Step 4) | Added two checklist items: (a) screenshots in artifacts/ for every feature group, (b) PR body Screenshots section contains embedded image links | HITL gate now explicitly gates on screenshot evidence before user confirms PR |
| 6 | Step 5 PR creation | Added `⛔ STOP` block before `git push` | PR could be created with empty Screenshots section — now explicitly blocked |
| 7 | Critical Rules | Added "Re-verify after feedback" rule | Fix cycles (address-pr-feedback) were not covered by any visual verification requirement — agent pushed fix commits without re-testing UI |

**What was working (kept):**
- Commit discipline, test coverage/naming, review cycle handling (RP tracking), activity log and memory file updates
- All existing Visual Verification content (procedure, what-to-verify checklist, DevTools MCP instructions) — preserved, only the step header and surrounding guards changed
- HITL gate structure (Step 4 and Step 0.5) — extended, not replaced
- PR body template structure (Summary, Changes, Test Results, Screenshots sections) — preserved

**What wasn't working (fixed):**
- Agent skipped browser testing entirely despite having DevTools MCP tool access and a Step 1d-bis instruction
- The "bis" sub-step designation signaled low priority — agents skip optional-looking steps
- No hard gate existed between "tests pass" and `git commit` — agent could commit without screenshots on disk
- No hard gate existed between commit and `git push` — PR could be created with empty Screenshots section
- The HITL gate (Step 4) didn't ask about screenshots — agent could get user's "proceed" confirmation without screenshot evidence being surfaced
- Fix cycles (address-pr-feedback) had no visual re-verification requirement — UI changes after review were pushed without visual confirmation

**Lessons learned:**
- **Numbered steps enforce priority; "bis" sub-steps do not.** Agents process workflow steps as an ordered list — a `1d-bis` between `1d` and `1e` is mentally parsed as "optional extra" between two required steps. Promote blocking requirements to full step numbers.
- **Hard gates must appear at the action level, not only at the review level.** A self-verification checklist item ("screenshots exist?") is overridden by the agent's momentum toward completing the workflow. A `⛔ STOP` block immediately before the `git commit` command creates friction at the moment of action.
- **Text-only browser testing reports are a red flag pattern.** An agent claiming to have "verified X state" without screenshots is performing description, not verification. Instructions must require artifact production (screenshots on disk), not just behavioral confirmation.
- **HITL gates should surface quality evidence, not just action confirmations.** "Should I open the PR?" is less effective than "I see screenshots in artifacts/ for all 4 feature groups — confirm?" — the latter forces the user to acknowledge the evidence exists.
- **Fix cycles need their own re-verification requirement.** Initial implementation gates don't transfer to subsequent push events in the address-pr-feedback flow. Every workflow that ends with a `git push` needs explicit visual verification requirements.

**Risks & watch items:**
- Multiple `⛔ STOP` blocks add friction to the workflow — if the agent interprets them as interactive blockers (waiting for user response) rather than self-checks, it may stall. Monitor whether the blocks are treated as self-verification or blocking prompts.
- Fix cycles note is in Critical Rules but the `address-pr-feedback` skill itself has been updated in the same session — Step 6b (Visual Re-verification) added with required/optional criteria, `⛔ STOP` before commit, and Critical Rule with thread reply convention. Gap fully closed.
- If the dev server is not running (no Docker, no backend, API errors), the agent may take screenshots of error states and argue those satisfy the requirement. The "what to verify visually" checklist should help distinguish valid from invalid screenshots.
