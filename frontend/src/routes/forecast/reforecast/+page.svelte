<script lang="ts">
  import { page } from '$app/state';
  import ForecastOverlay from '$lib/components/ForecastOverlay.svelte';
  import ReforecastAdjustmentList from '$lib/components/ReforecastAdjustmentList.svelte';
  import {
    allForecasts,
    fetchAllForecasts,
    forecastError,
    forecastLoading,
    reforecastResult,
    submitReforecast
  } from '$lib/stores/forecastStore';
  import { budget, budgetLoading, fetchBudgetById } from '$lib/stores/budgetStore';
  import type { ExpenseAdjustment } from '$lib/types/forecast';

  const budgetId = $derived(page.url.searchParams.get('budgetId'));
  const forecastId = $derived(page.url.searchParams.get('forecastId'));

  const todayDay = new Date().getDate();

  let currentStep = $state<1 | 2 | 3>(1);
  let actualBalance = $state('');
  let startDay = $state(String(todayDay));
  let versionLabel = $state('');
  let adjustments = $state<ExpenseAdjustment[]>([]);
  let step1Error = $state<string | null>(null);
  let step3Submitted = $state(false);

  const parsedStartDay = $derived(Number.parseInt(startDay, 10));

  const futureExpenses = $derived(
    ($budget?.expenses ?? []).filter(
      (e) => e.isSpread || (e.dayOfMonth !== null && e.dayOfMonth >= parsedStartDay)
    )
  );

  const originalForecast = $derived(
    $allForecasts.find((fc) => fc.forecastId === forecastId) ?? $allForecasts[0]
  );

  const autoLabel = $derived(`Re-forecast ${new Date().toLocaleDateString('en-IE', { month: 'short', day: 'numeric' })}`);

  const validateStep1 = (): boolean => {
    step1Error = null;
    const balance = Number.parseFloat(actualBalance);
    if (!Number.isFinite(balance) || balance < 0) {
      step1Error = 'Actual balance must be a number ≥ 0.';
      return false;
    }
    const day = Number.parseInt(startDay, 10);
    if (!Number.isFinite(day) || !Number.isInteger(day) || day < 1 || day > 31) {
      step1Error = 'Day must be between 1 and 31.';
      return false;
    }
    return true;
  };

  const goToStep2 = async (): Promise<void> => {
    if (!validateStep1()) return;
    if (!budgetId) return;
    currentStep = 2;
    await fetchBudgetById(budgetId);
  };

  const goBackToStep1 = (): void => {
    currentStep = 1;
  };

  const handleGenerate = async (): Promise<void> => {
    if (!budgetId || !forecastId) return;
    currentStep = 3;
    step3Submitted = false;
    await submitReforecast(budgetId, forecastId, {
      startDay: parsedStartDay,
      actualBalance: Number.parseFloat(actualBalance),
      versionLabel: versionLabel.trim() || autoLabel,
      expenseAdjustments: adjustments.length > 0 ? adjustments : undefined
    });
    // fetchAllForecasts is called inside submitReforecast
    if (!$forecastError) {
      step3Submitted = true;
      await fetchAllForecasts(budgetId);
    }
  };

  const handleAdjustmentsChange = (updated: ExpenseAdjustment[]): void => {
    adjustments = updated;
  };
</script>

