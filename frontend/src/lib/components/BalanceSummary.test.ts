import { cleanup, render, screen } from '@testing-library/svelte';
import { afterEach, describe, expect, it } from 'vitest';
import BalanceSummary from '$lib/components/BalanceSummary.svelte';

describe('BalanceSummary', () => {
  afterEach(() => {
    cleanup();
  });

  it('renders end-of-month balance prominently', () => {
    render(BalanceSummary, {
      props: {
        endOfMonthBalance: 1250.5,
        todayBalance: 600,
        todayDate: 'April 11, 2026',
        yearMonth: '2026-04'
      }
    });

    const endOfMonth = screen.getByTestId('end-of-month-balance');

    expect(endOfMonth).toBeInTheDocument();
    expect(endOfMonth).toHaveClass('end-value');
    expect(endOfMonth.textContent).toContain('1,250.50');
  });

  it("renders today's date and balance", () => {
    render(BalanceSummary, {
      props: {
        endOfMonthBalance: 1000,
        todayBalance: 640.25,
        todayDate: 'April 11, 2026',
        yearMonth: '2026-04'
      }
    });

    expect(screen.getByTestId('today-date')).toHaveTextContent('April 11, 2026');
    expect(screen.getByTestId('today-balance').textContent).toContain('640.25');
  });

  it('applies negative warning styling when balance < 0', () => {
    render(BalanceSummary, {
      props: {
        endOfMonthBalance: -100,
        todayBalance: 20,
        todayDate: 'April 11, 2026',
        yearMonth: '2026-04'
      }
    });

    expect(screen.getByTestId('balance-summary')).toHaveClass('balance-summary-warning');
    expect(screen.getByTestId('negative-warning')).toHaveTextContent(
      'Warning: you may finish this month below zero.'
    );
  });

  it('does not apply warning styling when balance >= 0', () => {
    render(BalanceSummary, {
      props: {
        endOfMonthBalance: 0,
        todayBalance: 0,
        todayDate: 'April 11, 2026',
        yearMonth: '2026-04'
      }
    });

    expect(screen.getByTestId('balance-summary')).not.toHaveClass('balance-summary-warning');
    expect(screen.queryByTestId('negative-warning')).not.toBeInTheDocument();
  });

  it('shows N/A when todayBalance is null', () => {
    render(BalanceSummary, {
      props: {
        endOfMonthBalance: 400,
        todayBalance: null,
        todayDate: 'April 11, 2026',
        yearMonth: '2026-04'
      }
    });

    expect(screen.getByTestId('today-balance')).toHaveTextContent('N/A');
  });

  it('renders budget month label in plain language', () => {
    render(BalanceSummary, {
      props: {
        endOfMonthBalance: 1000,
        todayBalance: 500,
        todayDate: 'April 11, 2026',
        yearMonth: '2026-03'
      }
    });

    expect(screen.getByTestId('budget-month-label')).toHaveTextContent('Budget month: March 2026');
  });
});
