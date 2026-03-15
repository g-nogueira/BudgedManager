import { getAuthHeader } from '$lib/stores/authStore';
import { API_BASE, handleResponse } from '$lib/api/apiBase';
import type {
  ActivateBudgetResult,
  AddExpenseRequest,
  AddExpenseResult,
  AddIncomeRequest,
  AddIncomeResult,
  Budget,
  CreateBudgetRequest,
  CreateBudgetResult,
  RolloverMonthRequest,
  RolloverMonthResult,
  ToggleExclusionRequest,
  UpdateExpenseRequest,
  UpdateIncomeRequest
} from '$lib/types/budget';

export const createBudget = async (request: CreateBudgetRequest): Promise<CreateBudgetResult> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<CreateBudgetResult>(response);
};

export const getBudgetById = async (budgetId: string): Promise<Budget> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}`, {
    headers: { ...getAuthHeader() }
  });

  return handleResponse<Budget>(response);
};

export const getBudgetByMonth = async (yearMonth: string): Promise<Budget> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/by-month/${yearMonth}`, {
    headers: { ...getAuthHeader() }
  });

  return handleResponse<Budget>(response);
};

export const addIncome = async (
  budgetId: string,
  request: AddIncomeRequest
): Promise<AddIncomeResult> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/income`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<AddIncomeResult>(response);
};

export const updateIncome = async (
  budgetId: string,
  incomeId: string,
  request: UpdateIncomeRequest
): Promise<void> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/income/${incomeId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  await handleResponse<void>(response);
};

export const removeIncome = async (budgetId: string, incomeId: string): Promise<void> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/income/${incomeId}`, {
    method: 'DELETE',
    headers: { ...getAuthHeader() }
  });

  await handleResponse<void>(response);
};

export const addExpense = async (
  budgetId: string,
  request: AddExpenseRequest
): Promise<AddExpenseResult> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/expenses`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<AddExpenseResult>(response);
};

export const updateExpense = async (
  budgetId: string,
  expenseId: string,
  request: UpdateExpenseRequest
): Promise<void> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/expenses/${expenseId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  await handleResponse<void>(response);
};

export const removeExpense = async (budgetId: string, expenseId: string): Promise<void> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/expenses/${expenseId}`, {
    method: 'DELETE',
    headers: { ...getAuthHeader() }
  });

  await handleResponse<void>(response);
};

export const toggleExclusion = async (
  budgetId: string,
  expenseId: string,
  request: ToggleExclusionRequest
): Promise<void> => {
  const response = await fetch(
    `${API_BASE}/api/v1/budgets/${budgetId}/expenses/${expenseId}/exclusion`,
    {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
      body: JSON.stringify(request)
    }
  );

  await handleResponse<void>(response);
};

export const activateBudget = async (budgetId: string): Promise<ActivateBudgetResult> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/activate`, {
    method: 'POST',
    headers: { ...getAuthHeader() }
  });

  return handleResponse<ActivateBudgetResult>(response);
};

export const rolloverMonth = async (
  budgetId: string,
  request: RolloverMonthRequest
): Promise<RolloverMonthResult> => {
  const response = await fetch(`${API_BASE}/api/v1/budgets/${budgetId}/rollover`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<RolloverMonthResult>(response);
};
