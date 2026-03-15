---
name: additive-review
description: "Implements the additive PR review workflow: detects review round, computes file-state baselines, tracks review point resolution via GitHub API, and applies staleness thresholds. Used by the PR Reviewer agent."
---

# Additive PR Review Skill

This skill implements the **additive review model** — each review round builds on the previous one instead of starting from scratch. The reviewer tracks which prior findings were addressed, which remain open, and scans only the changed files for new issues.

## Core Concepts

| Concept | Definition |
|---|---|
| **Baseline** | A snapshot of reviewed file paths + git blob SHAs taken at the end of each review round |
| **Review Point (RP)** | A single finding with a unique ID, severity, status, and optional GitHub comment ID |
| **Delta** | The set of files whose blob SHA differs between the previous baseline and the current HEAD |
| **Staleness Threshold** | If the delta exceeds **10 files**, fall back to a full review and reset the baseline |
| **Review Round** | An incrementing counter (Round 1, Round 2, …) — first review is always Round 1 |

## Review Mode Detection

At the start of every review, determine the mode:

```
IF memory file `.github/agents/memory/code-reviewer-<issue>.md` exists
   AND contains a `## Baseline` section with at least one entry
THEN mode = ADDITIVE (Round N+1)
ELSE mode = FULL (Round 1)
```

Both modes use the **same memory file format** — Round 1 simply has an empty "Prior Review Points" section.

## Staleness Check (Additive Mode Only)

Before running an additive review, check whether the diff is too large:

```powershell
# Get the list of files changed in the PR (against base branch)
$prFiles = git diff --name-only origin/master...HEAD

# Compare against baseline — count files with changed blob SHAs
$baselineFiles = <parsed from memory file>
$changedSinceReview = 0

foreach ($file in $prFiles) {
    $currentSha = (git ls-tree HEAD -- $file) -replace '.*\s(\w{40})\s.*','$1'
    $baselineSha = $baselineFiles[$file]  # from memory
    if ($currentSha -ne $baselineSha) { $changedSinceReview++ }
}

if ($changedSinceReview -gt 10) {
    # FALLBACK: Reset to full review mode
    Write-Host "Staleness threshold exceeded ($changedSinceReview files changed). Falling back to full review."
}
```

**If staleness threshold is exceeded:** Run the full review checklist (Steps 1–10 from the main agent), reset the baseline, and mark all prior review points as `SUPERSEDED`.

## Additive Review Workflow

### Phase 1: Load Prior State

1. Read the memory file: `.github/agents/memory/code-reviewer-<issue>.md`
2. Parse the `## Baseline` section to get prior file SHAs
3. Parse the `## Review Points` section to get all open/addressed points
4. Read own prior GitHub PR review comments via GitHub API tools:
   - Fetch all review comments on the PR
   - Filter to comments authored by the agent's GitHub identity
   - Cross-reference with Review Point IDs from the memory file

### Phase 2: Compute Delta

1. Get all files in the current PR: `git diff --name-only origin/master...HEAD`
2. For each file, compute current blob SHA: `git ls-tree HEAD -- <file>`
3. Compare against baseline SHAs
4. Build three lists:
   - **Changed files**: blob SHA differs from baseline (these get the new-issue scan)
   - **New files**: in current PR but not in baseline (added since last review)
   - **Unchanged files**: blob SHA matches baseline (skip unless a prior RP references them)

### Phase 3: Resolve Prior Review Points

For each review point with status `OPEN`:

