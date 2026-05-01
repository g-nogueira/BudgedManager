import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/svelte';
import { afterEach, describe, expect, it, vi } from 'vitest';
import ReforecastAdjustmentList from '$lib/components/ReforecastAdjustmentList.svelte';
import type { Expense } from '$lib/types/budget';
import type { ExpenseAdjustment } from '$lib/types/forecast';

const buildExpense = (overrides?: Partial<Expense>): Expense => ({
  expenseId: 'e-1',
  name: 'Rent',
  category: 'FIXED',
  dayOfMonth: 1,
  isSpread: false,
  amount: 1200,
  isExcluded: false,
  ...overrides
});

const expenseFixture: Expense[] = [
  buildExpense({ expenseId: 'e-1', name: 'Rent', amount: 1200, dayOfMonth: 1, category: 'FIXED' }),
  buildExpense({
    expenseId: 'e-2',
    name: 'Electricity',
    amount: 85,
    dayOfMonth: 19,
    category: 'VARIABLE'
  }),
  buildExpense({
    expenseId: 'e-3',
    name: 'Groceries',
    amount: 400,
    isSpread: true,
    dayOfMonth: null,
    category: 'VARIABLE'
  })
];

describe('ReforecastAdjustmentList', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('renders expense rows', () => {
    render(ReforecastAdjustmentList, {
      props: { expenses: expenseFixture, onAdjustmentsChange: vi.fn() }
    });

    const rows = screen.getAllByTestId('expense-row');
    expect(rows).toHaveLength(3);
    expect(screen.getByText('Rent')).toBeTruthy();
    expect(screen.getByText('Electricity')).toBeTruthy();
    expect(screen.getByText('Groceries')).toBeTruthy();
  });

  it('shows empty state when no expenses', () => {
    render(ReforecastAdjustmentList, {
      props: { expenses: [], onAdjustmentsChange: vi.fn() }
    });

    expect(screen.getByTestId('no-future-expenses')).toBeTruthy();
  });

  it('tracks MODIFY adjustment when amount changes', async () => {
    const onAdjustmentsChange = vi.fn();
    render(ReforecastAdjustmentList, {
      props: { expenses: expenseFixture, onAdjustmentsChange }
    });

    const amountInputs = screen.getAllByTestId('amount-input');
    fireEvent.input(amountInputs[0], { target: { value: '1350' } });

    await waitFor(() => {
      expect(onAdjustmentsChange).toHaveBeenCalledWith(
        expect.arrayContaining<ExpenseAdjustment>([
          { action: 'MODIFY', originalExpenseId: 'e-1', newAmount: 1350 }
        ])
      );
    });
  });

  it('marks row as REMOVE and shows strikethrough', async () => {
    const onAdjustmentsChange = vi.fn();
    render(ReforecastAdjustmentList, {
      props: { expenses: expenseFixture, onAdjustmentsChange }
    });

    const removeButtons = screen.getAllByTestId('remove-button');
    fireEvent.click(removeButtons[0]);

    await waitFor(() => {
      expect(onAdjustmentsChange).toHaveBeenCalledWith(
        expect.arrayContaining<ExpenseAdjustment>([
          { action: 'REMOVE', originalExpenseId: 'e-1' }
        ])
      );
    });

    // row should show undo button now
    expect(screen.getByTestId('undo-remove-button')).toBeTruthy();
  });

  it('can undo removal', async () => {
    const onAdjustmentsChange = vi.fn();
    render(ReforecastAdjustmentList, {
      props: { expenses: expenseFixture, onAdjustmentsChange }
    });

    const removeButtons = screen.getAllByTestId('remove-button');
    fireEvent.click(removeButtons[0]);

    await waitFor(() => expect(screen.getByTestId('undo-remove-button')).toBeTruthy());

    fireEvent.click(screen.getByTestId('undo-remove-button'));

    await waitFor(() => {
      // after undo, REMOVE adjustment should be gone
      const lastCall = onAdjustmentsChange.mock.calls.at(-1)?.[0] as ExpenseAdjustment[];
      expect(lastCall.some((a) => a.action === 'REMOVE' && a.originalExpenseId === 'e-1')).toBe(
        false
      );
    });
  });

  it('adds new expense via inline form', async () => {
    const onAdjustmentsChange = vi.fn();
    render(ReforecastAdjustmentList, {
      props: { expenses: expenseFixture, onAdjustmentsChange }
    });

    fireEvent.click(screen.getByTestId('add-expense-button'));
    await waitFor(() => expect(screen.getByTestId('add-expense-form')).toBeTruthy());

    fireEvent.input(screen.getByTestId('add-name-input'), {
      target: { value: 'Car Repair' }
    });
    fireEvent.input(screen.getByTestId('add-amount-input'), {
      target: { value: '500' }
    });
    fireEvent.input(screen.getByTestId('add-day-input'), {
      target: { value: '20' }
    });

    fireEvent.submit(screen.getByTestId('add-expense-form'));

    await waitFor(() => {
      expect(onAdjustmentsChange).toHaveBeenCalledWith(
        expect.arrayContaining<ExpenseAdjustment>([
          expect.objectContaining({ action: 'ADD', name: 'Car Repair', newAmount: 500 })
        ])
      );
    });

    expect(screen.getByTestId('added-expense-name')).toHaveTextContent('Car Repair');
  });
});
