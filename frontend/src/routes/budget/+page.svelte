<script lang="ts">
  import { goto } from '$app/navigation';
  import { budgetError, budgetLoading, storeCreateBudget } from '$lib/stores/budgetStore';

  const now = new Date();
  const defaultYearMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;

  let yearMonth = $state(defaultYearMonth);

  const canSubmit = $derived(yearMonth.trim().length > 0 && !$budgetLoading);

  const handleSubmit = async (event: SubmitEvent): Promise<void> => {
    event.preventDefault();

    if (!canSubmit) {
      return;
    }

    try {
      const createdBudget = await storeCreateBudget(yearMonth);
      await goto(`/budget/${createdBudget.budgetId}`);
    } catch {
      return;
    }
  };
</script>

<section class="budget-create" data-testid="budget-create-page">
  <header class="budget-create-header">
    <p class="eyebrow">Budget setup</p>
    <h1>Start a New Monthly Budget</h1>
    <p class="subtitle">Pick the month you want to plan so we can create your draft budget.</p>
  </header>

  <form class="budget-create-form" onsubmit={handleSubmit}>
    <label for="month-input">Budget month</label>
    <input
      id="month-input"
      data-testid="month-input"
      type="month"
      bind:value={yearMonth}
      required
    />

    {#if $budgetError}
      <p class="form-error" data-testid="budget-create-error">{$budgetError}</p>
    {/if}

    <button type="submit" disabled={!canSubmit} data-testid="create-budget-button">
      {#if $budgetLoading}
        Creating draft...
      {:else}
        Create Budget Draft
      {/if}
    </button>
  </form>
</section>

<style>
  .budget-create {
    max-width: 720px;
    margin: 1rem auto;
    padding: 1.5rem;
    border-radius: 1rem;
    border: 1px solid #d9dde1;
    background: #ffffff;
    display: grid;
    gap: 1.5rem;
  }

  .budget-create-header {
    display: grid;
    gap: 0.6rem;
  }

  .budget-create-header h1 {
    margin: 0;
    font-size: 2rem;
    color: #2c2f32;
  }

  .eyebrow {
    margin: 0;
    font-size: 0.78rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: #595c5e;
    font-weight: 700;
  }

  .subtitle {
    margin: 0;
    color: #595c5e;
  }

  .budget-create-form {
    display: grid;
    gap: 0.75rem;
  }

  .budget-create-form label {
    font-weight: 700;
    color: #2c2f32;
  }

  .budget-create-form input {
    height: 2.75rem;
    padding: 0 0.75rem;
    border-radius: 0.55rem;
    border: 1px solid #abadb0;
    background: #eef1f4;
  }

  .form-error {
    margin: 0;
    color: #b41340;
    font-weight: 600;
  }

  .budget-create-form button {
    justify-self: start;
    min-width: 200px;
    height: 2.9rem;
    padding: 0 1.15rem;
    border: none;
    border-radius: 999px;
    background: linear-gradient(135deg, #9116c4 0%, #d67aff 100%);
    color: #ffffff;
    font-weight: 700;
    cursor: pointer;
  }

  .budget-create-form button:disabled {
    opacity: 0.65;
    cursor: not-allowed;
  }
</style>
