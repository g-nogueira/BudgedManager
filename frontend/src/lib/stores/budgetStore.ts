import { derived, writable } from 'svelte/store';
import {
  activateBudget,
  addExpense,
  addIncome,
  createBudget,
  getBudgetById,
  getBudgetByMonth,
  removeExpense,
  removeIncome,
  toggleExclusion,
  updateExpense,
  updateIncome
} from '$lib/api/budgetApi';
import type {
  AddExpenseRequest,
  AddIncomeRequest,
  Budget,
  CreateBudgetResult,
  UpdateExpenseRequest,
  UpdateIncomeRequest
} from '$lib/types/budget';

export const budget = writable<Budget | null>(null);
export const budgetLoading = writable(false);
export const budgetError = writable<string | null>(null);

const toErrorMessage = (error: unknown): string => {
  return error instanceof Error ? error.message : 'Unknown error';
};

const refreshBudget = async (budgetId: string): Promise<void> => {
  const result = await getBudgetById(budgetId);
  budget.set(result);
};

const runMutation = async <T>(operation: () => Promise<T>): Promise<T> => {
  budgetLoading.set(true);
  budgetError.set(null);

  try {
    return await operation();
  } catch (error) {
    budgetError.set(toErrorMessage(error));
    throw error;
  } finally {
    budgetLoading.set(false);
  }
};

export const fetchBudgetById = async (budgetId: string): Promise<void> => {
  budgetLoading.set(true);
  budgetError.set(null);

  try {
    await refreshBudget(budgetId);
  } catch (error) {
    budgetError.set(toErrorMessage(error));
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
    budgetError.set(toErrorMessage(error));
    budget.set(null);
  } finally {
    budgetLoading.set(false);
  }
};

export const storeCreateBudget = async (yearMonth: string): Promise<CreateBudgetResult> => {
  return runMutation(async () => {
    const createdBudget = await createBudget({ yearMonth });
    await refreshBudget(createdBudget.budgetId);
    return createdBudget;
  });
};

export const storeAddIncome = async (
  budgetId: string,
  request: AddIncomeRequest
): Promise<void> => {
  await runMutation(async () => {
    await addIncome(budgetId, request);
    await refreshBudget(budgetId);
  });
};

export const storeUpdateIncome = async (
  budgetId: string,
  incomeId: string,
  request: UpdateIncomeRequest
): Promise<void> => {
  await runMutation(async () => {
    await updateIncome(budgetId, incomeId, request);
    await refreshBudget(budgetId);
  });
};

export const storeRemoveIncome = async (budgetId: string, incomeId: string): Promise<void> => {
  await runMutation(async () => {
    await removeIncome(budgetId, incomeId);
    await refreshBudget(budgetId);
  });
};

export const storeAddExpense = async (
  budgetId: string,
  request: AddExpenseRequest
): Promise<void> => {
  await runMutation(async () => {
    await addExpense(budgetId, request);
    await refreshBudget(budgetId);
  });
};

export const storeUpdateExpense = async (
  budgetId: string,
  expenseId: string,
  request: UpdateExpenseRequest
): Promise<void> => {
  await runMutation(async () => {
    await updateExpense(budgetId, expenseId, request);
    await refreshBudget(budgetId);
  });
};

export const storeRemoveExpense = async (budgetId: string, expenseId: string): Promise<void> => {
  await runMutation(async () => {
    await removeExpense(budgetId, expenseId);
    await refreshBudget(budgetId);
  });
};

export const storeToggleExclusion = async (
  budgetId: string,
  expenseId: string,
  isExcluded: boolean
): Promise<void> => {
  await runMutation(async () => {
    await toggleExclusion(budgetId, expenseId, { isExcluded });
    await refreshBudget(budgetId);
  });
};

export const storeActivateBudget = async (budgetId: string): Promise<void> => {
  await runMutation(async () => {
    await activateBudget(budgetId);
    await refreshBudget(budgetId);
  });
};

export const totalExpenses = derived(budget, ($budget) => {
  if (!$budget) {
    return 0;
  }

  return $budget.expenses.reduce((total, expense) => {
    if (expense.isExcluded) {
      return total;
    }

    return total + expense.amount;
  }, 0);
});

export const netBalance = derived([budget, totalExpenses], ([$budget, $totalExpenses]) => {
  if (!$budget) {
    return 0;
  }

  return $budget.totalIncome - $totalExpenses;
});
