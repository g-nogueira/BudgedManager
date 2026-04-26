import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/svelte';
import type { Writable } from 'svelte/store';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('$app/navigation', () => ({
  goto: vi.fn()
}));

vi.mock('$lib/stores/budgetStore', async () => {
  const { writable } = await import('svelte/store');

  return {
    budgetLoading: writable(false),
    budgetError: writable<string | null>(null),
    storeCreateBudget: vi.fn()
  };
});

import * as navigation from '$app/navigation';
import * as budgetStore from '$lib/stores/budgetStore';

import BudgetCreatePage from './budget/+page.svelte';

describe('budget create page', () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.clearAllMocks();

    (budgetStore.budgetLoading as Writable<boolean>).set(false);
    (budgetStore.budgetError as Writable<string | null>).set(null);
  });

  it('renders month picker with heading', () => {
    render(BudgetCreatePage);

    expect(screen.getByRole('heading', { name: 'Start a New Monthly Budget' })).toBeInTheDocument();
    expect(screen.getByTestId('month-input')).toBeInTheDocument();
  });

  it('disables submit when no month selected', async () => {
    render(BudgetCreatePage);

    const monthInput = screen.getByTestId('month-input') as HTMLInputElement;
    await fireEvent.input(monthInput, { target: { value: '' } });

    expect(screen.getByTestId('create-budget-button')).toBeDisabled();
  });

  it('submits and navigates to budget detail on success', async () => {
    vi.mocked(budgetStore.storeCreateBudget).mockResolvedValue({
      budgetId: 'budget-42',
      status: 'DRAFT'
    });
    render(BudgetCreatePage);

    const monthInput = screen.getByTestId('month-input') as HTMLInputElement;
    await fireEvent.input(monthInput, { target: { value: '2026-07' } });
    await fireEvent.click(screen.getByTestId('create-budget-button'));

    await waitFor(() => {
      expect(budgetStore.storeCreateBudget).toHaveBeenCalledWith('2026-07');
      expect(navigation.goto).toHaveBeenCalledWith('/budget/budget-42');
    });
  });

  it('shows error message on API failure', () => {
    (budgetStore.budgetError as Writable<string | null>).set('Unable to create budget');
    render(BudgetCreatePage);

    expect(screen.getByTestId('budget-create-error')).toHaveTextContent('Unable to create budget');
  });

  it('disables submit while loading', () => {
    (budgetStore.budgetLoading as Writable<boolean>).set(true);
    render(BudgetCreatePage);

    expect(screen.getByTestId('create-budget-button')).toBeDisabled();
    expect(screen.getByTestId('create-budget-button')).toHaveTextContent('Creating draft...');
  });
});
