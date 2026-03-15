# Frontend Patterns — SvelteKit + TypeScript

> Authoritative reference for frontend conventions. Populated during scaffold (issue #41) and updated as features are implemented.

## Status: STUB

This file will be populated with **real code patterns** once the SvelteKit scaffold is implemented (issue #41). Until then, it documents the **target conventions** from the architecture spec.

---

## Namespace / Import Conventions

- Use `$lib/` alias for all imports from `src/lib/`
- Use `$lib/types/budget` not `../../../lib/types/budget`
- Component imports: `import ExpenseList from '$lib/components/ExpenseList.svelte'`

## Route Template

```svelte
<!-- src/routes/<feature>/+page.svelte -->
<script lang="ts">
  import { onMount } from 'svelte';
  import { data, loading, error } from '$lib/stores/<feature>Store';
  import { fetchData } from '$lib/api/<feature>Api';

  onMount(async () => {
    await fetchData();
  });
</script>

{#if $loading}
  <p>Loading...</p>
{:else if $error}
  <p class="error">{$error}</p>
{:else if !$data}
  <p>No data found.</p>
{:else}
  <!-- content -->
{/if}
```

## Component Template

```svelte
<!-- src/lib/components/<Name>.svelte -->
<script lang="ts">
  import type { SomeType } from '$lib/types/<module>';

  export let items: SomeType[];
  export let onAction: ((id: string) => void) | undefined = undefined;
</script>

<!-- markup -->
```

## Store Template

```typescript
// src/lib/stores/<feature>Store.ts
import { writable } from 'svelte/store';
import type { Feature } from '$lib/types/<feature>';

export const data = writable<Feature | null>(null);
export const loading = writable(false);
export const error = writable<string | null>(null);

export async function fetchFeature(id: string): Promise<void> {
  loading.set(true);
  error.set(null);
  try {
    const result = await featureApi.get(id);
    data.set(result);
  } catch (e) {
    error.set(e instanceof Error ? e.message : 'Unknown error');
  } finally {
    loading.set(false);
  }
}
```

## API Client Template

```typescript
// src/lib/api/<feature>Api.ts
import { getAuthHeader } from '$lib/stores/authStore';
import type { Feature, CreateFeatureRequest } from '$lib/types/<feature>';

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? '';

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error ?? `HTTP ${res.status}`);
  }
  return res.json();
}

export async function getFeature(id: string): Promise<Feature> {
  const res = await fetch(`${API_BASE}/api/v1/features/${id}`, {
    headers: { ...getAuthHeader() }
  });
  return handleResponse<Feature>(res);
}

export async function createFeature(req: CreateFeatureRequest): Promise<Feature> {
  const res = await fetch(`${API_BASE}/api/v1/features`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(req)
  });
  return handleResponse<Feature>(res);
}
```

## TypeScript Type Template

```typescript
// src/lib/types/<feature>.ts
// Must exactly mirror API response shapes from docs/arch/api-contracts.md

export interface Budget {
  budgetId: string;
  householdId: string;
  yearMonth: string;
  status: BudgetStatus;
  incomeSources: IncomeSource[];
  expenses: Expense[];
}

export type BudgetStatus = 'DRAFT' | 'ACTIVE' | 'CLOSED';
```

## Test Template

```typescript
// src/lib/components/<Name>.test.ts
import { render, screen } from '@testing-library/svelte';
import { describe, it, expect } from 'vitest';
import Component from './<Name>.svelte';

describe('<Name>', () => {
  it('renders with provided data', () => {
    render(Component, { props: { items: mockData } });
    expect(screen.getByText('Expected text')).toBeInTheDocument();
  });

  it('shows empty state when no data', () => {
    render(Component, { props: { items: [] } });
    expect(screen.getByText('No items found')).toBeInTheDocument();
  });
});
```

---

> **Note:** Update this file with real patterns extracted from the codebase after each feature implementation. The stub templates above are starting points — real patterns may evolve.
