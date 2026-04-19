<script lang="ts">
  import type { AddIncomeRequest, IncomeSource, UpdateIncomeRequest } from '$lib/types/budget';

  interface Props {
    incomes: IncomeSource[];
    totalIncome: number;
    onAdd: (request: AddIncomeRequest) => Promise<void> | void;
    onUpdate: (incomeId: string, request: UpdateIncomeRequest) => Promise<void> | void;
    onRemove: (incomeId: string) => Promise<void> | void;
  }

  let { incomes, totalIncome, onAdd, onUpdate, onRemove }: Props = $props();

  let isAdding = $state(false);
  let addName = $state('');
  let addAmount = $state('');
  let addError = $state<string | null>(null);

  let editingIncomeId = $state<string | null>(null);
  let editName = $state('');
  let editAmount = $state('');
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

  const validateIncome = (name: string, amountInput: string): string | null => {
    if (name.trim().length === 0) {
      return 'Name is required.';
    }

    const parsedAmount = Number.parseFloat(amountInput);

    if (Number.isNaN(parsedAmount) || parsedAmount <= 0) {
      return 'Amount must be greater than zero.';
    }

    return null;
  };

  const toErrorMessage = (error: unknown): string => {
    return error instanceof Error ? error.message : 'Something went wrong.';
  };

  const startAdd = (): void => {
    isAdding = true;
    addName = '';
    addAmount = '';
    addError = null;
  };

  const cancelAdd = (): void => {
    isAdding = false;
    addError = null;
  };

  const handleAdd = async (): Promise<void> => {
    addError = null;
    actionError = null;

    const validationError = validateIncome(addName, addAmount);
    if (validationError) {
      addError = validationError;
      return;
    }

    isSaving = true;

    try {
      await onAdd({
        name: addName.trim(),
        amount: Number.parseFloat(addAmount)
      });

      isAdding = false;
      addName = '';
      addAmount = '';
    } catch (error) {
      addError = toErrorMessage(error);
    } finally {
      isSaving = false;
    }
  };

  const startEdit = (income: IncomeSource): void => {
    editingIncomeId = income.incomeId;
    editName = income.name;
    editAmount = String(income.amount);
    editError = null;
    actionError = null;
  };

  const cancelEdit = (): void => {
    editingIncomeId = null;
    editName = '';
    editAmount = '';
    editError = null;
  };

  const handleUpdate = async (incomeId: string): Promise<void> => {
    editError = null;
    actionError = null;

    const validationError = validateIncome(editName, editAmount);
    if (validationError) {
      editError = validationError;
      return;
    }

    isSaving = true;

    try {
      await onUpdate(incomeId, {
        name: editName.trim(),
        amount: Number.parseFloat(editAmount)
      });

      cancelEdit();
    } catch (error) {
      editError = toErrorMessage(error);
    } finally {
      isSaving = false;
    }
  };

  const handleRemove = async (incomeId: string): Promise<void> => {
    actionError = null;
    isSaving = true;

    try {
      await onRemove(incomeId);
    } catch (error) {
      actionError = toErrorMessage(error);
    } finally {
      isSaving = false;
    }
  };
</script>

