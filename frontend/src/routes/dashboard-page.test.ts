import { cleanup, render, screen, waitFor } from '@testing-library/svelte';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { Budget } from '$lib/types/budget';
import type { DailyEntry, Forecast, ForecastSummary } from '$lib/types/forecast';

vi.mock('$lib/api/budgetApi', () => ({
  getBudgetById: vi.fn(),
  getBudgetByMonth: vi.fn()
}));

vi.mock('$lib/api/forecastApi', () => ({
  getForecastById: vi.fn(),
  getForecastsByBudget: vi.fn()
}));

import * as budgetApi from '$lib/api/budgetApi';
import * as forecastApi from '$lib/api/forecastApi';
import { budget, budgetError, budgetLoading } from '$lib/stores/budgetStore';
import { forecast, forecastError, forecastLoading, forecasts } from '$lib/stores/forecastStore';
import DashboardPage from './+page.svelte';

const buildBudget = (): Budget => ({
  budgetId: 'budget-1',
  householdId: 'household-1',
  yearMonth: '2026-04',
  status: 'ACTIVE',
  incomeSources: [],
  expenses: [],
  totalIncome: 1500,
  createdAt: '2026-04-01T00:00:00Z',
  updatedAt: '2026-04-01T00:00:00Z'
});

const buildDailyEntries = (): DailyEntry[] => {
  const today = new Date().getDate();

  return [
    {
      dayNumber: Math.max(1, today - 1),
      remainingBalance: 950,
      dailyExpenseTotal: 20,
      breakdown: []
    },
    {
      dayNumber: today,
      remainingBalance: 900,
      dailyExpenseTotal: 50,
      breakdown: []
    }
  ];
};

const buildForecastSummary = (): ForecastSummary => ({
  forecastId: 'forecast-1',
  versionLabel: 'v1',
  forecastType: 'ORIGINAL',
  endOfMonthBalance: 900,
  isSnapshot: false,
  createdAt: '2026-04-11T10:00:00Z'
});

const buildForecast = (overrides?: Partial<Forecast>): Forecast => ({
  forecastId: 'forecast-1',
  budgetId: 'budget-1',
  versionLabel: 'v1',
  forecastType: 'ORIGINAL',
  startDay: 0,
  startBalance: 1500,
  endOfMonthBalance: 900,
  isSnapshot: false,
  dailyEntries: buildDailyEntries(),
  ...overrides
});

const resetStores = (): void => {
  budget.set(null);
  budgetLoading.set(false);
  budgetError.set(null);

  forecast.set(null);
  forecasts.set([]);
  forecastLoading.set(false);
  forecastError.set(null);
};

const configureSuccessfulLoad = (staleValue?: boolean): void => {
  const budgetValue = buildBudget();
  const forecastSummary = buildForecastSummary();
  const forecastValue = buildForecast(
    staleValue === undefined
      ? undefined
      : {
          isStale: staleValue
        }
  );

  vi.mocked(budgetApi.getBudgetByMonth).mockResolvedValue(budgetValue);
  vi.mocked(forecastApi.getForecastsByBudget).mockResolvedValue([forecastSummary]);
  vi.mocked(forecastApi.getForecastById).mockResolvedValue(forecastValue);
};

describe('Dashboard route', () => {
  beforeEach(() => {
    resetStores();
    vi.resetAllMocks();
  });

  afterEach(() => {
    cleanup();
  });

  it('renders loading state while fetching budget', () => {
    const pendingBudget = new Promise<Budget>(() => {
      return;
    });
    vi.mocked(budgetApi.getBudgetByMonth).mockReturnValue(pendingBudget);

    render(DashboardPage);

    return waitFor(() => {
      expect(screen.getByTestId('budget-loading')).toBeInTheDocument();
    });
  });

  it('renders "No budget for this month" when budget is null', async () => {
    vi.mocked(budgetApi.getBudgetByMonth).mockResolvedValue(null as unknown as Budget);

    render(DashboardPage);

    await waitFor(() => {
      expect(screen.getByTestId('no-budget')).toBeInTheDocument();
    });
  });

  it('renders "No forecast generated yet" when budget exists but no forecasts', async () => {
    vi.mocked(budgetApi.getBudgetByMonth).mockResolvedValue(buildBudget());
    vi.mocked(forecastApi.getForecastsByBudget).mockResolvedValue([]);

    render(DashboardPage);

    await waitFor(() => {
      expect(screen.getByTestId('no-forecast')).toBeInTheDocument();
    });
  });

  it('renders BalanceSummary with correct data when budget+forecast exist', async () => {
    configureSuccessfulLoad(false);

    render(DashboardPage);

    await waitFor(() => {
      expect(screen.getByTestId('end-of-month-balance').textContent).toContain('900.00');
    });
  });

  it('renders ForecastChart with dailyEntries', async () => {
    configureSuccessfulLoad(false);

    render(DashboardPage);

    await waitFor(() => {
      expect(screen.getByTestId('forecast-chart')).toBeInTheDocument();
    });
  });

  it('renders stale banner when forecast.isStale is true', async () => {
    configureSuccessfulLoad(true);

    render(DashboardPage);

    await waitFor(() => {
      expect(screen.getByTestId('stale-banner')).toBeInTheDocument();
    });
  });

  it('does not render stale banner when forecast.isStale is false/undefined', async () => {
    configureSuccessfulLoad();

    render(DashboardPage);

    await waitFor(() => {
      expect(screen.getByTestId('dashboard')).toBeInTheDocument();
    });

    expect(screen.queryByTestId('stale-banner')).not.toBeInTheDocument();
  });
});
