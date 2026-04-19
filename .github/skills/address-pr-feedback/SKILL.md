---
name: address-pr-feedback
description: "Read PR review comments, fix code, reply to GitHub threads, and push. Used by the Implementor to address reviewer feedback without going through the Planner for straightforward fixes."
---

# Address PR Feedback Skill

This skill enables the **Implementor** to directly address PR review feedback: read review comments, fix code, reply to GitHub threads, and push updates — without routing through the Planner for straightforward fixes.

## When to Use This Skill

Use this skill when:
- The Reviewer has posted review comments on your PR
- You need to address feedback and push fixes
- The PR already exists and has at least one review round

Do NOT use this skill for:
- The initial implementation (use the implementation plan instead)
- Creating a new PR (use the normal implementation workflow)

## Pre-flight Check

Before starting, verify:
1. You know the PR number and issue number
2. The PR exists and has review comments
3. The review memory file exists: `.github/agents/memory/active/code-reviewer-<issue-number>.md`
4. Your feature branch is checked out and clean (`git status`)

If any check fails, STOP and ask the user.

## Escalation Rules — When to Consult the Planner

**CRITICAL:** Not all review feedback is a simple code fix. Before addressing each review point, classify it:

### Handle Directly (no Planner needed)
- Bug fixes (null checks, off-by-one, missing validation)
- Convention violations (naming, formatting, import order)
- Missing tests for existing functionality
- Documentation or comment improvements
- Simple refactors (extract method, rename variable)
- Adding error handling for identified edge cases

### Escalate to Planner Sub-agent
Invoke the Planner as a sub-agent when:
- The feedback **questions the design approach** (e.g., "should this use a value object instead of a primitive?")
- The feedback **suggests a different architecture** (e.g., "this should be in the domain layer, not the handler")
- The feedback **identifies a missing feature** that wasn't in the original plan
- The feedback **conflicts with the implementation plan** — the reviewer wants something the plan didn't specify
- The feedback **affects multiple files or layers** in a way that needs coordinated planning
- You are **unsure whether the fix is correct** or could introduce a regression

When escalating, invoke the Planner sub-agent with:
```
Review point RP-<ID> on PR #<number> requires a design decision:
- Reviewer's comment: <quote>
- File: <path> Line: <line>
- My concern: <why you're escalating>
Please advise on the approach.
```

The Planner sub-agent will respond with guidance. Apply the guidance, then continue with the remaining review points.

### Escalation Signals Checklist

Ask yourself these questions for each review point. If ANY answer is "yes", escalate:
- [ ] Does this change the public API of a class or interface?
- [ ] Does this move code between hexagonal layers (Domain ↔ Application ↔ Infrastructure)?
- [ ] Does the reviewer's suggestion contradict the implementation plan?
- [ ] Would this fix require changing more than 3 files?
- [ ] Am I unsure what the reviewer is asking for?

## Step-by-Step Workflow

### Step 1: Load Context

1. Read the review memory file: `.github/agents/memory/active/code-reviewer-<issue-number>.md`
2. Parse the `## Review Points` table — extract all points with status `OPEN`
3. Read the implementation plan: `.github/agents/memory/active/plan-<issue-number>.md` (needed for escalation decisions)
4. Fetch PR review comments from GitHub to get the full comment text and thread context

### Step 2: Classify and Prioritize Review Points

For each `OPEN` review point:

1. **Read the full GitHub comment thread** (not just the summary in the memory file — the thread may have follow-up discussion)
2. **Classify** as "Handle Directly" or "Escalate to Planner" using the rules above
3. **Group** by file to minimize context switches
4. **Order** by:
   - ❌ CRITICAL severity first
   - ⚠️ WARNING second
   - ℹ️ INFO last

### Step 3: Present Plan to User (HITL Gate)

Before making any changes, present to the user via `vscode/askQuestions`:

> **Note:** If the plan is too detailed for the `vscode/askQuestions` tool (more than a few sentences), write the full plan to the chat response first — listing each review point, its classification (direct fix vs. escalate), and the intended fix approach. Then ask a short confirmation question via the tool.

Present:
- Number of OPEN review points to address
- Which points you'll handle directly (with brief fix description)
- Which points you'll escalate to the Planner sub-agent (with reason)
- Any points you believe should be marked WONTFIX (with justification)

**Wait for user confirmation before proceeding.**

### Step 4: Fix Code

For each "Handle Directly" review point, in the grouped/prioritized order:

1. **Read the current file** around the cited line
2. **Apply the fix** — make the minimal change that addresses the reviewer's feedback
3. **Verify the fix** — re-read the modified area to confirm:
   - The fix matches what the reviewer requested
   - The surrounding code still makes sense
   - No imports are broken
   - No new lint issues are introduced

After each file's fixes are complete, build to catch errors early:
- **Backend:** `dotnet build`
- **Frontend:** `cd frontend; pnpm check`

### Step 5: Escalate Where Needed

For each "Escalate" review point:
1. Invoke the Planner as a sub-agent with the escalation prompt (see Escalation Rules above)
2. Apply the Planner's guidance
3. If the Planner's guidance is unclear, ask the user via `vscode/askQuestions`

### Step 6: Run Full Verification

After all fixes are applied:

**Backend:**
```powershell
dotnet build
dotnet test
```

**Frontend:**
```powershell
cd frontend; pnpm check && pnpm lint && pnpm test
```

ALL must pass. If tests fail, fix them before proceeding.

### Step 7: Reply to GitHub Threads

For each addressed review point, reply to the GitHub comment thread:

- **For direct fixes:** Reply with a brief description of what was changed:
  ```
  Fixed — [brief description of change, e.g., "added guard clause at L42", "renamed to match convention"]
  ```
- **For escalated points:** Reply with the decision and the fix:
  ```
  Discussed with team lead — [brief description of decision and change made]
  ```
- **For WONTFIX points (if user approved):** Reply with justification:
  ```
  Won't fix — [reason, e.g., "accepted trade-off per user confirmation"]
  ```

**Rules for GitHub thread replies:**
- Keep replies concise — one sentence per point
- Reference the specific line or change, not just "fixed"
- Never argue with the reviewer — just describe what was done
- If the reviewer's comment was unclear and you interpreted it, state your interpretation: "Interpreted as [X] — [description of fix]"

### Step 8: Commit and Push

```powershell
git add -A
git commit -m "fix(<context>): address PR review feedback for #<issue-number>"
git push
```

### Step 9: Update Memory File

Update the implementation memory file (`implementation-<issue-number>.md`) with:
```markdown
## PR Feedback Round <N>
- **Date:** <YYYY-MM-DD>
- **Review Points Addressed:** <list of RP IDs>
- **Escalated to Planner:** <list of RP IDs, or "None">
- **WONTFIX:** <list of RP IDs, or "None">
- **Commit:** <sha>
```

### Step 10: Request Re-review (HITL Gate)

Present to the user via `vscode/askQuestions`:
- Summary of all fixes made
- Any escalated decisions
- Ask if they want to hand off to the Reviewer for re-review

If confirmed, use the handoff to the Reviewer agent for the next additive review round.

## Critical Rules

- **Classify before fixing** — always determine direct-fix vs. escalate before writing any code
- **HITL before coding** — present the plan to the user before making changes
- **Build after each file** — catch compilation errors early, don't let them accumulate
- **Never skip thread replies** — every addressed review point must have a GitHub thread reply
- **Never argue with the reviewer** — just describe what was done
