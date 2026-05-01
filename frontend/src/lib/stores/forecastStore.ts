import { writable } from 'svelte/store';
import { getForecastById, getForecastsByBudget, reforecast } from '$lib/api/forecastApi';
import type { Forecast, ForecastSummary, ReforecastRequest, ReforecastResult } from '$lib/types/forecast';

export const forecast = writable<Forecast | null>(null);
export const forecasts = writable<ForecastSummary[]>([]);
export const allForecasts = writable<Forecast[]>([]);
export const forecastLoading = writable(false);
export const forecastError = writable<string | null>(null);
export const reforecastResult = writable<ReforecastResult | null>(null);

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

export const fetchAllForecasts = async (budgetId: string): Promise<void> => {
  forecastLoading.set(true);
  forecastError.set(null);

  try {
    const summaries = await getForecastsByBudget(budgetId);
    const nonSnapshots = summaries.filter((s) => !s.isSnapshot);
    const fullForecasts = await Promise.all(
      nonSnapshots.map((s) => getForecastById(budgetId, s.forecastId))
    );
    allForecasts.set(fullForecasts);
  } catch (error) {
    forecastError.set(error instanceof Error ? error.message : 'Unknown error');
    allForecasts.set([]);
  } finally {
    forecastLoading.set(false);
  }
};

export const submitReforecast = async (
  budgetId: string,
  forecastId: string,
  request: ReforecastRequest
): Promise<void> => {
  forecastLoading.set(true);
  forecastError.set(null);
  try {
    const result = await reforecast(budgetId, forecastId, request);
    reforecastResult.set(result);
    await fetchAllForecasts(budgetId);
  } catch (error) {
    forecastError.set(error instanceof Error ? error.message : 'Unknown error');
  } finally {
    forecastLoading.set(false);
  }
};
