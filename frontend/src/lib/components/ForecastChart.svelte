<script lang="ts">
  import { Chart, type ChartConfiguration, registerables } from 'chart.js';
  import { onDestroy, onMount } from 'svelte';
  import type { DailyEntry } from '$lib/types/forecast';

  Chart.register(...registerables);

  interface Props {
    dailyEntries: DailyEntry[];
  }

  let { dailyEntries }: Props = $props();
  let canvasElement: HTMLCanvasElement | null = null;
  let chartInstance: Chart | null = null;

  onMount(() => {
    if (!canvasElement) {
      return;
    }

    const context = canvasElement.getContext('2d');
    if (!context) {
      return;
    }

    const config: ChartConfiguration<'line'> = {
      type: 'line',
      data: {
        labels: dailyEntries.map((entry) => `Day ${entry.dayNumber}`),
        datasets: [
          {
            label: 'Remaining Balance',
            data: dailyEntries.map((entry) => entry.remainingBalance),
            borderColor: '#2563eb',
            backgroundColor: 'rgba(37, 99, 235, 0.15)',
            fill: true,
            tension: 0.2
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false
      }
    };

    chartInstance = new Chart(context, config);
  });

  onDestroy(() => {
    chartInstance?.destroy();
  });
</script>

<div class="chart-wrapper" data-testid="forecast-chart">
  <canvas bind:this={canvasElement} aria-label="Forecast chart"></canvas>
</div>

<style>
  .chart-wrapper {
    height: 280px;
    width: 100%;
  }
</style>