<section class="income-section" data-testid="income-section">
  <div class="income-section-header">
    <div>
      <h2 data-testid="income-section-heading">Monthly Income</h2>
      <p>Tracking your monthly inflows</p>
    </div>

    <div class="income-total">
      <span>Total Inflow</span>
      <strong data-testid="income-total">{formatCurrency(totalIncome)}</strong>
    </div>
  </div>

  {#if actionError}
    <p class="income-error">{actionError}</p>
  {/if}

  <div class="income-grid">
    {#if incomes.length === 0}
      <p class="income-empty">No income added yet.</p>
    {/if}

    {#each incomes as income (income.incomeId)}
      {#if editingIncomeId === income.incomeId}
        <article
          class="income-card income-card-edit"
          data-testid={`income-edit-${income.incomeId}`}
        >
          <label>
            Name
            <input bind:value={editName} data-testid={`income-edit-name-${income.incomeId}`} />
          </label>

          <label>
            Amount
            <input
              bind:value={editAmount}
              type="number"
              min="0"
              step="0.01"
              data-testid={`income-edit-amount-${income.incomeId}`}
            />
          </label>

          {#if editError}
            <p class="income-error">{editError}</p>
          {/if}

          <div class="income-card-actions">
            <button type="button" onclick={() => handleUpdate(income.incomeId)} disabled={isSaving}>
              Save
            </button>
            <button type="button" class="secondary" onclick={cancelEdit} disabled={isSaving}>
              Cancel
            </button>
          </div>
        </article>
      {:else}
        <article class="income-card" data-testid={`income-card-${income.incomeId}`}>
          <div class="income-card-main">
            <div class="income-icon" aria-hidden="true">EUR</div>
            <p class="income-name">{income.name}</p>
          </div>

          <div class="income-card-side">
            <p class="income-amount">{formatCurrency(income.amount)}</p>
            <div class="income-card-actions">
              <button
                type="button"
                class="secondary"
                onclick={() => startEdit(income)}
                data-testid={`edit-income-${income.incomeId}`}
                disabled={isSaving}
              >
                Edit
              </button>
              <button
                type="button"
                class="secondary danger"
                onclick={() => handleRemove(income.incomeId)}
                data-testid={`remove-income-${income.incomeId}`}
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

  {#if !isAdding}
    <button
      type="button"
      class="add-income-button"
      onclick={startAdd}
      data-testid="add-income-button"
    >
      Add Income
    </button>
  {:else}
    <article class="add-income-form" data-testid="add-income-form">
      <label>
        Name
        <input bind:value={addName} data-testid="add-income-name" />
      </label>

      <label>
        Amount
        <input
          bind:value={addAmount}
          type="number"
          min="0"
          step="0.01"
          data-testid="add-income-amount"
        />
      </label>

      {#if addError}
        <p class="income-error" data-testid="add-income-error">{addError}</p>
      {/if}

      <div class="income-card-actions">
        <button
          type="button"
          onclick={handleAdd}
          disabled={isSaving}
          data-testid="save-income-button"
        >
          Save
        </button>
        <button type="button" class="secondary" onclick={cancelAdd} disabled={isSaving}>
          Cancel
        </button>
      </div>
    </article>
  {/if}
</section>

<style>
  .income-section {
    display: grid;
    gap: 1rem;
  }

  .income-section-header {
    display: flex;
    justify-content: space-between;
    gap: 1rem;
    align-items: flex-end;
  }

  .income-section-header h2 {
    margin: 0;
    color: #2c2f32;
  }

  .income-section-header p {
    margin: 0.4rem 0 0;
    color: #595c5e;
  }

  .income-total {
    text-align: right;
    display: grid;
    gap: 0.25rem;
  }

  .income-total span {
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: #595c5e;
    font-weight: 700;
  }

  .income-total strong {
    font-size: 1.65rem;
    color: #2c2f32;
  }

  .income-grid {
    display: grid;
    gap: 0.75rem;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  }

  .income-card {
    display: flex;
    justify-content: space-between;
    gap: 1rem;
    border: 1px solid #d9dde1;
    border-radius: 0.85rem;
    background: #ffffff;
    padding: 1rem;
  }

  .income-card-edit {
    flex-direction: column;
  }

  .income-card-edit label,
  .add-income-form label {
    display: grid;
    gap: 0.35rem;
    font-weight: 600;
    color: #2c2f32;
  }

  .income-card-edit input,
  .add-income-form input {
    height: 2.35rem;
    border: 1px solid #abadb0;
    border-radius: 0.55rem;
    background: #eef1f4;
    padding: 0 0.65rem;
  }

  .income-card-main {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }

  .income-icon {
    width: 2rem;
    height: 2rem;
    border-radius: 50%;
    background: #f2ddff;
    color: #9116c4;
    display: inline-flex;
    justify-content: center;
    align-items: center;
    font-size: 0.6rem;
    font-weight: 800;
  }

  .income-name,
  .income-amount {
    margin: 0;
    color: #2c2f32;
    font-weight: 700;
  }

  .income-card-side {
    display: grid;
    justify-items: end;
    gap: 0.45rem;
  }

  .income-card-actions {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  button {
    border: none;
    background: linear-gradient(135deg, #9116c4 0%, #d67aff 100%);
    color: #ffffff;
    border-radius: 999px;
    min-height: 2rem;
    padding: 0 0.9rem;
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

  .add-income-button {
    border: 2px dashed #abadb0;
    background: transparent;
    color: #595c5e;
    min-height: 2.7rem;
  }

  .add-income-form {
    border: 1px solid #d9dde1;
    border-radius: 0.85rem;
    background: #ffffff;
    padding: 1rem;
    display: grid;
    gap: 0.75rem;
  }

  .income-error {
    margin: 0;
    color: #b41340;
    font-weight: 600;
  }

  .income-empty {
    margin: 0;
    padding: 0.85rem;
    border-radius: 0.65rem;
    border: 1px solid #d9dde1;
    background: #ffffff;
    color: #595c5e;
    grid-column: 1 / -1;
  }

  button:disabled {
    cursor: not-allowed;
    opacity: 0.6;
  }
</style>
