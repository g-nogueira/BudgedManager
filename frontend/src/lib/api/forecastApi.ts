import { getAuthHeader } from '$lib/stores/authStore';
import { API_BASE, handleResponse } from '$lib/api/apiBase';
import type {
  ComparisonResult,
  Forecast,
  ForecastSummary,
  GenerateForecastRequest,
  GenerateForecastResult,
  ReforecastRequest,
  ReforecastResult,
  SaveSnapshotResult
} from '$lib/types/forecast';

export const generateForecast = async (
  budgetId: string,
  request: GenerateForecastRequest
): Promise<GenerateForecastResult> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/forecasts`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<GenerateForecastResult>(response);
};

export const getForecastsByBudget = async (budgetId: string): Promise<ForecastSummary[]> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/forecasts`, {
    headers: { ...getAuthHeader() }
  });

  return handleResponse<ForecastSummary[]>(response);
};

export const getForecastById = async (budgetId: string, forecastId: string): Promise<Forecast> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/forecasts/${forecastId}`, {
    headers: { ...getAuthHeader() }
  });

  return handleResponse<Forecast>(response);
};

export const saveSnapshot = async (
  budgetId: string,
  forecastId: string
): Promise<SaveSnapshotResult> => {
  const response = await fetch(
    `${API_BASE}/api/v1/budgets/${budgetId}/forecasts/${forecastId}/snapshot`,
    {
      method: 'POST',
      headers: { ...getAuthHeader() }
    }
  );

  return handleResponse<SaveSnapshotResult>(response);
};

export const reforecast = async (
  budgetId: string,
  forecastId: string,
  request: ReforecastRequest
): Promise<ReforecastResult> => {
  const response = await fetch(
    `${API_BASE}/api/v1/budgets/${budgetId}/forecasts/${forecastId}/reforecast`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
      body: JSON.stringify(request)
    }
  );

  return handleResponse<ReforecastResult>(response);
};

export const compareForecasts = async (
  budgetId: string,
  versionA: string,
  versionB: string
): Promise<ComparisonResult> => {
  const query = new URLSearchParams({ versionA, versionB }).toString();
  const response = await fetch(
    `${API_BASE}/api/v1/budgets/${budgetId}/forecasts/compare?${query}`,
    {
      headers: { ...getAuthHeader() }
    }
  );

  return handleResponse<ComparisonResult>(response);
};
