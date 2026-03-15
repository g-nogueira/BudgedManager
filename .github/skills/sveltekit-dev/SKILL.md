```skill
---
name: sveltekit-dev
description: "Build, test, lint, and validate the MonthlyBudget SvelteKit frontend. Use when running frontend commands, checking type safety, or validating frontend architecture patterns."
---

# SvelteKit Development Skill

This skill provides the commands and validation rules for building, testing, and maintaining the MonthlyBudget SvelteKit frontend.

## Project Structure

```
frontend/
  src/
    routes/                          # SvelteKit file-based routing
      +page.svelte                   # Dashboard (US-8.1)
      +layout.svelte                 # Root layout with nav
      budget/                        # Budget pages
      forecast/                      # Forecast pages
      auth/                          # Login, register
      household/                     # Household management
    lib/
      api/                           # REST client wrappers
        budgetApi.ts
        forecastApi.ts
        authApi.ts
      components/                    # Reusable Svelte components
        ForecastChart.svelte
        ForecastOverlay.svelte
        ExpenseList.svelte
        BalanceSummary.svelte
      stores/                        # Svelte writable stores
        budgetStore.ts
        forecastStore.ts
        authStore.ts
      types/                         # TypeScript interfaces mirroring API contracts
        budget.ts
        forecast.ts
        auth.ts
  static/
  svelte.config.js
  package.json
  tsconfig.json
  vite.config.ts
```

## Commands

All commands run from the `frontend/` directory.

```powershell
# Install dependencies
cd frontend; pnpm install

# Start dev server with HMR
cd frontend; pnpm dev

# Production build (includes type checking)
cd frontend; pnpm build

# Type checking only (Svelte + TypeScript)
cd frontend; pnpm check

# Lint (ESLint + Prettier)
cd frontend; pnpm lint

# Format code
cd frontend; pnpm format

# Run unit/component tests (Vitest)
cd frontend; pnpm test

# Run tests in watch mode
cd frontend; pnpm test -- --watch
```

## Verification Sequence

Run before every commit:

```powershell
cd frontend
pnpm check   # TypeScript + Svelte type checking
pnpm lint     # ESLint + Prettier
pnpm test     # Vitest unit/component tests
```

All three must pass.

## Frontend Architecture Rules

### 1. TypeScript Strict Mode

- No `any` types — use proper interfaces from `lib/types/`
- All component props must be typed
- All API client return types must be explicit
- Enable `strict: true` in `tsconfig.json`

### 2. API Contract Fidelity

TypeScript interfaces in `lib/types/` must exactly mirror API response shapes from `docs/arch/api-contracts.md`:
- Property names match JSON keys (camelCase)
- Types: `string` for UUIDs, `number` for decimals, string literals for enums
- No extra or missing fields

**Always verify contracts against `docs/arch/api-contracts.md`** — never rely on memory for expected request/response schemas.

### 3. API Client Layer

All API calls go through `lib/api/*.ts` clients:
- Never use raw `fetch` in components or routes
- Every client function injects auth header from `authStore`
- Every client function handles error responses
- API base URL from environment config — never hardcoded

```typescript
// Pattern: lib/api/budgetApi.ts
import { getAuthHeader } from '$lib/stores/authStore';

const API_BASE = import.meta.env.VITE_API_BASE_URL;

export async function getBudget(budgetId: string): Promise<Budget> {
  const res = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}`, {
    headers: { ...getAuthHeader() }
  });
  if (!res.ok) throw await mapApiError(res);
  return res.json();
}
```

### 4. Store Patterns

Stores use Svelte writable/readable stores with loading and error state:

```typescript
// Pattern: lib/stores/budgetStore.ts
import { writable } from 'svelte/store';
import type { Budget } from '$lib/types/budget';

export const budget = writable<Budget | null>(null);
export const budgetLoading = writable(false);
export const budgetError = writable<string | null>(null);
```

### 5. Component Patterns

Components handle three states: loading, error, empty:

```svelte
<!-- Pattern: every page/component -->
{#if $loading}
  <LoadingSkeleton />
{:else if $error}
  <ErrorMessage message={$error} />
{:else if !data || data.length === 0}
  <EmptyState message="No items found" />
{:else}
  <!-- actual content -->
{/if}
```

### 6. Auth & Route Guards

- JWT stored in `authStore` (backed by `localStorage`)
- Protected routes check auth state in `+page.ts` load function
- 401 responses trigger redirect to `/auth/login`
- Auth store provides `getAuthHeader()` helper for API clients

### 7. Chart.js Integration

- Chart.js is always wrapped in Svelte components (`ForecastChart.svelte`, `ForecastOverlay.svelte`)
- Never import Chart.js directly in routes
- Charts receive typed props, not raw API responses
- Cleanup chart instances in `onDestroy`

### 8. Environment Configuration

```
VITE_API_BASE_URL=http://localhost:5000   # Dev proxy target
```

In dev, use Vite proxy to avoid CORS:
```typescript
// vite.config.ts
export default defineConfig({
  server: {
    proxy: {
      '/api': 'http://localhost:5000'
    }
  }
});
```

## Validation Checklist

Run these checks to validate frontend architecture compliance:

```powershell
# Check for `any` types
cd frontend; Select-String -Recurse -Pattern ": any\b|as any\b|<any>" -Include "*.ts","*.svelte" -Path "src/" | Format-Table Path, LineNumber, Line

# Check for raw fetch in components/routes (should only be in lib/api/)
cd frontend; Select-String -Recurse -Pattern "\bfetch\(" -Include "*.svelte" -Path "src/routes/" | Format-Table Path, LineNumber, Line

# Check for hardcoded API URLs
cd frontend; Select-String -Recurse -Pattern "localhost|127\.0\.0\.1" -Include "*.ts","*.svelte" -Path "src/" | Format-Table Path, LineNumber, Line

# Check for missing loading/error states in pages
cd frontend; Get-ChildItem -Recurse -Filter "+page.svelte" -Path "src/routes/" | ForEach-Object { $content = Get-Content $_.FullName -Raw; if ($content -notmatch '#if.*loading|#if.*error') { Write-Host "MISSING states: $($_.FullName)" -ForegroundColor Red } }
```

## Test Conventions

- **Framework:** Vitest + `@testing-library/svelte`
- **Test location:** Co-located with source or in `__tests__/` folders
- **Naming:** `<component>.test.ts` or `<module>.test.ts`
- **Test naming:** Descriptive strings: `'renders expenses grouped by category'`
- **No mocking of API clients in component tests** — use MSW (Mock Service Worker) or direct store manipulation
```
