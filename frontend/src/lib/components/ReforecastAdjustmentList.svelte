<script lang="ts">
  import { untrack } from 'svelte';
  import { formatCurrency } from '$lib/utils/formatCurrency';
  import { validateExpenseInput } from '$lib/utils/expenseValidation';
  import type { Expense, ExpenseCategory } from '$lib/types/budget';
  import type { ExpenseAdjustment } from '$lib/types/forecast';

  interface Props {
    expenses: Expense[];
    onAdjustmentsChange: (adjustments: ExpenseAdjustment[]) => void;
  }

  let { expenses, onAdjustmentsChange }: Props = $props();

  // Local state for tracking adjustments per existing expense
  type ExpenseState = 'default' | 'modified' | 'removed';

  interface ExpenseRow {
    expense: Expense;
    state: ExpenseState;
    editedAmount: string;
  }

  let rows = $state<ExpenseRow[]>(untrack(() => expenses.map((e) => ({ expense: e, state: 'default' as ExpenseState, editedAmount: String(e.amount) }))));

  // Add-expense form state
  let showAddForm = $state(false);
  let addName = $state('');
  let addCategory = $state<ExpenseCategory>('VARIABLE');
  let addAmount = $state('');
  let addDay = $state('');
  let addSpread = $state(false);
  let addError = $state<string | null>(null);

  interface AddedExpense {
    name: string;
    category: ExpenseCategory;
    dayOfMonth: number | null;
    isSpread: boolean;
    amount: number;
  }

  let addedExpenses = $state<AddedExpense[]>([]);

  const rebuildAdjustments = (): void => {
    const adjustments: ExpenseAdjustment[] = [];

    for (const row of rows) {
      if (row.state === 'removed') {
        adjustments.push({ action: 'REMOVE', originalExpenseId: row.expense.expenseId });
      } else if (row.state === 'modified') {
        adjustments.push({
          action: 'MODIFY',
          originalExpenseId: row.expense.expenseId,
          newAmount: Number.parseFloat(row.editedAmount)
        });
      }
    }

    for (const added of addedExpenses) {
      adjustments.push({
        action: 'ADD',
        name: added.name,
        category: added.category,
        dayOfMonth: added.dayOfMonth ?? undefined,
        isSpread: added.isSpread,
        newAmount: added.amount
      });
    }

    onAdjustmentsChange(adjustments);
  };

  const handleAmountInput = (index: number, value: string): void => {
    const row = rows[index];
    rows[index] = { ...row, editedAmount: value };
    const parsed = Number.parseFloat(value);
    if (Number.isFinite(parsed) && parsed > 0 && parsed !== row.expense.amount) {
      rows[index] = { ...rows[index], state: 'modified' };
    } else if (Number.isFinite(parsed) && parsed === row.expense.amount) {
      rows[index] = { ...rows[index], state: 'default' };
    } else {
      // empty or invalid input — revert to default, do not emit a MODIFY with NaN
      rows[index] = { ...rows[index], state: 'default' };
    }
    rebuildAdjustments();
  };

  const handleRemove = (index: number): void => {
    rows[index] = { ...rows[index], state: 'removed' };
    rebuildAdjustments();
  };

  const handleUndoRemove = (index: number): void => {
    rows[index] = { ...rows[index], state: 'default' };
    rebuildAdjustments();
  };

  const handleAddSpreadToggle = (event: Event): void => {
    const target = event.currentTarget as HTMLInputElement;
    addSpread = target.checked;
    if (addSpread) {
      addDay = '';
    } else if (addDay.length === 0) {
      addDay = '1';
    }
  };

  const handleAddSubmit = (event: SubmitEvent): void => {
    event.preventDefault();
    addError = null;

    const validationError = validateExpenseInput({
      name: addName,
      amountInput: addAmount,
      isSpread: addSpread,
      dayInput: addDay
    });

    if (validationError) {
      addError = validationError;
      return;
    }

    addedExpenses = [
      ...addedExpenses,
      {
        name: addName.trim(),
        category: addCategory,
        dayOfMonth: addSpread ? null : Number.parseInt(addDay, 10),
        isSpread: addSpread,
        amount: Number.parseFloat(addAmount)
      }
    ];

    addName = '';
    addAmount = '';
    addDay = '';
    addSpread = false;
    addCategory = 'VARIABLE';
    showAddForm = false;
    rebuildAdjustments();
  };

  const handleRemoveAdded = (index: number): void => {
    addedExpenses = addedExpenses.filter((_, i) => i !== index);
    rebuildAdjustments();
  };

  const categoryLabel: Record<ExpenseCategory, string> = {
    FIXED: 'Fixed',
    SUBSCRIPTION: 'Sub',
    VARIABLE: 'Variable'
  };
