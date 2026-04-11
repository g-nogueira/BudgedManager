---
name: 'Frontend SvelteKit Conventions'
description: 'SvelteKit/TypeScript specific grounding rules and patterns for the MonthlyBudget frontend'
applyTo: 'frontend/**/*.ts,frontend/**/*.svelte,frontend/**/*.js'
---

# Frontend SvelteKit Conventions

## Grounding Rules (Frontend-Specific)

Before writing any SvelteKit/TypeScript code:

1. **Before writing ANY component:** Read an existing `.svelte` file to match patterns
2. **Before writing ANY import statement:** Verify the module exists by searching
3. **Before referencing ANY type:** Grep the codebase for its exact declaration
4. **When writing test names:** Grep existing tests to match naming convention

## TypeScript Rules

- **Strict mode** — no `any` types
- **API type fidelity** — TypeScript interfaces must exactly mirror `docs/arch/api-contracts.md` response shapes
- **API client layer** — all API calls go through `lib/api/*.ts` clients — never raw `fetch` in components or routes
- **Auth isolation** — JWT token management in `authStore.ts` only
- **Chart.js wrapping** — Chart.js wrapped in Svelte components — never used directly in routes
- **Environment config** — API URL from environment variable — never hardcoded

## State Handling

Every page must handle all three states:
1. **Loading skeleton** — shown while data is being fetched
2. **Error state** — shown when the request fails
3. **Empty state** — shown when data is loaded but the collection is empty

## Build & Lint Commands

```powershell
cd frontend
pnpm check     # Type checking
pnpm lint       # ESLint
pnpm test       # Run tests
pnpm dev        # Dev server
```

## Never Load Backend Files

Frontend agents should NEVER load:
- `domain-invariants.md`
- `persistence-conventions.md`
- `shared-patterns.md`
- `budget-patterns.md`
- `forecast-patterns.md`
- `identity-patterns.md`
