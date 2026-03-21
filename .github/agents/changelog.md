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
