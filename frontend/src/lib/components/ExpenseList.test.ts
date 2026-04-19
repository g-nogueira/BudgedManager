import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/svelte';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import ExpenseList from '$lib/components/ExpenseList.svelte';
import type { Expense } from '$lib/types/budget';

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

const expenseFixture: Expense[] = [
  buildExpense({
    expenseId: 'fixed-1',
    category: 'FIXED',
    name: 'Rent',
    amount: 1200,
    dayOfMonth: 1
  }),
  buildExpense({
    expenseId: 'sub-1',
    category: 'SUBSCRIPTION',
    name: 'Netflix',
    amount: 16,
    dayOfMonth: 15
  }),
  buildExpense({
    expenseId: 'var-1',
    category: 'VARIABLE',
    name: 'Groceries',
    amount: 400,
    isSpread: true,
    dayOfMonth: null
  }),
  buildExpense({
    expenseId: 'var-2',
    category: 'VARIABLE',
    name: 'Electricity',
    amount: 85,
    dayOfMonth: 19,
    isExcluded: true
  })
];

describe('ExpenseList', () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderComponent = (overrides?: Partial<{ expenses: Expense[] }>) => {
    const onToggleExclusion = vi.fn().mockResolvedValue(undefined);
    const onUpdate = vi.fn().mockResolvedValue(undefined);
    const onRemove = vi.fn().mockResolvedValue(undefined);

    render(ExpenseList, {
      props: {
        expenses: overrides?.expenses ?? expenseFixture,
        onToggleExclusion,
        onUpdate,
        onRemove
      }
    });

    return { onToggleExclusion, onUpdate, onRemove };
  };

  it('groups expenses by category with section headers', () => {
    renderComponent();

    expect(screen.getByText('Fixed Expenses')).toBeInTheDocument();
    expect(screen.getByText('Subscriptions')).toBeInTheDocument();
    expect(screen.getByText('Variable')).toBeInTheDocument();
  });

  it('shows item counts and subtotals by category', () => {
    renderComponent();

    expect(screen.getByTestId('category-count-FIXED')).toHaveTextContent('1 ITEMS');
    expect(screen.getByTestId('category-count-SUBSCRIPTION')).toHaveTextContent('1 ITEMS');
    expect(screen.getByTestId('category-count-VARIABLE')).toHaveTextContent('2 ITEMS');

    expect(screen.getByTestId('category-total-FIXED')).toHaveTextContent('€1,200.00');
    expect(screen.getByTestId('category-total-SUBSCRIPTION')).toHaveTextContent('€16.00');
    expect(screen.getByTestId('category-total-VARIABLE')).toHaveTextContent('€485.00');
  });

  it('shows spread badge for spread expenses', () => {
    renderComponent();

    expect(screen.getByTestId('spread-badge-var-1')).toHaveTextContent('Spread across month');
  });

  it('applies excluded styling with strikethrough and opacity', () => {
    renderComponent();

    const excludedRow = screen.getByTestId('expense-row-var-2');
    expect(excludedRow).toHaveClass('expense-row-excluded');

    const excludedName = screen.getByText('Electricity');
    expect(excludedName).toHaveClass('crossed');

    const amount = screen.getByText('€85.00');
    expect(amount).toHaveClass('crossed');
  });

  it('calls toggle exclusion callback', async () => {
    const { onToggleExclusion } = renderComponent();

    await fireEvent.click(screen.getByTestId('toggle-expense-fixed-1'));

    await waitFor(() => {
      expect(onToggleExclusion).toHaveBeenCalledWith('fixed-1', true);
    });
  });

  it('enters inline edit mode on edit click', async () => {
    renderComponent();

    await fireEvent.click(screen.getByTestId('edit-expense-fixed-1'));

    expect(screen.getByTestId('edit-name-fixed-1')).toBeInTheDocument();
    expect(screen.getByTestId('edit-amount-fixed-1')).toBeInTheDocument();
  });

  it('calls remove callback on delete click', async () => {
    const { onRemove } = renderComponent();

    await fireEvent.click(screen.getByTestId('remove-expense-fixed-1'));

    await waitFor(() => {
      expect(onRemove).toHaveBeenCalledWith('fixed-1');
    });
  });

  it('collapses and expands category content', async () => {
    renderComponent();

    expect(screen.getByTestId('expense-row-fixed-1')).toBeInTheDocument();

    await fireEvent.click(screen.getByTestId('category-toggle-FIXED'));
    expect(screen.queryByTestId('expense-row-fixed-1')).not.toBeInTheDocument();

    await fireEvent.click(screen.getByTestId('category-toggle-FIXED'));
    expect(screen.getByTestId('expense-row-fixed-1')).toBeInTheDocument();
  });
});
