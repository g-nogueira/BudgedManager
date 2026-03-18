import { writable } from 'svelte/store';
import { getBudgetById, getBudgetByMonth } from '$lib/api/budgetApi';
import type { Budget } from '$lib/types/budget';

export const budget = writable<Budget | null>(null);
export const budgetLoading = writable(false);
export const budgetError = writable<string | null>(null);

export const fetchBudgetById = async (budgetId: string): Promise<void> => {
  budgetLoading.set(true);
  budgetError.set(null);

  try {
    const result = await getBudgetById(budgetId);
    budget.set(result);
  } catch (error) {
    budgetError.set(error instanceof Error ? error.message : 'Unknown error');
    budget.set(null);
  } finally {
    budgetLoading.set(false);
  }
};

export const fetchBudgetByMonth = async (yearMonth: string): Promise<void> => {
  budgetLoading.set(true);
  budgetError.set(null);

  try {
    const result = await getBudgetByMonth(yearMonth);
    budget.set(result);
  } catch (error) {
    budgetError.set(error instanceof Error ? error.message : 'Unknown error');
    budget.set(null);
  } finally {
    budgetLoading.set(false);
  }
};
