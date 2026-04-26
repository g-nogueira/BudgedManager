<script lang="ts">
  import { untrack } from 'svelte';
  import { page } from '$app/state';
  import ExpenseForm from '$lib/components/ExpenseForm.svelte';
  import ExpenseList from '$lib/components/ExpenseList.svelte';
  import IncomeSection from '$lib/components/IncomeSection.svelte';
  import { formatCurrency } from '$lib/utils/formatCurrency';
  import {
    budget,
    budgetError,
    budgetLoading,
    fetchBudgetById,
    netBalance,
    storeActivateBudget,
    storeAddExpense,
    storeAddIncome,
    storeRemoveExpense,
    storeRemoveIncome,
    storeToggleExclusion,
    storeUpdateExpense,
    storeUpdateIncome,
    totalExpenses
  } from '$lib/stores/budgetStore';
  import type {
    AddExpenseRequest,
    AddIncomeRequest,
    Expense,
    UpdateExpenseRequest,
    UpdateIncomeRequest
  } from '$lib/types/budget';

  let initializing = $state($budget === null);
  const routeBudgetId = $derived.by(() => page.params.budgetId ?? '');

  const formatBudgetMonth = (value: string): string => {
    const [yearToken, monthToken] = value.split('-');
    const year = Number.parseInt(yearToken ?? '', 10);
    const month = Number.parseInt(monthToken ?? '', 10);

    if (Number.isNaN(year) || Number.isNaN(month) || month < 1 || month > 12) {
      return value;
    }

    return new Intl.DateTimeFormat('en-US', {
      month: 'long',
      year: 'numeric',
      timeZone: 'UTC'
    }).format(new Date(Date.UTC(year, month - 1, 1)));
  };

  const withBudget = async (handler: (budgetId: string) => Promise<void>): Promise<void> => {
    const currentBudget = $budget;

    if (!currentBudget || currentBudget.status === 'CLOSED') {
      return;
    }

    await handler(currentBudget.budgetId);
  };

  const handleAddIncome = async (request: AddIncomeRequest): Promise<void> => {
    await withBudget(async (budgetId) => {
      await storeAddIncome(budgetId, request);
    });
  };

  const handleUpdateIncome = async (
    incomeId: string,
    request: UpdateIncomeRequest
  ): Promise<void> => {
    await withBudget(async (budgetId) => {
      await storeUpdateIncome(budgetId, incomeId, request);
    });
  };

  const handleRemoveIncome = async (incomeId: string): Promise<void> => {
    await withBudget(async (budgetId) => {
      await storeRemoveIncome(budgetId, incomeId);
    });
  };

  const handleAddExpense = async (request: AddExpenseRequest): Promise<void> => {
    await withBudget(async (budgetId) => {
      await storeAddExpense(budgetId, request);
    });
  };

  const handleUpdateExpense = async (
    expenseId: string,
    request: UpdateExpenseRequest
  ): Promise<void> => {
    await withBudget(async (budgetId) => {
      await storeUpdateExpense(budgetId, expenseId, request);
    });
  };

  const handleRemoveExpense = async (expenseId: string): Promise<void> => {
    await withBudget(async (budgetId) => {
      await storeRemoveExpense(budgetId, expenseId);
    });
  };

  const handleToggleExclusion = async (expenseId: string, isExcluded: boolean): Promise<void> => {
    await withBudget(async (budgetId) => {
      await storeToggleExclusion(budgetId, expenseId, isExcluded);
    });
  };

  const handleActivate = async (): Promise<void> => {
    const currentBudget = $budget;

    if (!currentBudget || currentBudget.incomeSources.length === 0) {
      return;
    }

    try {
      await storeActivateBudget(currentBudget.budgetId);
    } catch {
      return;
    }
  };

  $effect(() => {
    const budgetId = routeBudgetId;

    if (budgetId.length === 0) {
      initializing = false;
      return;
    }

    const currentBudget = untrack(() => $budget);
    const shouldShowLoading = !currentBudget || currentBudget.budgetId !== budgetId;

    if (shouldShowLoading) {
      initializing = true;
    }

    let cancelled = false;

    void (async () => {
      try {
        await fetchBudgetById(budgetId);
      } finally {
        if (!cancelled) {
          initializing = false;
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  });

  const budgetStatusClass = $derived.by(() => {
    const currentBudget = $budget;

    if (!currentBudget) {
      return 'status-badge';
    }

    if (currentBudget.status === 'ACTIVE') {
      return 'status-badge status-active';
    }

    if (currentBudget.status === 'CLOSED') {
      return 'status-badge status-closed';
    }

    return 'status-badge status-draft';
  });

  const expensesForList = $derived.by(() => {
    const currentBudget = $budget;
    return currentBudget?.expenses ?? ([] as Expense[]);
  });

  const showInitialLoading = $derived.by(() => {
    const currentBudget = $budget;

    return (
      initializing ||
      ($budgetLoading &&
        (!currentBudget ||
          (routeBudgetId.length > 0 && currentBudget.budgetId !== routeBudgetId)))
    );
  });

  const canActivateBudget = $derived.by(() => {
    const currentBudget = $budget;

    if (!currentBudget) {
      return false;
    }

    return currentBudget.incomeSources.length > 0;
  });
</script>

{#if showInitialLoading}
  <section class="budget-state" data-testid="budget-detail-loading">
    Loading budget details...
  </section>
{:else if !$budget && $budgetError}
  <section class="budget-state budget-state-error" data-testid="budget-detail-error">
    {$budgetError}
  </section>
{:else if !$budget}
  <section class="budget-state" data-testid="budget-detail-empty">
    No budget data found for this month.
  </section>
{:else}
  <section class="budget-detail-page" data-testid="budget-detail-page">
    <header class="budget-detail-header">
      <div class="budget-title-block">
        <h1 data-testid="budget-detail-heading">{formatBudgetMonth($budget.yearMonth)} Budget</h1>
        <span class={budgetStatusClass} data-testid="budget-status-badge">{$budget.status}</span>
      </div>

      {#if $budget.status === 'DRAFT'}
        <button
          type="button"
          class="activate-button"
          onclick={handleActivate}
          disabled={!canActivateBudget || $budgetLoading}
          data-testid="activate-budget-button"
        >
          Activate Budget
        </button>
      {/if}
    </header>

    {#if $budgetError}
      <p class="budget-inline-error" data-testid="budget-detail-inline-error">{$budgetError}</p>
    {/if}

    <IncomeSection
      incomes={$budget.incomeSources}
      totalIncome={$budget.totalIncome}
      onAdd={handleAddIncome}
      onUpdate={handleUpdateIncome}
      onRemove={handleRemoveIncome}
    />

    <ExpenseList
      expenses={expensesForList}
      onToggleExclusion={handleToggleExclusion}
      onUpdate={handleUpdateExpense}
      onRemove={handleRemoveExpense}
    />

    <ExpenseForm onSubmit={handleAddExpense} />
  </section>

  <footer class="budget-summary-bar" data-testid="budget-summary-bar">
    <div class="summary-block">
      <span>Total Income</span>
      <strong data-testid="summary-total-income">{formatCurrency($budget.totalIncome)}</strong>
    </div>

    <div class="summary-block summary-expenses">
      <span>Total Expenses</span>
      <strong data-testid="summary-total-expenses">{formatCurrency($totalExpenses)}</strong>
    </div>

    <div class="summary-block summary-net">
      <span>Projected Remaining</span>
      <strong data-testid="summary-net-balance">{formatCurrency($netBalance)}</strong>
    </div>

    <a href="/forecast" class="forecast-link" data-testid="generate-forecast-link"
      >Generate Forecast</a
    >
  </footer>
{/if}

<style>
  .budget-detail-page {
    display: grid;
    gap: 1rem;
    max-width: 1120px;
    margin: 0 auto;
    padding: 1rem 1rem 8.5rem;
  }

  .budget-detail-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 1rem;
  }

  .budget-title-block {
    display: grid;
    gap: 0.5rem;
  }

  .budget-title-block h1 {
    margin: 0;
    color: #9116c4;
    font-size: 2rem;
  }

  .status-badge {
    display: inline-flex;
    justify-content: center;
    align-items: center;
    width: fit-content;
    border-radius: 999px;
    padding: 0.2rem 0.6rem;
    font-size: 0.7rem;
    font-weight: 800;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    border: 1px solid #abadb0;
  }

  .status-draft {
    background: #eef1f4;
    color: #595c5e;
  }

  .status-active {
    background: #dcfce7;
    border-color: #86efac;
    color: #166534;
  }

  .status-closed {
    background: #fee2e2;
    border-color: #fca5a5;
    color: #991b1b;
  }

  .activate-button {
    border: none;
    border-radius: 999px;
    min-height: 2.7rem;
    padding: 0 1rem;
    color: #ffffff;
    background: linear-gradient(135deg, #9116c4 0%, #d67aff 100%);
    font-weight: 700;
    cursor: pointer;
  }

  .activate-button:disabled {
    opacity: 0.65;
    cursor: not-allowed;
  }

  .budget-inline-error {
    margin: 0;
    color: #b41340;
    font-weight: 600;
  }

  .budget-state {
    margin: 1rem auto;
    max-width: 720px;
    padding: 1rem;
    border: 1px solid #d9dde1;
    border-radius: 0.75rem;
    background: #ffffff;
    color: #2c2f32;
  }

  .budget-state-error {
    border-color: #f2b8c5;
    color: #b41340;
  }

  .budget-summary-bar {
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    display: grid;
    grid-template-columns: repeat(3, minmax(120px, 1fr)) auto;
    align-items: center;
    gap: 0.9rem;
    padding: 0.9rem 1.1rem;
    background: rgba(255, 255, 255, 0.94);
    border-top: 1px solid #d9dde1;
    backdrop-filter: blur(8px);
  }

  .summary-block {
    display: grid;
    gap: 0.2rem;
  }

  .summary-block span {
    margin: 0;
    font-size: 0.72rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: #595c5e;
    font-weight: 700;
  }

  .summary-block strong {
    margin: 0;
    color: #2c2f32;
    font-size: 1.1rem;
  }

  .summary-expenses strong {
    color: #b41340;
  }

  .summary-net strong {
    color: #9116c4;
  }

  .forecast-link {
    display: inline-flex;
    justify-content: center;
    align-items: center;
    min-height: 2.65rem;
    border-radius: 999px;
    text-decoration: none;
    padding: 0 1rem;
    color: #ffffff;
    background: linear-gradient(135deg, #9116c4 0%, #d67aff 100%);
    font-weight: 700;
  }

  @media (max-width: 860px) {
    .budget-detail-page {
      padding-bottom: calc(16rem + env(safe-area-inset-bottom));
    }

    .budget-summary-bar {
      grid-template-columns: 1fr;
      text-align: center;
    }

    .summary-block {
      justify-items: center;
    }
  }
</style>
