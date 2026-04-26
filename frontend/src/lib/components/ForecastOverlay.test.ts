import { cleanup, render, screen } from '@testing-library/svelte';
import { afterEach, describe, expect, it } from 'vitest';
import ForecastOverlay from '$lib/components/ForecastOverlay.svelte';
import type { Forecast } from '$lib/types/forecast';

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

describe('ForecastOverlay', () => {
  afterEach(() => {
    cleanup();
  });

  it('renders canvas with data-testid="forecast-overlay"', () => {
    render(ForecastOverlay, { props: { forecasts: [buildForecast()] } });
    expect(screen.getByTestId('forecast-overlay')).toBeInTheDocument();
  });

  it('renders without crashing with single forecast', () => {
    render(ForecastOverlay, { props: { forecasts: [buildForecast()] } });
    expect(screen.getByLabelText('Forecast overlay chart')).toBeInTheDocument();
  });

  it('renders without crashing with multiple forecasts', () => {
    const forecasts: Forecast[] = [
      buildForecast({ forecastId: 'f-1', versionLabel: 'Original', forecastType: 'ORIGINAL' }),
      buildForecast({
        forecastId: 'f-2',
        versionLabel: 'Re-forecast Apr 10',
        forecastType: 'REFORECAST',
        startDay: 10
      })
    ];
    render(ForecastOverlay, { props: { forecasts } });
    expect(screen.getByTestId('forecast-overlay')).toBeInTheDocument();
  });

  it('renders without crashing with empty array', () => {
    render(ForecastOverlay, { props: { forecasts: [] } });
    expect(screen.getByTestId('forecast-overlay')).toBeInTheDocument();
  });
});
