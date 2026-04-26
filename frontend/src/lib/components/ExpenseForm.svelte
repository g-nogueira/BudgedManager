<script lang="ts">
  import { validateExpenseInput } from '$lib/utils/expenseValidation';
  import type { AddExpenseRequest, ExpenseCategory } from '$lib/types/budget';

  interface Props {
    onSubmit: (request: AddExpenseRequest) => Promise<void> | void;
  }

  let { onSubmit }: Props = $props();

  let name = $state('');
  let category = $state<ExpenseCategory>('FIXED');
  let dayOfMonth = $state('1');
  let isSpread = $state(false);
  let amount = $state('');
  let error = $state<string | null>(null);
  let submitting = $state(false);

  const toErrorMessage = (value: unknown): string => {
    return value instanceof Error ? value.message : 'Unable to save expense.';
  };

  const resetForm = (): void => {
    name = '';
    category = 'FIXED';
    dayOfMonth = '1';
    isSpread = false;
    amount = '';
  };

  const handleSpreadToggle = (event: Event): void => {
    const target = event.currentTarget as HTMLInputElement;
    isSpread = target.checked;

    if (isSpread) {
      dayOfMonth = '';
    } else if (dayOfMonth.length === 0) {
      dayOfMonth = '1';
    }
  };

  const handleSubmit = async (event: SubmitEvent): Promise<void> => {
    event.preventDefault();
    error = null;

    const validationError = validateExpenseInput({
      name,
      amountInput: amount,
      isSpread,
      dayInput: dayOfMonth
    });

    if (validationError) {
      error = validationError;
      return;
    }

    submitting = true;

    try {
      const request: AddExpenseRequest = {
        name: name.trim(),
        category,
        dayOfMonth: isSpread ? null : Number.parseInt(dayOfMonth, 10),
        isSpread,
        amount: Number.parseFloat(amount)
      };

      await onSubmit(request);
      resetForm();
    } catch (requestError) {
      error = toErrorMessage(requestError);
    } finally {
      submitting = false;
    }
  };
</script>

<form class="expense-form" onsubmit={handleSubmit} data-testid="expense-form" novalidate>
  <h3 class="expense-form-title">Quick Add Expense</h3>

  <div class="expense-form-grid">
    <label>
      Expense Name
      <input bind:value={name} placeholder="e.g. Gym Membership" data-testid="expense-name" />
    </label>

    <label>
      Category
      <select bind:value={category} data-testid="expense-category">
        <option value="FIXED">Fixed</option>
        <option value="SUBSCRIPTION">Subscriptions</option>
        <option value="VARIABLE">Variable</option>
      </select>
    </label>

    <label>
      Frequency/Day
      <input
        bind:value={dayOfMonth}
        type="number"
        min="1"
        max="31"
        disabled={isSpread}
        data-testid="expense-day"
      />
      {#if isSpread}
        <small data-testid="spread-state">Spread across month</small>
      {/if}
    </label>

    <label>
      Amount
      <div class="amount-input-wrap">
        <span class="currency">EUR</span>
        <input
          bind:value={amount}
          type="number"
          min="0"
          step="0.01"
          placeholder="0.00"
          data-testid="expense-amount"
        />
      </div>
    </label>
  </div>

  <label class="spread-toggle">
    <input
      type="checkbox"
      checked={isSpread}
      onchange={handleSpreadToggle}
      data-testid="expense-spread"
    />
    Spread across month
  </label>

  {#if error}
    <p class="expense-error" data-testid="expense-form-error">{error}</p>
  {/if}

  <div class="expense-form-actions">
    <button type="submit" disabled={submitting} data-testid="save-expense-button">
      {#if submitting}
        Saving...
      {:else}
        Save Expense
      {/if}
    </button>
  </div>
</form>

<style>
  .expense-form {
    border: 1px solid #d4b2e3;
    border-radius: 1rem;
    background: linear-gradient(145deg, #ffffff 0%, #f8efff 100%);
    padding: 1rem;
    display: grid;
    gap: 0.9rem;
  }

  .expense-form-title {
    margin: 0;
    color: #2c2f32;
    font-size: 1.2rem;
  }

  .expense-form-grid {
    display: grid;
    gap: 0.8rem;
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  }

  label {
    display: grid;
    gap: 0.4rem;
    color: #2c2f32;
    font-weight: 600;
  }

  input,
  select {
    height: 2.3rem;
    border: 1px solid #abadb0;
    border-radius: 0.55rem;
    background: #eef1f4;
    padding: 0 0.6rem;
    color: #2c2f32;
  }

  .amount-input-wrap {
    display: grid;
    grid-template-columns: auto 1fr;
    gap: 0.35rem;
    align-items: center;
  }

  .currency {
    font-size: 0.75rem;
    font-weight: 800;
    text-transform: uppercase;
    color: #595c5e;
  }

  .spread-toggle {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-weight: 600;
    color: #2c2f32;
  }

  .spread-toggle input {
    width: 1rem;
    height: 1rem;
  }

  .expense-form-actions {
    display: flex;
    justify-content: flex-end;
  }

  button {
    border: none;
    border-radius: 999px;
    min-height: 2.5rem;
    padding: 0 1rem;
    color: #ffffff;
    font-weight: 700;
    cursor: pointer;
    background: linear-gradient(135deg, #9116c4 0%, #d67aff 100%);
  }

  button:disabled {
    cursor: not-allowed;
    opacity: 0.65;
  }

  .expense-error {
    margin: 0;
    color: #b41340;
    font-weight: 600;
  }

  small {
    color: #595c5e;
  }
</style>
