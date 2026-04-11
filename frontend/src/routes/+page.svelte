<script lang="ts">
  import { onMount } from 'svelte';
  import { get } from 'svelte/store';
  import BalanceSummary from '$lib/components/BalanceSummary.svelte';
  import ForecastChart from '$lib/components/ForecastChart.svelte';
  import StaleIndicator from '$lib/components/StaleIndicator.svelte';
  import { budget, budgetError, budgetLoading, fetchBudgetByMonth } from '$lib/stores/budgetStore';
  import {
    forecast,
    forecastError,
    forecastLoading,
    forecasts,
    fetchForecast,
    fetchForecasts
  } from '$lib/stores/forecastStore';
  import type { ForecastSummary } from '$lib/types/forecast';

  const now = new Date();
  const currentYearMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
  const formattedToday = new Intl.DateTimeFormat('en-US', {
    month: 'long',
    day: 'numeric',
    year: 'numeric'
  }).format(now);

  const selectLatestForecast = (items: ForecastSummary[]): ForecastSummary | null => {
    const nonSnapshotForecasts = items.filter((item) => !item.isSnapshot);

    if (nonSnapshotForecasts.length === 0) {
      return null;
    }

    return [...nonSnapshotForecasts].sort((a, b) => {
      const dateA = new Date(a.createdAt).getTime();
      const dateB = new Date(b.createdAt).getTime();
      return dateB - dateA;
    })[0];
  };

  const todayBalance = $derived.by(() => {
    const activeForecast = $forecast;

    if (!activeForecast) {
      return null;
    }

    const entryForToday = activeForecast.dailyEntries.find(
      (entry) => entry.dayNumber === now.getDate()
    );
    return entryForToday?.remainingBalance ?? null;
  });

  const showStaleIndicator = $derived(Boolean($forecast?.isStale));

  onMount(async () => {
    await fetchBudgetByMonth(currentYearMonth);

    const currentBudget = get(budget);
    if (!currentBudget?.budgetId) {
      return;
    }

    await fetchForecasts(currentBudget.budgetId);

    const latestForecast = selectLatestForecast(get(forecasts));
    if (!latestForecast) {
      forecast.set(null);
      return;
    }

    await fetchForecast(currentBudget.budgetId, latestForecast.forecastId);
  });
</script>

{#if $budgetLoading}
  <section class="placeholder-state" data-testid="budget-loading">
    <h1>Your month at a glance</h1>
    <p>Loading your budget summary...</p>
  </section>
{:else if $budgetError}
  <section class="placeholder-state" data-testid="budget-error">
    <h1>Your month at a glance</h1>
    <p>We could not load your dashboard right now. {$budgetError}</p>
  </section>
{:else if !$budget}
  <section class="placeholder-state" data-testid="no-budget">
    <h1>Your month at a glance</h1>
    <p>No budget for this month yet. Create one to see your forecast and daily balance.</p>
  </section>
{:else}
  <section class="dashboard" data-testid="dashboard">
    <h1>Your month at a glance</h1>
    <p class="intro">
      Understand your month in one place with your latest forecast and current balance.
    </p>

    <BalanceSummary
      yearMonth={$budget.yearMonth}
      endOfMonthBalance={$forecast?.endOfMonthBalance ?? 0}
      {todayBalance}
      todayDate={formattedToday}
    />

    <StaleIndicator visible={showStaleIndicator} />

    <section class="forecast-card">
      <h2>Forecast trend</h2>

      {#if $forecastLoading}
        <p data-testid="forecast-loading">Loading your forecast...</p>
      {:else if $forecastError}
        <p data-testid="forecast-error">
          We could not load your forecast right now. {$forecastError}
        </p>
      {:else if !$forecast || $forecasts.length === 0}
        <p data-testid="no-forecast">
          No forecast generated yet. Generate one to see your balance trajectory.
        </p>
      {:else}
        <ForecastChart dailyEntries={$forecast.dailyEntries} />
      {/if}
    </section>
  </section>
{/if}

<style>
  .dashboard {
    display: grid;
    gap: 1rem;
  }

  h1 {
    margin: 0;
  }

  .intro {
    margin: 0;
    color: #334155;
  }

  .forecast-card {
    display: grid;
    gap: 0.75rem;
    background: #ffffff;
    border: 1px solid #cbd5e1;
    border-radius: 0.75rem;
    padding: 1rem;
  }

  .forecast-card h2 {
    margin: 0;
  }

  .forecast-card p {
    margin: 0;
  }
</style>
