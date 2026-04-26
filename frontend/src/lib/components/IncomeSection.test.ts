import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/svelte';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import IncomeSection from '$lib/components/IncomeSection.svelte';
import type { IncomeSource } from '$lib/types/budget';

const buildIncome = (overrides?: Partial<IncomeSource>): IncomeSource => ({
  incomeId: 'income-1',
  name: 'Salary',
  amount: 4200,
  ...overrides
});

describe('IncomeSection', () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders income cards with name and amount', () => {
    render(IncomeSection, {
      props: {
        incomes: [buildIncome()],
        totalIncome: 4200,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    const card = screen.getByTestId('income-card-income-1');

    expect(card).toBeInTheDocument();
    expect(screen.getByText('Salary')).toBeInTheDocument();
    expect(within(card).getByText('€4,200.00')).toBeInTheDocument();
  });

  it('displays total inflow', () => {
    render(IncomeSection, {
      props: {
        incomes: [buildIncome()],
        totalIncome: 5200,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    expect(screen.getByTestId('income-total')).toHaveTextContent('€5,200.00');
  });

  it('shows add income button', () => {
    render(IncomeSection, {
      props: {
        incomes: [],
        totalIncome: 0,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    expect(screen.getByTestId('add-income-button')).toBeInTheDocument();
  });

  it('clicking edit shows edit inputs', async () => {
    render(IncomeSection, {
      props: {
        incomes: [buildIncome()],
        totalIncome: 4200,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    await fireEvent.click(screen.getByTestId('edit-income-income-1'));

    expect(screen.getByTestId('income-edit-name-income-1')).toBeInTheDocument();
    expect(screen.getByTestId('income-edit-amount-income-1')).toBeInTheDocument();
  });

  it('validates empty name on add', async () => {
    render(IncomeSection, {
      props: {
        incomes: [],
        totalIncome: 0,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    await fireEvent.click(screen.getByTestId('add-income-button'));
    await fireEvent.input(screen.getByTestId('add-income-amount'), { target: { value: '200' } });
    await fireEvent.click(screen.getByTestId('save-income-button'));

    expect(screen.getByTestId('add-income-error')).toHaveTextContent('Name is required.');
  });

  it('validates zero amount on add', async () => {
    render(IncomeSection, {
      props: {
        incomes: [],
        totalIncome: 0,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    await fireEvent.click(screen.getByTestId('add-income-button'));
    await fireEvent.input(screen.getByTestId('add-income-name'), {
      target: { value: 'Freelance' }
    });
    await fireEvent.input(screen.getByTestId('add-income-amount'), { target: { value: '0' } });
    await fireEvent.click(screen.getByTestId('save-income-button'));

    expect(screen.getByTestId('add-income-error')).toHaveTextContent(
      'Amount must be greater than zero.'
    );
  });

  it('calls onAdd callback with form data', async () => {
    const onAdd = vi.fn().mockResolvedValue(undefined);

    render(IncomeSection, {
      props: {
        incomes: [],
        totalIncome: 0,
        onAdd,
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    await fireEvent.click(screen.getByTestId('add-income-button'));
    await fireEvent.input(screen.getByTestId('add-income-name'), {
      target: { value: 'Freelance' }
    });
    await fireEvent.input(screen.getByTestId('add-income-amount'), { target: { value: '500' } });
    await fireEvent.click(screen.getByTestId('save-income-button'));

    await waitFor(() => {
      expect(onAdd).toHaveBeenCalledWith({ name: 'Freelance', amount: 500 });
    });
  });

  it('calls onUpdate callback with edited data', async () => {
    const onUpdate = vi.fn().mockResolvedValue(undefined);

    render(IncomeSection, {
      props: {
        incomes: [buildIncome()],
        totalIncome: 4200,
        onAdd: vi.fn(),
        onUpdate,
        onRemove: vi.fn()
      }
    });

    await fireEvent.click(screen.getByTestId('edit-income-income-1'));
    await fireEvent.input(screen.getByTestId('income-edit-name-income-1'), {
      target: { value: 'Main Salary' }
    });
    await fireEvent.input(screen.getByTestId('income-edit-amount-income-1'), {
      target: { value: '4500' }
    });
    await fireEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(onUpdate).toHaveBeenCalledWith('income-1', { name: 'Main Salary', amount: 4500 });
    });
  });

  it('calls onRemove callback', async () => {
    const onRemove = vi.fn().mockResolvedValue(undefined);

    render(IncomeSection, {
      props: {
        incomes: [buildIncome()],
        totalIncome: 4200,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove
      }
    });

    await fireEvent.click(screen.getByTestId('remove-income-income-1'));

    await waitFor(() => {
      expect(onRemove).toHaveBeenCalledWith('income-1');
    });
  });

  it('uses non-technical heading text', () => {
    render(IncomeSection, {
      props: {
        incomes: [],
        totalIncome: 0,
        onAdd: vi.fn(),
        onUpdate: vi.fn(),
        onRemove: vi.fn()
      }
    });

    expect(screen.getByTestId('income-section-heading')).toHaveTextContent('Monthly Income');
  });
});
