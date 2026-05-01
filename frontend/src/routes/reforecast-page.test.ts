import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/svelte';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { Forecast, ReforecastResult } from '$lib/types/forecast';
import type { Budget } from '$lib/types/budget';
import type { Writable } from 'svelte/store';

// Hoisted mock for $app/state
const { mockPage } = vi.hoisted(() => ({
  mockPage: {
    params: {},
    url: new URL('http://localhost/forecast/reforecast?budgetId=b-1&forecastId=f-1')
  }
}));

vi.mock('$app/state', () => ({
  page: mockPage
}));

vi.mock('$lib/stores/forecastStore', async () => {
  const { writable } = await import('svelte/store');
  return {
    allForecasts: writable<Forecast[]>([]),
    forecastLoading: writable(false),
    forecastError: writable<string | null>(null),
    reforecastResult: writable<ReforecastResult | null>(null),
    fetchAllForecasts: vi.fn(),
    submitReforecast: vi.fn()
  };
});

vi.mock('$lib/stores/budgetStore', async () => {
  const { writable } = await import('svelte/store');
  return {
    budget: writable<Budget | null>(null),
    budgetLoading: writable(false),
    budgetError: writable<string | null>(null),
    fetchBudgetById: vi.fn()
  };
});

// Must import after mocks
import * as forecastStore from '$lib/stores/forecastStore';
import * as budgetStore from '$lib/stores/budgetStore';
import ReforecastPage from './forecast/reforecast/+page.svelte';

const buildForecast = (overrides?: Partial<Forecast>): Forecast => ({
  forecastId: 'f-1',
  budgetId: 'b-1',
  versionLabel: 'Original',
  forecastType: 'ORIGINAL',
  startDay: 0,
  startBalance: 5000,
  endOfMonthBalance: 3200,
  isSnapshot: false,
  dailyEntries: [],
  ...overrides
});

const buildBudget = (overrides?: Partial<Budget>): Budget => ({
  budgetId: 'b-1',
  householdId: 'hh-1',
  yearMonth: '2026-05',
  status: 'ACTIVE',
  incomeSources: [],
  expenses: [
    {
      expenseId: 'e-1',
      name: 'Rent',
      category: 'FIXED',
      dayOfMonth: 1,
      isSpread: false,
      amount: 1200,
      isExcluded: false
    },
    {
      expenseId: 'e-2',
      name: 'Groceries',
      category: 'VARIABLE',
      dayOfMonth: 20,
      isSpread: false,
      amount: 400,
      isExcluded: false
    }
  ],
  totalIncome: 5000,
  createdAt: '2026-05-01T00:00:00Z',
  updatedAt: '2026-05-01T00:00:00Z',
  ...overrides
});

const resetStores = () => {
  (forecastStore.allForecasts as Writable<Forecast[]>).set([]);
  (forecastStore.forecastLoading as Writable<boolean>).set(false);
  (forecastStore.forecastError as Writable<string | null>).set(null);
  (forecastStore.reforecastResult as Writable<ReforecastResult | null>).set(null);
  (budgetStore.budget as Writable<Budget | null>).set(null);
  (budgetStore.budgetLoading as Writable<boolean>).set(false);
};

