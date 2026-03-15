import { writable } from 'svelte/store';
import { getForecastById, getForecastsByBudget } from '$lib/api/forecastApi';
import type { Forecast, ForecastSummary } from '$lib/types/forecast';

export const forecast = writable<Forecast | null>(null);
export const forecasts = writable<ForecastSummary[]>([]);
export const forecastLoading = writable(false);
export const forecastError = writable<string | null>(null);

export const fetchForecast = async (budgetId: string, forecastId: string): Promise<void> => {
  forecastLoading.set(true);
  forecastError.set(null);

  try {
    const result = await getForecastById(budgetId, forecastId);
    forecast.set(result);
  } catch (error) {
    forecastError.set(error instanceof Error ? error.message : 'Unknown error');
    forecast.set(null);
  } finally {
    forecastLoading.set(false);
  }
};

export const fetchForecasts = async (budgetId: string): Promise<void> => {
  forecastLoading.set(true);
  forecastError.set(null);

  try {
    const result = await getForecastsByBudget(budgetId);
    forecasts.set(result);
  } catch (error) {
    forecastError.set(error instanceof Error ? error.message : 'Unknown error');
    forecasts.set([]);
  } finally {
    forecastLoading.set(false);
  }
};
