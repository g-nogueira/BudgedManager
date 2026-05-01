<script lang="ts">
  import { page } from '$app/state';
  import ForecastChart from '$lib/components/ForecastChart.svelte';
  import ForecastOverlay from '$lib/components/ForecastOverlay.svelte';
  import {
    allForecasts,
    fetchAllForecasts,
    forecastError,
    forecastLoading
  } from '$lib/stores/forecastStore';

  const forecastId = $derived(page.params.forecastId);
  const budgetId = $derived(page.url.searchParams.get('budgetId'));

  let hasFetched = $state(false);
  const currentForecast = $derived(
    $allForecasts.find((fc) => fc.forecastId === forecastId) ?? $allForecasts[0]
  );

  $effect(() => {
    if (!budgetId) return;
    hasFetched = false;
    void fetchAllForecasts(budgetId).then(() => {
      hasFetched = true;
    });
  });
</script>

{#if !budgetId}
  <section data-testid="forecast-detail-page">
    <p class="error" data-testid="missing-budget-error">
      Missing budget context. <a href="/budget">Go to Budgets</a>
    </p>
  </section>
{:else if $forecastLoading}
  <section data-testid="forecast-detail-page">
    <p data-testid="loading-state">Loading forecast…</p>
  </section>
{:else if $forecastError}
  <section data-testid="forecast-detail-page">
    <p class="error" data-testid="error-state">{$forecastError}</p>
  </section>
{:else if hasFetched && $allForecasts.length === 0}
  <section data-testid="forecast-detail-page">
    <p data-testid="empty-state">No forecasts found for this budget.</p>
  </section>
{:else}
  <section data-testid="forecast-detail-page">
    <header class="page-header">
      <h1>Forecast</h1>
      <div class="version-pills" data-testid="version-pills">
        {#each $allForecasts as fc (fc.forecastId)}
          <span
            class="pill"
            class:pill--active={fc.forecastId === forecastId}
            data-testid="version-pill"
          >
            {fc.versionLabel}
          </span>
        {/each}
      </div>
    </header>

    <div class="chart-container">
      {#if $allForecasts.length > 1}
        <ForecastOverlay forecasts={$allForecasts} />
      {:else}
        <ForecastChart dailyEntries={currentForecast?.dailyEntries ?? []} />
      {/if}
    </div>

    <div class="action-links">
      <a
        href="/forecast/reforecast?budgetId={budgetId}&forecastId={forecastId}"
        data-testid="reforecast-link"
      >
        Re-Forecast
      </a>
      <a href="/forecast/compare?budgetId={budgetId}" data-testid="compare-link">Compare</a>
    </div>
  </section>
{/if}

<style>
  .page-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 1rem;
  }

  .version-pills {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  .pill {
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 600;
    background-color: #e5e8ec;
    color: #595c5e;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .pill--active {
    background: linear-gradient(135deg, #9116c4, #d67aff);
    color: #ffffff;
  }

  .chart-container {
    margin-bottom: 1.5rem;
  }

  .action-links {
    display: flex;
    gap: 1rem;
  }

  .action-links a {
    color: #9116c4;
    text-decoration: none;
    font-weight: 600;
  }

  .error {
    color: #b41340;
  }
</style>