describe('reforecast page', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  beforeEach(() => {
    resetStores();
    mockPage.url = new URL('http://localhost/forecast/reforecast?budgetId=b-1&forecastId=f-1');
  });

  it('shows error when budgetId is missing', () => {
    mockPage.url = new URL('http://localhost/forecast/reforecast?forecastId=f-1');
    render(ReforecastPage);
    expect(screen.getByTestId('missing-params-error')).toBeTruthy();
  });

  it('shows error when forecastId is missing', () => {
    mockPage.url = new URL('http://localhost/forecast/reforecast?budgetId=b-1');
    render(ReforecastPage);
    expect(screen.getByTestId('missing-params-error')).toBeTruthy();
  });

  it('renders step 1 form with balance and day inputs', () => {
    render(ReforecastPage);
    expect(screen.getByTestId('step-1')).toBeTruthy();
    expect(screen.getByTestId('actual-balance-input')).toBeTruthy();
    expect(screen.getByTestId('start-day-input')).toBeTruthy();
    expect(screen.getByTestId('version-label-input')).toBeTruthy();
  });

  it('next button is disabled when balance is empty', () => {
    render(ReforecastPage);
    const nextButton = screen.getByTestId('next-button') as HTMLButtonElement;
    expect(nextButton.disabled).toBe(true);
  });

  it('shows step1 validation error for invalid balance', async () => {
    render(ReforecastPage);

    await fireEvent.input(screen.getByTestId('actual-balance-input'), {
      target: { value: '-50' }
    });
    await fireEvent.click(screen.getByTestId('next-button'));

    await waitFor(() => {
      expect(screen.getByTestId('step1-error')).toBeTruthy();
    });
  });

  it('advances to step 2 when step 1 is valid', async () => {
    vi.mocked(budgetStore.fetchBudgetById).mockResolvedValueOnce(undefined);
    (budgetStore.budget as Writable<Budget | null>).set(buildBudget());

    render(ReforecastPage);

    await fireEvent.input(screen.getByTestId('actual-balance-input'), {
      target: { value: '3100' }
    });
    await fireEvent.click(screen.getByTestId('next-button'));

    await waitFor(() => {
      expect(screen.getByTestId('step-2')).toBeTruthy();
    });
    expect(budgetStore.fetchBudgetById).toHaveBeenCalledWith('b-1');
  });

  it('step 2 shows loading while fetching budget', async () => {
    (budgetStore.budgetLoading as Writable<boolean>).set(true);
    vi.mocked(budgetStore.fetchBudgetById).mockResolvedValueOnce(undefined);

    render(ReforecastPage);

    await fireEvent.input(screen.getByTestId('actual-balance-input'), {
      target: { value: '3100' }
    });
    await fireEvent.click(screen.getByTestId('next-button'));

    await waitFor(() => expect(screen.queryByTestId('step-2')).toBeTruthy());

    expect(screen.getByTestId('budget-loading')).toBeTruthy();
  });

  it('step 2 shows future expense list', async () => {
    vi.mocked(budgetStore.fetchBudgetById).mockResolvedValueOnce(undefined);
    (budgetStore.budget as Writable<Budget | null>).set(buildBudget());

    render(ReforecastPage);

    await fireEvent.input(screen.getByTestId('actual-balance-input'), {
      target: { value: '3100' }
    });
    // set startDay to 10 so Rent(day1) is excluded but Groceries(day20) is included
    await fireEvent.input(screen.getByTestId('start-day-input'), {
      target: { value: '10' }
    });
    await fireEvent.click(screen.getByTestId('next-button'));

    await waitFor(() => expect(screen.getByTestId('step-2')).toBeTruthy());
    await waitFor(() => expect(screen.getByTestId('adjustment-list')).toBeTruthy());

    // Rent is day 1 — below startDay 10, so excluded. Groceries is day 20 — included.
    expect(screen.queryByText('Rent')).toBeNull();
    expect(screen.getByText('Groceries')).toBeTruthy();
  });

  it('generate button calls submitReforecast with correct payload', async () => {
    vi.mocked(budgetStore.fetchBudgetById).mockResolvedValueOnce(undefined);
    vi.mocked(forecastStore.submitReforecast).mockResolvedValueOnce(undefined);
    vi.mocked(forecastStore.fetchAllForecasts).mockResolvedValueOnce(undefined);
    (budgetStore.budget as Writable<Budget | null>).set(buildBudget());
    (forecastStore.reforecastResult as Writable<ReforecastResult | null>).set({
      forecastId: 'f-new',
      endOfMonthBalance: 2800
    });
    (forecastStore.allForecasts as Writable<Forecast[]>).set([buildForecast()]);

    render(ReforecastPage);

    await fireEvent.input(screen.getByTestId('actual-balance-input'), {
      target: { value: '3100' }
    });
    await fireEvent.input(screen.getByTestId('start-day-input'), {
      target: { value: '15' }
    });
    await fireEvent.click(screen.getByTestId('next-button'));

    await waitFor(() => expect(screen.getByTestId('step-2')).toBeTruthy());

    await fireEvent.click(screen.getByTestId('generate-button'));

    await waitFor(() => {
      expect(forecastStore.submitReforecast).toHaveBeenCalledWith(
        'b-1',
        'f-1',
        expect.objectContaining({
          startDay: 15,
          actualBalance: 3100,
          expenseAdjustments: undefined
        })
      );
    });
  });

  it('step 3 shows result after successful reforecast', async () => {
    (forecastStore.reforecastResult as Writable<ReforecastResult | null>).set({
      forecastId: 'f-new',
      endOfMonthBalance: 2800
    });
    (forecastStore.allForecasts as Writable<Forecast[]>).set([
      buildForecast(),
      buildForecast({ forecastId: 'f-new', versionLabel: 'Re-forecast', forecastType: 'REFORECAST', endOfMonthBalance: 2800 })
    ]);
    vi.mocked(budgetStore.fetchBudgetById).mockResolvedValueOnce(undefined);
    vi.mocked(forecastStore.submitReforecast).mockResolvedValueOnce(undefined);
    vi.mocked(forecastStore.fetchAllForecasts).mockResolvedValueOnce(undefined);
    (budgetStore.budget as Writable<Budget | null>).set(buildBudget());

    render(ReforecastPage);

    await fireEvent.input(screen.getByTestId('actual-balance-input'), {
      target: { value: '3100' }
    });
    await fireEvent.click(screen.getByTestId('next-button'));
    await waitFor(() => expect(screen.getByTestId('step-2')).toBeTruthy());

    await fireEvent.click(screen.getByTestId('generate-button'));
    await waitFor(() => expect(screen.getByTestId('step-3')).toBeTruthy());
    await waitFor(() => expect(screen.getByTestId('balance-comparison')).toBeTruthy());

    expect(screen.getByTestId('new-balance')).toHaveTextContent('€2,800.00');
    expect(screen.getByTestId('compare-link')).toBeTruthy();
    expect(screen.getByTestId('new-forecast-link')).toBeTruthy();
  });

  it('step 3 shows error if reforecast fails', async () => {
    vi.mocked(budgetStore.fetchBudgetById).mockResolvedValueOnce(undefined);
    vi.mocked(forecastStore.submitReforecast).mockResolvedValueOnce(undefined);
    (budgetStore.budget as Writable<Budget | null>).set(buildBudget());
    (forecastStore.forecastError as Writable<string | null>).set('API error');

    render(ReforecastPage);

    await fireEvent.input(screen.getByTestId('actual-balance-input'), {
      target: { value: '3100' }
    });
    await fireEvent.click(screen.getByTestId('next-button'));
    await waitFor(() => expect(screen.getByTestId('step-2')).toBeTruthy());

    await fireEvent.click(screen.getByTestId('generate-button'));
    await waitFor(() => expect(screen.getByTestId('step-3')).toBeTruthy());
    await waitFor(() => expect(screen.getByTestId('result-error')).toBeTruthy());
  });
});
