import { cleanup, render, screen } from '@testing-library/svelte';
import { afterEach, describe, expect, it } from 'vitest';
import ForecastChart from '$lib/components/ForecastChart.svelte';
import type { DailyEntry } from '$lib/types/forecast';

describe('ForecastChart', () => {
  afterEach(() => {
    cleanup();
  });

  it('renders canvas element for chart', () => {
    render(ForecastChart, {
      props: {
        dailyEntries: []
      }
    });

    expect(screen.getByTestId('forecast-chart')).toBeInTheDocument();
    expect(screen.getByLabelText('Forecast chart')).toBeInTheDocument();
  });

  it('accepts DailyEntry[] prop', () => {
    const sampleEntries: DailyEntry[] = [
      { dayNumber: 1, remainingBalance: 1500, dailyExpenseTotal: 50, breakdown: [] },
      { dayNumber: 2, remainingBalance: 1450, dailyExpenseTotal: 40, breakdown: [] }
    ];

    render(ForecastChart, {
      props: {
        dailyEntries: sampleEntries
      }
    });

    expect(screen.getByTestId('forecast-chart')).toBeInTheDocument();
  });
});
