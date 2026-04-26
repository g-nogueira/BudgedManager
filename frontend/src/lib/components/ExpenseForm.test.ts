import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/svelte';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import ExpenseForm from '$lib/components/ExpenseForm.svelte';

describe('ExpenseForm', () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders required fields', () => {
    render(ExpenseForm, {
      props: {
        onSubmit: vi.fn()
      }
    });

    expect(screen.getByTestId('expense-name')).toBeInTheDocument();
    expect(screen.getByTestId('expense-category')).toBeInTheDocument();
    expect(screen.getByTestId('expense-day')).toBeInTheDocument();
    expect(screen.getByTestId('expense-amount')).toBeInTheDocument();
  });

  it('category dropdown includes fixed, subscriptions and variable', () => {
    render(ExpenseForm, {
      props: {
        onSubmit: vi.fn()
      }
    });

    const options = Array.from(
      (screen.getByTestId('expense-category') as HTMLSelectElement).options
    ).map((option) => option.textContent);

    expect(options).toEqual(['Fixed', 'Subscriptions', 'Variable']);
  });

  it('validates empty name', async () => {
    render(ExpenseForm, {
      props: {
        onSubmit: vi.fn()
      }
    });

    await fireEvent.input(screen.getByTestId('expense-amount'), { target: { value: '100' } });
    await fireEvent.click(screen.getByTestId('save-expense-button'));

    expect(screen.getByTestId('expense-form-error')).toHaveTextContent('Expense name is required.');
  });

  it('validates zero amount', async () => {
    render(ExpenseForm, {
      props: {
        onSubmit: vi.fn()
      }
    });

    await fireEvent.input(screen.getByTestId('expense-name'), { target: { value: 'Rent' } });
    await fireEvent.input(screen.getByTestId('expense-amount'), { target: { value: '0' } });
    await fireEvent.click(screen.getByTestId('save-expense-button'));

    expect(screen.getByTestId('expense-form-error')).toHaveTextContent(
      'Amount must be greater than zero.'
    );
  });

  it('validates day outside range 1-31', async () => {
    render(ExpenseForm, {
      props: {
        onSubmit: vi.fn()
      }
    });

    await fireEvent.input(screen.getByTestId('expense-name'), { target: { value: 'Rent' } });
    await fireEvent.input(screen.getByTestId('expense-amount'), { target: { value: '100' } });
    await fireEvent.input(screen.getByTestId('expense-day'), { target: { value: '0' } });
    await fireEvent.click(screen.getByTestId('save-expense-button'));

    expect(screen.getByTestId('expense-form-error')).toHaveTextContent(
      'Day of month must be between 1 and 31.'
    );
  });

  it('spread toggle disables day input and submits null day', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    render(ExpenseForm, {
      props: {
        onSubmit
      }
    });

    await fireEvent.input(screen.getByTestId('expense-name'), { target: { value: 'Groceries' } });
    await fireEvent.input(screen.getByTestId('expense-amount'), { target: { value: '300' } });
    await fireEvent.change(screen.getByTestId('expense-category'), {
      target: { value: 'VARIABLE' }
    });

    const spreadToggle = screen.getByTestId('expense-spread');
    await fireEvent.click(spreadToggle);

    const dayInput = screen.getByTestId('expense-day');
    expect(dayInput).toBeDisabled();
    expect(screen.getByTestId('spread-state')).toHaveTextContent('Spread across month');

    await fireEvent.click(screen.getByTestId('save-expense-button'));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith({
        name: 'Groceries',
        category: 'VARIABLE',
        dayOfMonth: null,
        isSpread: true,
        amount: 300
      });
    });
  });

  it('clears form after successful submission', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    render(ExpenseForm, {
      props: {
        onSubmit
      }
    });

    const nameInput = screen.getByTestId('expense-name') as HTMLInputElement;
    const amountInput = screen.getByTestId('expense-amount') as HTMLInputElement;

    await fireEvent.input(nameInput, { target: { value: 'Internet' } });
    await fireEvent.input(amountInput, { target: { value: '80' } });
    await fireEvent.click(screen.getByTestId('save-expense-button'));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    expect(nameInput.value).toBe('');
    expect(amountInput.value).toBe('');
  });
});