<section class="reforecast-page" data-testid="reforecast-page">
  {#if !budgetId || !forecastId}
    <p class="error" data-testid="missing-params-error">
      Missing budget or forecast context.
      <a href="/budget">Go to Budgets</a>
    </p>
  {:else}
    <header class="page-header">
      <h1>Re-Forecast</h1>
      <div class="step-indicator" data-testid="step-indicator">
        <span class="step" class:step--active={currentStep === 1} data-testid="step-1-indicator">1</span>
        <span class="step-line"></span>
        <span class="step" class:step--active={currentStep === 2} data-testid="step-2-indicator">2</span>
        <span class="step-line"></span>
        <span class="step" class:step--active={currentStep === 3} data-testid="step-3-indicator">3</span>
      </div>
    </header>

    {#if currentStep === 1}
      <div class="wizard-step" data-testid="step-1">
        <h2>Enter Actual Balance</h2>
        <p class="step-description">Tell us your current bank balance and from which day you want to re-forecast.</p>

        <form class="step-form" onsubmit={(e) => { e.preventDefault(); void goToStep2(); }} novalidate>
          {#if step1Error}
            <p class="form-error" data-testid="step1-error">{step1Error}</p>
          {/if}

          <label for="actual-balance">Current bank balance (€)</label>
          <input
            id="actual-balance"
            type="number"
            data-testid="actual-balance-input"
            value={actualBalance}
            oninput={(e) => { actualBalance = (e.currentTarget as HTMLInputElement).value; }}
            min="0"
            step="0.01"
            placeholder="e.g. 3100.00"
          />

          <label for="start-day">Re-forecast from day</label>
          <input
            id="start-day"
            type="number"
            data-testid="start-day-input"
            value={startDay}
            oninput={(e) => { startDay = (e.currentTarget as HTMLInputElement).value; }}
            min="1"
            max="31"
            placeholder="e.g. 15"
          />

          <label for="version-label">Label (optional)</label>
          <input
            id="version-label"
            type="text"
            data-testid="version-label-input"
            bind:value={versionLabel}
            placeholder={autoLabel}
          />

          <div class="step-actions">
            <button
              type="submit"
              class="btn-primary"
              data-testid="next-button"
              disabled={actualBalance.trim().length === 0}
            >
              Next →
            </button>
          </div>
        </form>
      </div>

    {:else if currentStep === 2}
      <div class="wizard-step" data-testid="step-2">
        <h2>Review & Adjust Expenses</h2>
        <p class="step-description">
          Expenses from day {startDay} onward. Modify amounts, remove expenses, or add new ones.
        </p>

        {#if $budgetLoading}
          <p data-testid="budget-loading">Loading expenses…</p>
        {:else}
          <ReforecastAdjustmentList
            expenses={futureExpenses}
            onAdjustmentsChange={handleAdjustmentsChange}
          />
        {/if}

        <div class="step-actions">
          <button type="button" class="btn-secondary" data-testid="back-button" onclick={goBackToStep1}>
            ← Back
          </button>
          <button
            type="button"
            class="btn-primary"
            data-testid="generate-button"
            disabled={$budgetLoading}
            onclick={() => void handleGenerate()}
          >
            Generate Re-Forecast
          </button>
        </div>
      </div>

    {:else}
      <div class="wizard-step" data-testid="step-3">
        <h2>Result</h2>

        {#if $forecastLoading}
          <p data-testid="result-loading">Generating re-forecast…</p>
        {:else if $forecastError}
          <p class="error" data-testid="result-error">{$forecastError}</p>
          <button type="button" class="btn-secondary" onclick={() => { currentStep = 2; }} data-testid="retry-button">
            ← Back
          </button>
        {:else if $reforecastResult}
          <div class="balance-comparison" data-testid="balance-comparison">
            <div class="balance-card">
              <p class="balance-label">Original end-of-month</p>
              <p class="balance-value" data-testid="original-balance">
                €{(originalForecast?.endOfMonthBalance ?? 0).toFixed(2)}
              </p>
            </div>
            <span class="comparison-arrow">→</span>
            <div class="balance-card balance-card--new">
              <p class="balance-label">New end-of-month</p>
              <p
                class="balance-value"
                class:balance-value--negative={$reforecastResult.endOfMonthBalance < 0}
                data-testid="new-balance"
              >
                €{$reforecastResult.endOfMonthBalance.toFixed(2)}
              </p>
            </div>
          </div>

          {#if $allForecasts.length > 1}
            <div class="chart-container" data-testid="overlay-chart">
              <ForecastOverlay forecasts={$allForecasts} />
            </div>
          {/if}

          <div class="result-links" data-testid="result-links">
            <a
              href="/forecast/compare?budgetId={budgetId}"
              data-testid="compare-link"
            >
              View full comparison →
            </a>
            <a
              href="/forecast/{$reforecastResult.forecastId}?budgetId={budgetId}"
              data-testid="new-forecast-link"
            >
              View new forecast →
            </a>
          </div>
        {/if}
      </div>
    {/if}
  {/if}
</section>

<style>
  .reforecast-page {
    max-width: 640px;
    margin: 0 auto;
    padding: 2rem 1rem;
  }

  .page-header {
    display: flex;
    align-items: center;
    gap: 1.5rem;
    margin-bottom: 2rem;
  }

  .page-header h1 {
    margin: 0;
  }

  .step-indicator {
    display: flex;
    align-items: center;
    gap: 0.25rem;
  }

  .step {
    width: 2rem;
    height: 2rem;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.85rem;
    font-weight: 600;
    background: #e5e7eb;
    color: #6b7280;
    border: 2px solid #e5e7eb;
  }

  .step--active {
    background: #2563eb;
    color: white;
    border-color: #2563eb;
  }

  .step-line {
    width: 2rem;
    height: 2px;
    background: #e5e7eb;
  }

  .wizard-step h2 {
    margin-top: 0;
  }

  .step-description {
    color: #6b7280;
    margin-bottom: 1.25rem;
  }

  .step-form {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .step-form label {
    font-weight: 500;
    font-size: 0.9rem;
  }

  .step-form input {
    padding: 0.5rem 0.75rem;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 1rem;
    margin-bottom: 0.5rem;
  }

  .step-actions {
    display: flex;
    gap: 0.75rem;
    margin-top: 1.5rem;
  }

  .btn-primary {
    padding: 0.6rem 1.5rem;
    background: #2563eb;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 1rem;
    font-weight: 500;
  }

  .btn-primary:disabled {
    background: #93c5fd;
    cursor: not-allowed;
  }

  .btn-secondary {
    padding: 0.6rem 1.25rem;
    background: #e5e7eb;
    color: #374151;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 1rem;
  }

  .form-error,
  .error {
    color: #b91c1c;
    font-size: 0.9rem;
    margin-bottom: 0.5rem;
  }

  .balance-comparison {
    display: flex;
    align-items: center;
    gap: 1.5rem;
    margin-bottom: 1.5rem;
    padding: 1.25rem;
    border-radius: 8px;
    background: #f9fafb;
    border: 1px solid #e5e7eb;
  }

  .balance-card {
    flex: 1;
  }

  .balance-card--new {
    background: #eff6ff;
    padding: 0.75rem;
    border-radius: 6px;
  }

  .balance-label {
    font-size: 0.8rem;
    color: #6b7280;
    margin: 0 0 0.25rem;
  }

  .balance-value {
    font-size: 1.4rem;
    font-weight: 700;
    color: #111827;
    margin: 0;
  }

  .balance-value--negative {
    color: #dc2626;
  }

  .comparison-arrow {
    font-size: 1.5rem;
    color: #9ca3af;
  }

  .chart-container {
    margin-bottom: 1.5rem;
  }

  .result-links {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .result-links a {
    color: #2563eb;
    text-decoration: none;
    font-size: 0.95rem;
  }

  .result-links a:hover {
    text-decoration: underline;
  }
</style>