1. **Check if the file was modified** since baseline (it's in the delta)
2. **If modified:** Read the current file content around the RP's cited line. Determine if the issue was fixed:
   - Look for the guard clause / validation / pattern that the RP's "Fix Suggestion" described
   - If found → mark `ADDRESSED`, reply to the GitHub comment: `✅ Addressed in current revision — <brief description of fix>`
   - If not found but code changed significantly → mark `NEEDS_VERIFICATION`, ask the user
3. **If NOT modified:** The RP remains `OPEN` — reply to the GitHub comment: `⏳ Still open — file unchanged since last review`
4. **If the file was deleted:** Mark `SUPERSEDED` — the fix may have been structural

Status transitions:
```
OPEN → ADDRESSED      (fix confirmed in code)
OPEN → OPEN           (file unchanged or fix not found)
OPEN → WONTFIX        (user explicitly declined via GitHub thread resolution)
OPEN → SUPERSEDED     (file deleted or staleness fallback)
OPEN → NEEDS_VERIFICATION (code changed but fix is ambiguous)
```

**Honoring human signals:** If a GitHub review thread was manually resolved by a human (thread marked as "resolved"), treat the corresponding RP as `ADDRESSED` even if the code change isn't exactly what was suggested — the human made a judgment call.

### Phase 4: Scan Delta for New Issues

Run the **full architecture checklist** (from the main PR Reviewer agent Steps 2–7) but scoped to only:
- Files in the **Changed** list
- Files in the **New** list

This includes:
- Hexagonal purity checks (domain isolation, application layer imports)
- Domain invariant enforcement (for any invariant touched by changed files)
- API contract compliance (for any endpoint in changed controller files)
- Persistence conventions (for any changed EF config or migration)
- Coding conventions (naming, patterns, DI registration)
- Test coverage (for any new or modified domain/handler code)

Each new finding becomes a new Review Point with the next available ID (e.g., if last round ended at RP-7, new points start at RP-8).

### Phase 5: Build + Test

Always run build and tests regardless of review mode:

```powershell
dotnet build
dotnet test
```

### Phase 6: Post GitHub Review

The output is structured as **threaded replies + new comments**:

1. **For each resolved prior RP:** Reply to the GitHub inline comment with the resolution status
2. **For each still-open prior RP:** Reply to the GitHub inline comment noting it's still unresolved
3. **For each new finding:** Post a new inline review comment on the relevant file/line
4. **Summary comment:** Post a top-level review comment with the delta report:

```markdown
## Review Round <N> — <VERDICT>

### Prior Findings
| ID | Prior Status | New Status | Notes |
|---|---|---|---|
| RP-1 | OPEN | ✅ ADDRESSED | Guard clause added at L38 |
| RP-2 | OPEN | ⏳ STILL OPEN | File unchanged |

### New Findings
| ID | Severity | File | Description |
|---|---|---|---|
| RP-8 | ⚠️ WARNING | `Application/Commands/X.cs` | Missing null check |

### Delta Stats
- Files changed since last review: <N>
- Prior points resolved: <N> of <M>
- New issues found: <N>
- Mode: ADDITIVE / FULL (staleness fallback)
```

### Phase 7: Update Memory File

Write/overwrite the memory file with the complete current state:
- Increment the review round
- Update the baseline with current file blob SHAs
- Update all review point statuses
- Record the GitHub comment IDs for new points
- Update all other sections (architecture compliance, invariants, etc.)

## Baseline Capture

Run this at the end of every review (both FULL and ADDITIVE) to record the reviewed file states:

```powershell
# For each file in the PR diff
$files = git diff --name-only origin/master...HEAD
foreach ($file in $files) {
    $blobLine = git ls-tree HEAD -- $file
    # Parse: <mode> blob <sha>\t<path>
    $sha = ($blobLine -split '\s+')[2]
    # Record: | $file | $sha |
}
```

## Memory File Format

The memory file must follow this exact structure for additive reviews to work:

```markdown
# PR Review — Issue #<number>: <title>

## Review Metadata
- **PR:** #<pr-number>
- **Branch:** `feature/<issue-number>-<description>`
- **Review Round:** <N>
- **Review Date:** <YYYY-MM-DD>
- **Mode:** FULL / ADDITIVE / FULL (staleness fallback)
- **Verdict:** ✅ APPROVED / ⚠️ CHANGES REQUESTED / ❌ CHANGES REQUESTED

## Baseline
| File Path | Blob SHA |
|---|---|
| `src/Modules/.../Domain/X.cs` | `a1b2c3d4e5f6...` |
| `src/Modules/.../Application/Y.cs` | `f6e5d4c3b2a1...` |

## Review Points
| ID | Severity | Status | Category | File | Line | Description | Fix Suggestion | GitHub Comment ID |
|---|---|---|---|---|---|---|---|---|
| RP-1 | ❌ CRITICAL | OPEN | Invariant | `Domain/X.cs` | L42 | INV-B3 not enforced | Add guard clause | 12345678 |
| RP-2 | ⚠️ WARNING | ADDRESSED | Convention | `Application/Y.cs` | L10 | Missing validation | Add validator | 12345679 |
| RP-3 | ℹ️ INFO | WONTFIX | Style | `Infrastructure/Z.cs` | L5 | Verbose logging | Reduce log level | 12345680 |

## Review History
| Round | Date | Mode | Verdict | Points Added | Points Resolved |
|---|---|---|---|---|---|
| 1 | 2026-03-10 | FULL | ❌ CHANGES REQUESTED | 7 | 0 |
| 2 | 2026-03-12 | ADDITIVE | ⚠️ CHANGES REQUESTED | 1 | 5 |
| 3 | 2026-03-15 | ADDITIVE | ✅ APPROVED | 0 | 3 |

## Architecture Compliance
| Check | Result | Notes |
|---|---|---|
| Hexagonal purity | ✅/❌ | <details> |
| Domain layer isolation | ✅/❌ | <details> |
| Cross-context boundaries | ✅/❌ | <details> |

## Invariants Verified
| ID | Enforced In Code | Tested | Result |
|---|---|---|---|
| INV-B1 | ✅ `File.cs:L55` | ✅ `Tests.cs:L120` | ✅ |

## API Contract Compliance
| Endpoint | Method | Contract Match | Status Codes | Result |
|---|---|---|---|---|
| /api/v1/budgets | POST | ✅ | 201, 400, 409 ✅ | ✅ |

## Runtime API Validation
| Endpoint | Method | Expected | Actual | Result |
|---|---|---|---|---|
| /api/v1/budgets | POST | 201 | 201 | ✅ PASS |

## Test Coverage
| Area | Tests Found | Sufficient | Result |
|---|---|---|---|
| Domain invariants | 8 | ✅ | ✅ |

## Build Results
- **Build:** ✅ PASS / ❌ FAIL
- **Tests:** <X> passed, <Y> failed

## Task Completeness
| Acceptance Criterion | Implemented | Tested | Result |
|---|---|---|---|
| AC1: <text> | ✅ | ✅ | ✅ |

## Decisions / Clarifications
<Any questions asked/answered during this review round>
```

## Edge Cases

### Force-Push / Rebase
The diff-based approach is rebase-safe because it compares file content (blob SHAs), not commit history. After a rebase:
- Files that were rebased but not content-changed will have the same blob SHA → treated as unchanged
- Files with conflict resolutions will have different SHAs → included in the delta

### PR Closed and Reopened
If the memory file exists but the PR was closed and reopened, treat it as a continuation. The baseline is still valid.

### Multiple Reviewers
This additive system tracks only the agent's own review points. Human reviews are separate. The agent should not attempt to resolve or track human review comments — only its own.

### Review Point Renumbering
Never renumber existing review points. New points always get the next sequential ID. This ensures GitHub comment references remain stable across rounds.