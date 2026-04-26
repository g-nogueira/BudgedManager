import { beforeEach, describe, expect, it, vi } from 'vitest';
import { get } from 'svelte/store';
import type { Budget, Expense, IncomeSource } from '$lib/types/budget';

vi.mock('$lib/api/budgetApi', () => ({
  createBudget: vi.fn(),
  getBudgetById: vi.fn(),
  getBudgetByMonth: vi.fn(),
  addIncome: vi.fn(),
  updateIncome: vi.fn(),
  removeIncome: vi.fn(),
  addExpense: vi.fn(),
  updateExpense: vi.fn(),
  removeExpense: vi.fn(),
  toggleExclusion: vi.fn(),
  activateBudget: vi.fn()
}));

import * as budgetApi from '$lib/api/budgetApi';
import {
  budget,
  budgetError,
  budgetLoading,
  netBalance,
  storeAddIncome,
  storeCreateBudget,
  storeRemoveExpense,
  storeToggleExclusion,
  totalExpenses
} from '$lib/stores/budgetStore';

const buildIncome = (overrides?: Partial<IncomeSource>): IncomeSource => ({
  incomeId: 'income-1',
  name: 'Salary',
  amount: 4000,
  ...overrides
});

const buildExpense = (overrides?: Partial<Expense>): Expense => ({
  expenseId: 'expense-1',
  name: 'Rent',
  category: 'FIXED',
  dayOfMonth: 1,
  isSpread: false,
  amount: 1200,
  isExcluded: false,
  ...overrides
});

const buildBudget = (overrides?: Partial<Budget>): Budget => ({
  budgetId: 'budget-1',
  householdId: 'household-1',
  yearMonth: '2026-04',
  status: 'DRAFT',
  incomeSources: [buildIncome()],
  expenses: [buildExpense()],
  totalIncome: 4000,
  createdAt: '2026-04-01T00:00:00Z',
  updatedAt: '2026-04-01T00:00:00Z',
  ...overrides
});

describe('budgetStore', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    budget.set(null);
    budgetError.set(null);
    budgetLoading.set(false);
  });

  it('storeAddIncome calls API and re-fetches budget', async () => {
    const updatedBudget = buildBudget({
      incomeSources: [buildIncome(), buildIncome({ incomeId: 'income-2' })]
    });

    vi.mocked(budgetApi.addIncome).mockResolvedValue({ incomeId: 'income-2' });
    vi.mocked(budgetApi.getBudgetById).mockResolvedValue(updatedBudget);

    await storeAddIncome('budget-1', { name: 'Freelance', amount: 500 });

    expect(budgetApi.addIncome).toHaveBeenCalledWith('budget-1', {
      name: 'Freelance',
      amount: 500
    });
    expect(budgetApi.getBudgetById).toHaveBeenCalledWith('budget-1');
    expect(get(budget)?.incomeSources).toHaveLength(2);
  });

  it('storeRemoveExpense calls API and re-fetches budget', async () => {
    vi.mocked(budgetApi.removeExpense).mockResolvedValue(undefined);
    vi.mocked(budgetApi.getBudgetById).mockResolvedValue(buildBudget({ expenses: [] }));

    await storeRemoveExpense('budget-1', 'expense-1');

    expect(budgetApi.removeExpense).toHaveBeenCalledWith('budget-1', 'expense-1');
    expect(budgetApi.getBudgetById).toHaveBeenCalledWith('budget-1');
    expect(get(budget)?.expenses).toHaveLength(0);
  });

  it('storeToggleExclusion calls API and re-fetches budget', async () => {
    vi.mocked(budgetApi.toggleExclusion).mockResolvedValue(undefined);
    vi.mocked(budgetApi.getBudgetById).mockResolvedValue(
      buildBudget({ expenses: [buildExpense({ isExcluded: true })] })
    );

    await storeToggleExclusion('budget-1', 'expense-1', true);

    expect(budgetApi.toggleExclusion).toHaveBeenCalledWith('budget-1', 'expense-1', {
      isExcluded: true
    });
    expect(get(budget)?.expenses[0]?.isExcluded).toBe(true);
  });

  it('totalExpenses derives sum for non-excluded expenses', () => {
    budget.set(
      buildBudget({
        expenses: [
          buildExpense({ expenseId: 'expense-a', amount: 100, isExcluded: false }),
          buildExpense({ expenseId: 'expense-b', amount: 40, isExcluded: true }),
          buildExpense({ expenseId: 'expense-c', amount: 60, isExcluded: false })
        ]
      })
    );

    expect(get(totalExpenses)).toBe(160);
  });

  it('netBalance derives totalIncome minus totalExpenses', () => {
    budget.set(
      buildBudget({
        totalIncome: 1000,
        expenses: [
          buildExpense({ expenseId: 'expense-a', amount: 200, isExcluded: false }),
          buildExpense({ expenseId: 'expense-b', amount: 100, isExcluded: false })
        ]
      })
    );

    expect(get(netBalance)).toBe(700);
  });

  it('mutation stores error message when API call fails', async () => {
    vi.mocked(budgetApi.addIncome).mockRejectedValue(new Error('Add failed'));

    await expect(storeAddIncome('budget-1', { name: 'Freelance', amount: 500 })).rejects.toThrow(
      'Add failed'
    );

    expect(get(budgetError)).toBe('Add failed');
    expect(get(budgetLoading)).toBe(false);
  });

  it('storeCreateBudget returns budget id and refreshes state', async () => {
    vi.mocked(budgetApi.createBudget).mockResolvedValue({ budgetId: 'budget-2', status: 'DRAFT' });
    vi.mocked(budgetApi.getBudgetById).mockResolvedValue(buildBudget({ budgetId: 'budget-2' }));

    const result = await storeCreateBudget('2026-05');

    expect(budgetApi.createBudget).toHaveBeenCalledWith({ yearMonth: '2026-05' });
    expect(budgetApi.getBudgetById).toHaveBeenCalledWith('budget-2');
    expect(result.budgetId).toBe('budget-2');
    expect(get(budget)?.budgetId).toBe('budget-2');
  });
});
