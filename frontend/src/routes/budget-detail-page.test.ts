import { cleanup, render, screen, waitFor } from '@testing-library/svelte';
import type { Writable } from 'svelte/store';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { Budget, Expense, IncomeSource } from '$lib/types/budget';

const { mockPage } = vi.hoisted(() => ({
  mockPage: {
    params: {
      budgetId: 'budget-1'
    }
  }
}));

vi.mock('$app/state', () => ({
  page: mockPage
}));

const buildIncome = (overrides?: Partial<IncomeSource>): IncomeSource => ({
  incomeId: 'income-1',
  name: 'Salary',
  amount: 4200,
  ...overrides
});

const buildExpense = (overrides?: Partial<Expense>): Expense => ({
  expenseId: 'expense-1',
  name: 'Rent',
  category: 'FIXED',
  dayOfMonth: 1,
  isSpread: false,
  amount: 1200,
  isExcluded: false,
  ...overrides
});

const buildBudget = (overrides?: Partial<Budget>): Budget => ({
  budgetId: 'budget-1',
  householdId: 'household-1',
  yearMonth: '2026-04',
  status: 'DRAFT',
  incomeSources: [buildIncome()],
  expenses: [buildExpense()],
  totalIncome: 4200,
  createdAt: '2026-04-01T00:00:00Z',
  updatedAt: '2026-04-01T00:00:00Z',
  ...overrides
});

vi.mock('$lib/stores/budgetStore', async () => {
  const { writable } = await import('svelte/store');

  return {
    budget: writable<Budget | null>(null),
    budgetLoading: writable(false),
    budgetError: writable<string | null>(null),
    totalExpenses: writable(0),
    netBalance: writable(0),
    fetchBudgetById: vi.fn(),
    storeActivateBudget: vi.fn(),
    storeAddIncome: vi.fn(),
    storeUpdateIncome: vi.fn(),
    storeRemoveIncome: vi.fn(),
    storeAddExpense: vi.fn(),
    storeUpdateExpense: vi.fn(),
    storeRemoveExpense: vi.fn(),
    storeToggleExclusion: vi.fn()
  };
});

import * as budgetStore from '$lib/stores/budgetStore';

import BudgetDetailPage from './budget/[budgetId]/+page.svelte';

describe('budget detail page', () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.clearAllMocks();
    mockPage.params.budgetId = 'budget-1';
    window.history.pushState({}, '', '/budget/budget-1');

    (budgetStore.budgetLoading as Writable<boolean>).set(false);
    (budgetStore.budgetError as Writable<string | null>).set(null);
    (budgetStore.budget as Writable<Budget | null>).set(buildBudget());
    (budgetStore.totalExpenses as Writable<number>).set(1200);
    (budgetStore.netBalance as Writable<number>).set(3000);

    vi.mocked(budgetStore.fetchBudgetById).mockResolvedValue(undefined);
  });

  it('fetches budget on mount and renders sections', async () => {
    render(BudgetDetailPage);

    await waitFor(() => {
      expect(budgetStore.fetchBudgetById).toHaveBeenCalledWith('budget-1');
    });

    expect(screen.getByTestId('income-section')).toBeInTheDocument();
    expect(screen.getByTestId('expense-list')).toBeInTheDocument();
  });

  it('does not refetch when the budget store updates after initial load', async () => {
    render(BudgetDetailPage);

    await waitFor(() => {
      expect(budgetStore.fetchBudgetById).toHaveBeenCalledWith('budget-1');
    });

    // Simulate the store being updated (as would happen when fetchBudgetById resolves).
    // The $effect must NOT re-trigger a second fetch.
    (budgetStore.budget as Writable<Budget | null>).set(
      buildBudget({ totalIncome: 9999 })
    );

    await waitFor(() => {
      expect(budgetStore.fetchBudgetById).toHaveBeenCalledTimes(1);
    });
  });

  it('displays formatted month heading and status badge', () => {
    render(BudgetDetailPage);

    expect(screen.getByTestId('budget-detail-heading')).toHaveTextContent('April 2026 Budget');
    expect(screen.getByTestId('budget-status-badge')).toHaveTextContent('DRAFT');
  });

  it('shows activate button only for DRAFT budgets', () => {
    render(BudgetDetailPage);

    expect(screen.getByTestId('activate-budget-button')).toBeInTheDocument();
  });

  it('disables activate button when draft has no incomes', () => {
    (budgetStore.budget as Writable<Budget | null>).set(
      buildBudget({
        incomeSources: [],
        totalIncome: 0
      })
    );

    render(BudgetDetailPage);

    expect(screen.getByTestId('activate-budget-button')).toBeDisabled();
  });

  it('hides activate button for ACTIVE budgets', () => {
    (budgetStore.budget as Writable<Budget | null>).set(buildBudget({ status: 'ACTIVE' }));

    render(BudgetDetailPage);

    expect(screen.queryByTestId('activate-budget-button')).not.toBeInTheDocument();
    expect(screen.getByTestId('budget-status-badge')).toHaveTextContent('ACTIVE');
  });

  it('shows loading state while fetching', () => {
    (budgetStore.budgetLoading as Writable<boolean>).set(true);
    (budgetStore.budget as Writable<Budget | null>).set(null);

    render(BudgetDetailPage);

    expect(screen.getByTestId('budget-detail-loading')).toBeInTheDocument();
  });

  it('keeps detail content visible during mutation loading when budget exists', () => {
    (budgetStore.budgetLoading as Writable<boolean>).set(true);

    render(BudgetDetailPage);

    expect(screen.getByTestId('budget-detail-page')).toBeInTheDocument();
    expect(screen.queryByTestId('budget-detail-loading')).not.toBeInTheDocument();
  });

  it('shows error state when fetch fails', () => {
    (budgetStore.budgetError as Writable<string | null>).set('Unable to load budget');
    (budgetStore.budget as Writable<Budget | null>).set(null);

    render(BudgetDetailPage);

    return waitFor(() => {
      expect(screen.getByTestId('budget-detail-error')).toHaveTextContent('Unable to load budget');
    });
  });

  it('shows inline error when mutation fails but budget is loaded', () => {
    (budgetStore.budgetError as Writable<string | null>).set('Unable to update expense');

    render(BudgetDetailPage);

    expect(screen.getByTestId('budget-detail-inline-error')).toHaveTextContent(
      'Unable to update expense'
    );
  });

  it('shows summary totals and forecast link', () => {
    (budgetStore.totalExpenses as Writable<number>).set(1250.5);
    (budgetStore.netBalance as Writable<number>).set(2949.5);

    render(BudgetDetailPage);

    expect(screen.getByTestId('summary-total-income')).toHaveTextContent('€4,200.00');
    expect(screen.getByTestId('summary-total-expenses')).toHaveTextContent('€1,250.50');
    expect(screen.getByTestId('summary-net-balance')).toHaveTextContent('€2,949.50');

    const forecastLink = screen.getByTestId('generate-forecast-link');
    expect(forecastLink).toHaveAttribute('href', '/forecast');
  });
});
