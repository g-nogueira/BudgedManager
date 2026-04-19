<script lang="ts">
  import type { Expense, ExpenseCategory, UpdateExpenseRequest } from '$lib/types/budget';

  interface Props {
    expenses: Expense[];
    onToggleExclusion: (expenseId: string, isExcluded: boolean) => Promise<void> | void;
    onUpdate: (expenseId: string, request: UpdateExpenseRequest) => Promise<void> | void;
    onRemove: (expenseId: string) => Promise<void> | void;
  }

  let { expenses, onToggleExclusion, onUpdate, onRemove }: Props = $props();

  const categoryOrder: ExpenseCategory[] = ['FIXED', 'SUBSCRIPTION', 'VARIABLE'];

  const categoryLabel: Record<ExpenseCategory, string> = {
    FIXED: 'Fixed Expenses',
    SUBSCRIPTION: 'Subscriptions',
    VARIABLE: 'Variable'
  };

  const categoryIcon: Record<ExpenseCategory, string> = {
    FIXED: 'FX',
    SUBSCRIPTION: 'SUB',
    VARIABLE: 'VAR'
  };

  let expanded = $state<Record<ExpenseCategory, boolean>>({
    FIXED: true,
    SUBSCRIPTION: true,
    VARIABLE: true
  });

  let editingExpenseId = $state<string | null>(null);
  let editName = $state('');
  let editCategory = $state<ExpenseCategory>('FIXED');
  let editAmount = $state('');
  let editDay = $state('1');
  let editSpread = $state(false);
  let editError = $state<string | null>(null);
  let actionError = $state<string | null>(null);
  let isSaving = $state(false);

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('en-IE', {
      style: 'currency',
      currency: 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  };

  const expensesByCategory = (category: ExpenseCategory): Expense[] => {
    return expenses.filter((expense) => expense.category === category);
  };

  const subtotalByCategory = (items: Expense[]): number => {
    return items.reduce((total, item) => total + item.amount, 0);
  };

  const validateExpense = (
    name: string,
    amountInput: string,
    spread: boolean,
    dayInput: string
  ): string | null => {
    if (name.trim().length === 0) {
      return 'Expense name is required.';
    }

    const parsedAmount = Number.parseFloat(amountInput);
    if (Number.isNaN(parsedAmount) || parsedAmount <= 0) {
      return 'Amount must be greater than zero.';
    }

    if (!spread) {
      const parsedDay = Number.parseInt(dayInput, 10);
      if (Number.isNaN(parsedDay) || parsedDay < 1 || parsedDay > 31) {
        return 'Day of month must be between 1 and 31.';
      }
    }

    return null;
  };

  const toErrorMessage = (value: unknown): string => {
    return value instanceof Error ? value.message : 'Action failed.';
  };

  const toggleCategory = (category: ExpenseCategory): void => {
    expanded = {
      ...expanded,
      [category]: !expanded[category]
    };
  };

  const startEdit = (expense: Expense): void => {
    editingExpenseId = expense.expenseId;
    editName = expense.name;
    editCategory = expense.category;
    editAmount = String(expense.amount);
    editSpread = expense.isSpread;
    editDay = expense.dayOfMonth === null ? '' : String(expense.dayOfMonth);
    editError = null;
    actionError = null;
  };

  const cancelEdit = (): void => {
    editingExpenseId = null;
    editError = null;
  };

  const onEditSpreadChange = (event: Event): void => {
    const target = event.currentTarget as HTMLInputElement;
    editSpread = target.checked;

    if (editSpread) {
      editDay = '';
    } else if (editDay.length === 0) {
      editDay = '1';
    }
  };

  const handleUpdate = async (expenseId: string): Promise<void> => {
    editError = null;
    actionError = null;

    const validationError = validateExpense(editName, editAmount, editSpread, editDay);
    if (validationError) {
      editError = validationError;
      return;
    }

    isSaving = true;

    try {
      await onUpdate(expenseId, {
        name: editName.trim(),
        category: editCategory,
        dayOfMonth: editSpread ? null : Number.parseInt(editDay, 10),
        isSpread: editSpread,
        amount: Number.parseFloat(editAmount)
      });

      cancelEdit();
    } catch (error) {
      editError = toErrorMessage(error);
    } finally {
      isSaving = false;
    }
  };

  const handleToggleExclusion = async (expense: Expense): Promise<void> => {
    actionError = null;
    isSaving = true;

    try {
      await onToggleExclusion(expense.expenseId, !expense.isExcluded);
    } catch (error) {
      actionError = toErrorMessage(error);
    } finally {
      isSaving = false;
    }
  };

  const handleRemove = async (expenseId: string): Promise<void> => {
    actionError = null;
    isSaving = true;

    try {
      await onRemove(expenseId);
    } catch (error) {
      actionError = toErrorMessage(error);
    } finally {
      isSaving = false;
    }
  };
</script>

<section class="expense-section" data-testid="expense-list">
  <div class="expense-heading">
    <h2>Recurring Expenses</h2>
    <p>Categorized monthly spending</p>
  </div>

  {#if actionError}
    <p class="expense-error">{actionError}</p>
  {/if}

  {#each categoryOrder as category}
    {@const items = expensesByCategory(category)}
    <section class="expense-category" data-testid={`expense-category-${category}`}>
      <button
        type="button"
        class="expense-category-header"
        onclick={() => toggleCategory(category)}
        data-testid={`category-toggle-${category}`}
      >
        <span class="expense-category-main">
          <span class="chevron">{expanded[category] ? 'v' : '>'}</span>
          <strong>{categoryLabel[category]}</strong>
          <span class="count-badge" data-testid={`category-count-${category}`}
            >{items.length} ITEMS</span
          >
        </span>
        <span class="category-total" data-testid={`category-total-${category}`}>
          {formatCurrency(subtotalByCategory(items))}
        </span>
      </button>

      {#if expanded[category]}
        <div class="expense-category-list">
          {#if items.length === 0}
            <p class="category-empty">No expenses in this category.</p>
          {/if}

          {#each items as expense (expense.expenseId)}
            {#if editingExpenseId === expense.expenseId}
              <article
                class="expense-row expense-row-edit"
                data-testid={`expense-edit-${expense.expenseId}`}
              >
                <label>
                  Name
                  <input bind:value={editName} data-testid={`edit-name-${expense.expenseId}`} />
                </label>

                <label>
                  Category
                  <select
                    bind:value={editCategory}
                    data-testid={`edit-category-${expense.expenseId}`}
                  >
                    <option value="FIXED">Fixed</option>
                    <option value="SUBSCRIPTION">Subscriptions</option>
                    <option value="VARIABLE">Variable</option>
                  </select>
                </label>

                <label>
                  Day
                  <input
                    bind:value={editDay}
                    type="number"
                    min="1"
                    max="31"
                    disabled={editSpread}
                    data-testid={`edit-day-${expense.expenseId}`}
                  />
                </label>

                <label>
                  Amount
                  <input
                    bind:value={editAmount}
                    type="number"
                    min="0"
                    step="0.01"
                    data-testid={`edit-amount-${expense.expenseId}`}
                  />
                </label>

                <label class="spread-toggle">
                  <input type="checkbox" checked={editSpread} onchange={onEditSpreadChange} />
                  Spread across month
                </label>

                {#if editError}
                  <p class="expense-error">{editError}</p>
                {/if}

                <div class="expense-row-actions">
                  <button
                    type="button"
                    onclick={() => handleUpdate(expense.expenseId)}
                    disabled={isSaving}
                  >
                    Save
                  </button>
                  <button type="button" class="secondary" onclick={cancelEdit} disabled={isSaving}>
                    Cancel
                  </button>
                </div>
              </article>
            {:else}
              <article
                class="expense-row"
                class:expense-row-excluded={expense.isExcluded}
                data-testid={`expense-row-${expense.expenseId}`}
              >
                <div class="expense-main">
                  <span class="expense-icon" aria-hidden="true"
                    >{categoryIcon[expense.category]}</span
                  >
                  <div>
                    <p class="expense-name" class:crossed={expense.isExcluded}>{expense.name}</p>
                    {#if expense.isSpread}
                      <span
                        class="badge badge-spread"
                        data-testid={`spread-badge-${expense.expenseId}`}
                      >
                        Spread across month
                      </span>
                    {:else}
                      <span class="badge badge-day" data-testid={`day-badge-${expense.expenseId}`}>
                        Day {expense.dayOfMonth}
                      </span>
                    {/if}
                  </div>
                </div>

                <div class="expense-side">
                  <p class="expense-amount" class:crossed={expense.isExcluded}>
                    {formatCurrency(expense.amount)}
                  </p>
                  <label class="expense-toggle">
                    <input
                      type="checkbox"
                      checked={!expense.isExcluded}
                      onchange={() => handleToggleExclusion(expense)}
                      data-testid={`toggle-expense-${expense.expenseId}`}
                      disabled={isSaving}
                    />
                    <span>{expense.isExcluded ? 'Excluded' : 'Included'}</span>
                  </label>

                  <div class="expense-row-actions">
                    <button
                      type="button"
                      class="secondary"
                      onclick={() => startEdit(expense)}
                      data-testid={`edit-expense-${expense.expenseId}`}
                      disabled={isSaving}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      class="secondary danger"
                      onclick={() => handleRemove(expense.expenseId)}
                      data-testid={`remove-expense-${expense.expenseId}`}
                      disabled={isSaving}
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </article>
            {/if}
          {/each}
        </div>
      {/if}
    </section>
  {/each}
</section>

<style>
  .expense-section {
    display: grid;
    gap: 0.9rem;
  }

  .expense-heading {
    display: grid;
    gap: 0.35rem;
  }

  .expense-heading h2 {
    margin: 0;
    color: #2c2f32;
  }

  .expense-heading p {
    margin: 0;
    color: #595c5e;
  }

  .expense-category {
    border-radius: 0.85rem;
    overflow: hidden;
    border: 1px solid #d9dde1;
    background: #eef1f4;
  }

  .expense-category-header {
    width: 100%;
    border: none;
    background: transparent;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.9rem;
    cursor: pointer;
    color: #2c2f32;
  }

  .expense-category-main {
    display: flex;
    align-items: center;
    gap: 0.6rem;
  }

  .count-badge {
    border-radius: 0.35rem;
    background: rgba(255, 255, 255, 0.6);
    padding: 0.1rem 0.4rem;
    font-size: 0.7rem;
    font-weight: 700;
  }

  .category-total {
    font-weight: 800;
  }

  .expense-category-list {
    padding: 0.9rem;
    display: grid;
    gap: 0.6rem;
  }

  .expense-row {
    border: 1px solid #d9dde1;
    border-radius: 0.65rem;
    background: #ffffff;
    padding: 0.75rem;
    display: flex;
    justify-content: space-between;
    gap: 0.8rem;
  }

  .expense-row-excluded {
    opacity: 0.6;
    background: #e5e8ec;
  }

  .expense-row-edit {
    display: grid;
    gap: 0.5rem;
    grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  }

  .expense-row-edit label {
    display: grid;
    gap: 0.35rem;
    font-weight: 600;
    color: #2c2f32;
  }

  .expense-row-edit input,
  .expense-row-edit select {
    height: 2.2rem;
    border: 1px solid #abadb0;
    border-radius: 0.5rem;
    background: #ffffff;
    padding: 0 0.55rem;
  }

  .expense-main {
    display: flex;
    gap: 0.65rem;
    align-items: flex-start;
  }

  .expense-icon {
    width: 2rem;
    height: 2rem;
    border-radius: 50%;
    background: #f2ddff;
    color: #9116c4;
    font-size: 0.62rem;
    font-weight: 800;
    display: inline-flex;
    justify-content: center;
    align-items: center;
  }

  .expense-name,
  .expense-amount {
    margin: 0;
    font-weight: 700;
    color: #2c2f32;
  }

  .crossed {
    text-decoration: line-through;
  }

  .expense-side {
    display: grid;
    justify-items: end;
    gap: 0.45rem;
  }

  .expense-toggle {
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    color: #595c5e;
    font-size: 0.8rem;
    font-weight: 600;
  }

  .expense-toggle input,
  .spread-toggle input {
    width: 1rem;
    height: 1rem;
  }

  .expense-row-actions {
    display: flex;
    gap: 0.4rem;
    flex-wrap: wrap;
  }

  button {
    border: none;
    border-radius: 999px;
    min-height: 2rem;
    padding: 0 0.85rem;
    background: linear-gradient(135deg, #9116c4 0%, #d67aff 100%);
    color: #ffffff;
    font-weight: 700;
    cursor: pointer;
  }

  .secondary {
    background: #eef1f4;
    color: #2c2f32;
    border: 1px solid #d9dde1;
  }

  .secondary.danger {
    color: #b41340;
    border-color: #f2b8c5;
  }

  button:disabled {
    cursor: not-allowed;
    opacity: 0.6;
  }

  .badge {
    margin-top: 0.25rem;
    display: inline-flex;
    align-items: center;
    border-radius: 0.3rem;
    padding: 0.1rem 0.4rem;
    font-size: 0.68rem;
    font-weight: 800;
    text-transform: uppercase;
    letter-spacing: 0.03em;
  }

  .badge-day {
    color: #595c5e;
    background: #e5e8ec;
  }

  .badge-spread {
    color: #9116c4;
    background: #f2ddff;
  }

  .expense-error {
    margin: 0;
    color: #b41340;
    font-weight: 600;
  }

  .category-empty {
    margin: 0;
    color: #595c5e;
    font-style: italic;
  }

  .spread-toggle {
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    color: #2c2f32;
    font-weight: 600;
  }
</style>