</script>

<div class="adjustment-list" data-testid="adjustment-list">
  {#if rows.length === 0 && addedExpenses.length === 0}
    <p class="empty-state" data-testid="no-future-expenses">No upcoming expenses from this date onward.</p>
  {/if}

  {#each rows as row, i (row.expense.expenseId)}
    <div
      class="expense-row"
      class:expense-row--modified={row.state === 'modified'}
      class:expense-row--removed={row.state === 'removed'}
      data-testid="expense-row"
    >
      <span class="expense-name" data-testid="expense-name">{row.expense.name}</span>
      <span class="category-chip">{categoryLabel[row.expense.category]}</span>
      {#if row.expense.isSpread}
        <span class="day-label">Spread</span>
      {:else}
        <span class="day-label">Day {row.expense.dayOfMonth}</span>
      {/if}

      {#if row.state === 'removed'}
        <span class="amount-display" data-testid="expense-amount">{formatCurrency(row.expense.amount)}</span>
        <button
          type="button"
          class="btn-undo"
          data-testid="undo-remove-button"
          onclick={() => handleUndoRemove(i)}
        >
          Undo
        </button>
      {:else}
        <input
          type="number"
          class="amount-input"
          data-testid="amount-input"
          value={row.editedAmount}
          min="0.01"
          step="0.01"
          oninput={(e) => handleAmountInput(i, (e.currentTarget as HTMLInputElement).value)}
        />
        <button
          type="button"
          class="btn-remove"
          data-testid="remove-button"
          onclick={() => handleRemove(i)}
        >
          Remove
        </button>
      {/if}
    </div>
  {/each}

  {#each addedExpenses as added, i (i)}
    <div class="expense-row expense-row--added" data-testid="added-expense-row">
      <span class="expense-name" data-testid="added-expense-name">{added.name}</span>
      <span class="category-chip">{categoryLabel[added.category]}</span>
      {#if added.isSpread}
        <span class="day-label">Spread</span>
      {:else}
        <span class="day-label">Day {added.dayOfMonth}</span>
      {/if}
      <span class="amount-display">{formatCurrency(added.amount)}</span>
      <button
        type="button"
        class="btn-remove"
        data-testid="remove-added-button"
        onclick={() => handleRemoveAdded(i)}
      >
        Remove
      </button>
    </div>
  {/each}

  {#if showAddForm}
    <form class="add-expense-form" data-testid="add-expense-form" onsubmit={handleAddSubmit} novalidate>
      <h4>Add expense</h4>
      {#if addError}
        <p class="form-error" data-testid="add-expense-error">{addError}</p>
      {/if}
      <div class="add-form-row">
        <input
          type="text"
          placeholder="Name"
          data-testid="add-name-input"
          bind:value={addName}
          required
        />
        <select data-testid="add-category-select" bind:value={addCategory}>
          <option value="FIXED">Fixed</option>
          <option value="SUBSCRIPTION">Subscription</option>
          <option value="VARIABLE">Variable</option>
        </select>
        <input
          type="number"
          placeholder="Amount (€)"
          data-testid="add-amount-input"
          bind:value={addAmount}
          min="0.01"
          step="0.01"
          required
        />
        <label class="spread-label">
          <input
            type="checkbox"
            data-testid="add-spread-checkbox"
            checked={addSpread}
            onchange={handleAddSpreadToggle}
          />
          Spread
        </label>
        {#if !addSpread}
          <input
            type="number"
            placeholder="Day"
            data-testid="add-day-input"
            bind:value={addDay}
            min="1"
            max="31"
          />
        {/if}
      </div>
      <div class="add-form-actions">
        <button type="submit" class="btn-add" data-testid="confirm-add-button">Add</button>
        <button type="button" class="btn-cancel" onclick={() => { showAddForm = false; addError = null; }}>Cancel</button>
      </div>
    </form>
  {:else}
    <button
      type="button"
      class="btn-add-expense"
      data-testid="add-expense-button"
      onclick={() => (showAddForm = true)}
    >
      + Add expense
    </button>
  {/if}
</div>

<style>
  .adjustment-list {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .expense-row {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.5rem 0.75rem;
    border-radius: 6px;
    background: #f9f9f9;
    border: 1px solid #e5e7eb;
  }

  .expense-row--modified {
    background: #fefce8;
    border-color: #fde047;
  }

  .expense-row--removed {
    background: #fff1f2;
    border-color: #fca5a5;
    text-decoration: line-through;
    opacity: 0.7;
  }

  .expense-row--added {
    background: #f0fdf4;
    border-color: #86efac;
    text-decoration: none;
    opacity: 1;
  }

  .expense-name {
    flex: 1;
    font-weight: 500;
  }

  .category-chip {
    font-size: 0.7rem;
    padding: 0.1rem 0.4rem;
    border-radius: 9999px;
    background: #e5e7eb;
    color: #374151;
  }

  .day-label {
    font-size: 0.8rem;
    color: #6b7280;
    min-width: 4rem;
  }

  .amount-input {
    width: 7rem;
    padding: 0.25rem 0.5rem;
    border: 1px solid #d1d5db;
    border-radius: 4px;
    font-size: 0.9rem;
  }

  .amount-display {
    width: 7rem;
    font-size: 0.9rem;
    color: #6b7280;
  }

  .btn-remove {
    padding: 0.2rem 0.6rem;
    background: #fee2e2;
    color: #b91c1c;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.8rem;
  }

  .btn-remove:hover {
    background: #fecaca;
  }

  .btn-undo {
    padding: 0.2rem 0.6rem;
    background: #e5e7eb;
    color: #374151;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.8rem;
  }

  .empty-state {
    color: #6b7280;
    font-style: italic;
    padding: 0.5rem 0;
  }

  .add-expense-form {
    border: 1px dashed #86efac;
    border-radius: 6px;
    padding: 0.75rem;
    background: #f0fdf4;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .add-expense-form h4 {
    margin: 0;
    font-size: 0.9rem;
    color: #166534;
  }

  .add-form-row {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  .add-form-row input[type='text'],
  .add-form-row input[type='number'],
  .add-form-row select {
    padding: 0.25rem 0.5rem;
    border: 1px solid #d1d5db;
    border-radius: 4px;
    font-size: 0.85rem;
  }

  .spread-label {
    display: flex;
    align-items: center;
    gap: 0.25rem;
    font-size: 0.85rem;
    cursor: pointer;
  }

  .add-form-actions {
    display: flex;
    gap: 0.5rem;
  }

  .btn-add {
    padding: 0.25rem 0.75rem;
    background: #16a34a;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.85rem;
  }

  .btn-cancel {
    padding: 0.25rem 0.75rem;
    background: #e5e7eb;
    color: #374151;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.85rem;
  }

  .btn-add-expense {
    padding: 0.4rem 0.75rem;
    background: transparent;
    color: #16a34a;
    border: 1px dashed #86efac;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.85rem;
    align-self: flex-start;
  }

  .btn-add-expense:hover {
    background: #f0fdf4;
  }

  .form-error {
    color: #b91c1c;
    font-size: 0.85rem;
    margin: 0;
  }
</style>
