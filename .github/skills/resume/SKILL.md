---
name: resume
description: "Resume interrupted work on a feature implementation. Use when a coding session was interrupted mid-implementation and you need to figure out where you left off."
---

# Resume Interrupted Work Skill

This skill helps you resume work after a session interruption. It reconstructs the current state from git history, memory files, and plan progress markers.

## When to Use

- A previous agent session was interrupted before completing the plan
- You're asked to "continue" or "pick up where we left off"
- You're told to resume work on a specific issue number

## Step 1: Identify the Issue

If the issue number isn't provided, check:
```powershell
# Check current branch name (contains issue number)
git branch --show-current

# Check recent commits for issue references
git log --oneline -10
```

## Step 2: Read Memory Files

Read ALL memory files for the issue, in this order:

1. `.github/agents/memory/issue-reader-<issue-number>.md` — Original issue context
2. `.github/agents/memory/plan-<issue-number>.md` — Implementation plan
3. `.github/agents/memory/implementation-<issue-number>.md` — Implementation output (if exists)
4. `.github/agents/memory/code-reviewer-<issue-number>.md` — Review feedback (if exists)

## Step 3: Check Plan Progress Markers

Search the plan file for progress markers:
```powershell
Select-String -Pattern "<!-- PROGRESS:" -Path ".github/agents/memory/plan-*.md"
```

This tells you which feature groups were completed and their commit hashes.

## Step 4: Check Git State

```powershell
# Current branch
git branch --show-current

# Uncommitted changes
git status --short

# Last 10 commits on this branch (with files)
git log --oneline --stat -10

# Diff against master (what's been done total)
git diff --stat master

# Files changed since master
git diff --name-only master
```

## Step 5: Verify Build State

### Backend (.NET)
```powershell
dotnet build
dotnet test
```

### Frontend (SvelteKit)
```powershell
cd frontend; pnpm check && pnpm lint && pnpm test
```

Run the appropriate commands based on which stack the feature branch targets. If unsure, check the plan memory file for `**Stack:**` metadata.

Record whether the current state builds and tests pass.

## Step 6: Reconstruct Progress

Compare the plan's feature groups against:
1. Progress markers in the plan file
2. Committed files (from `git diff --name-only master`)
3. Uncommitted changes (from `git status`)

Build a status report:

```markdown
## Resume Assessment

### Issue: #<number> — <title>
### Branch: `<branch-name>`

### Plan Status
| Feature Group | Status | Evidence |
|---|---|---|
| Feature 1: <title> | ✅ Committed | Commit <hash> |
| Feature 2: <title> | 🔄 Partial | Files exist but tests fail |
| Feature 3: <title> | ❌ Not Started | No matching files |

### Current State
- **Build:** ✅ PASS / ❌ FAIL (error: ...)
- **Tests:** X passed, Y failed
- **Uncommitted changes:** <list of files>

### Recommended Next Action
<Specific instruction: "Continue with Feature 3 from the plan" or "Fix failing tests in Feature 2 before proceeding">
```

## Step 7: Resume Execution

Based on the assessment:

- **If mid-feature (partial files, failing tests):** Complete the current feature first
- **If between features (last feature committed, next not started):** Start the next feature group
- **If all features done but no PR:** Run validation (hexagonal for backend, type check + lint for frontend), then push and open PR
- **If PR exists but review has issues:** Read the review memory file and plan fixes

Continue following the **Backend Implementor** or **Frontend Implementor** workflow (depending on the stack) from the identified resume point.

## Important

- **Never re-implement completed features** — check git log to confirm what's done
- **Never skip the build/test check** — you need to know the current state before making changes
- **If the plan has been modified** since the last session, re-read it fully to catch changes