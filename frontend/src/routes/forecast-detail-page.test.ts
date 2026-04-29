import { cleanup, render, screen, waitFor } from '@testing-library/svelte';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { Forecast } from '$lib/types/forecast';

const { mockPage } = vi.hoisted(() => ({
  mockPage: {
    params: { forecastId: 'forecast-1' },
    url: new URL('http://localhost/forecast/forecast-1?budgetId=budget-1')
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
    fetchAllForecasts: vi.fn()
  };
});

import type { Writable } from 'svelte/store';
import * as forecastStore from '$lib/stores/forecastStore';
import ForecastDetailPage from './forecast/[forecastId]/+page.svelte';

const buildForecast = (overrides?: Partial<Forecast>): Forecast => ({
  forecastId: 'forecast-1',
  budgetId: 'budget-1',
  versionLabel: 'Original',
  forecastType: 'ORIGINAL',
  startDay: 0,
  startBalance: 2000,
  endOfMonthBalance: 1500,
  isSnapshot: false,
  dailyEntries: [
    { dayNumber: 1, remainingBalance: 1900, dailyExpenseTotal: 100, breakdown: [] },
    { dayNumber: 2, remainingBalance: 1800, dailyExpenseTotal: 100, breakdown: [] }
  ],
  ...overrides
});

const resetStores = () => {
  (forecastStore.allForecasts as Writable<Forecast[]>).set([]);
  (forecastStore.forecastLoading as Writable<boolean>).set(false);
  (forecastStore.forecastError as Writable<string | null>).set(null);
};

describe('forecast detail page', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  beforeEach(() => {
    resetStores();
    vi.mocked(forecastStore.fetchAllForecasts).mockResolvedValue(undefined);
    mockPage.params.forecastId = 'forecast-1';
    mockPage.url = new URL('http://localhost/forecast/forecast-1?budgetId=budget-1');
  });

  it('shows loading state', () => {
    (forecastStore.forecastLoading as Writable<boolean>).set(true);

    render(ForecastDetailPage);

    expect(screen.getByTestId('loading-state')).toBeInTheDocument();
  });

  it('shows error state', () => {
    (forecastStore.forecastError as Writable<string | null>).set('Network failure');

    render(ForecastDetailPage);

    expect(screen.getByTestId('error-state')).toBeInTheDocument();
    expect(screen.getByText('Network failure')).toBeInTheDocument();
  });

  it('shows empty state when no forecasts found', async () => {
    (forecastStore.allForecasts as Writable<Forecast[]>).set([]);

    render(ForecastDetailPage);

    await waitFor(() => {
      expect(screen.getByTestId('empty-state')).toBeInTheDocument();
    });
  });

  it('shows missing-budgetId error when budgetId query param is absent', () => {
    mockPage.url = new URL('http://localhost/forecast/forecast-1');

    render(ForecastDetailPage);

    expect(screen.getByTestId('missing-budget-error')).toBeInTheDocument();
    expect(vi.mocked(forecastStore.fetchAllForecasts)).not.toHaveBeenCalled();
  });

  it('renders ForecastChart with single forecast', async () => {
    (forecastStore.allForecasts as Writable<Forecast[]>).set([buildForecast()]);

    render(ForecastDetailPage);

    await waitFor(() => {
      expect(screen.getByTestId('forecast-detail-page')).toBeInTheDocument();
      expect(screen.getByTestId('forecast-chart')).toBeInTheDocument();
      expect(screen.queryByTestId('forecast-overlay')).toBeNull();
    });
  });

  it('renders ForecastOverlay with multiple forecasts', async () => {
    const forecasts: Forecast[] = [
      buildForecast({ forecastId: 'f-1', versionLabel: 'Original' }),
      buildForecast({
        forecastId: 'f-2',
        versionLabel: 'Re-forecast Apr 10',
        forecastType: 'REFORECAST',
        startDay: 10
      })
    ];
    mockPage.params.forecastId = 'f-2';
    (forecastStore.allForecasts as Writable<Forecast[]>).set(forecasts);

    render(ForecastDetailPage);

    await waitFor(() => {
      expect(screen.getByTestId('forecast-detail-page')).toBeInTheDocument();
      expect(screen.getByTestId('forecast-overlay')).toBeInTheDocument();
      expect(screen.queryByTestId('forecast-chart')).toBeNull();
      expect(screen.getByText('Re-forecast Apr 10')).toBeInTheDocument();
    });
  });
});
