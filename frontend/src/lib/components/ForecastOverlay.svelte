<script lang="ts">
  import { Chart, type ChartConfiguration, type Plugin, registerables } from 'chart.js';
  import { onDestroy, onMount } from 'svelte';
  import type { Forecast } from '$lib/types/forecast';

  Chart.register(...registerables);

  interface Props {
    forecasts: Forecast[];
  }

  let { forecasts }: Props = $props();
  let canvasElement: HTMLCanvasElement | null = null;
  let chartInstance: Chart | null = null;

  const COLORS = ['#9116c4', '#35dfab', '#f59e0b', '#ef4444', '#3b82f6'];

  const zeroLinePlugin: Plugin<'line'> = {
    id: 'overlayZeroLine',
    afterDraw(chart) {
      const { ctx, chartArea, scales } = chart;
      if (!scales['y']) return;
      const yZero = scales['y'].getPixelForValue(0);
      if (yZero < chartArea.top || yZero > chartArea.bottom) return;
      ctx.save();
      ctx.beginPath();
      ctx.setLineDash([6, 4]);
      ctx.strokeStyle = 'rgba(239, 68, 68, 0.6)';
      ctx.lineWidth = 1;
      ctx.moveTo(chartArea.left, yZero);
      ctx.lineTo(chartArea.right, yZero);
      ctx.stroke();
      ctx.restore();
    }
  };

  function buildReforecastMarkerPlugin(
    forecastList: Forecast[],
    axisEntries: { dayNumber: number }[]
  ): Plugin<'line'> {
    return {
      id: 'reforecastMarker',
      afterDraw(chart) {
        const { ctx, chartArea, scales } = chart;
        if (!scales['x']) return;

        for (const fc of forecastList) {
          if (fc.forecastType !== 'REFORECAST' || fc.startDay <= 0) continue;

          const labelIndex = axisEntries.findIndex((e) => e.dayNumber >= fc.startDay);
          if (labelIndex < 0) continue;

          const xPos = scales['x'].getPixelForValue(labelIndex);
          if (xPos < chartArea.left || xPos > chartArea.right) continue;

          ctx.save();
          ctx.beginPath();
          ctx.setLineDash([5, 5]);
          ctx.strokeStyle = 'rgba(100, 100, 100, 0.7)';
          ctx.lineWidth = 1.5;
          ctx.moveTo(xPos, chartArea.top);
          ctx.lineTo(xPos, chartArea.bottom);
          ctx.stroke();
          ctx.restore();
        }
      }
    };
  }

  onMount(() => {
    if (!canvasElement || forecasts.length === 0) return;

    const context = canvasElement.getContext('2d');
    if (!context) return;

    const datasets = forecasts.map((fc, i) => ({
      label: fc.versionLabel,
      data: fc.dailyEntries.map((e) => e.remainingBalance),
      borderColor: COLORS[i % COLORS.length],
      backgroundColor: 'transparent',
      fill: false,
      tension: 0.2,
      pointRadius: 2
    }));

    // Use labels from the forecast with most entries for x-axis
    const longestForecast = forecasts.reduce((a, b) =>
      a.dailyEntries.length >= b.dailyEntries.length ? a : b
    );

    const config: ChartConfiguration<'line'> = {
      type: 'line',
      data: {
        labels: longestForecast.dailyEntries.map((e) => `Day ${e.dayNumber}`),
        datasets
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            position: 'top'
          },
          tooltip: {
            callbacks: {
              label(tooltipItem) {
                return `${tooltipItem.dataset.label}: €${(tooltipItem.parsed.y ?? 0).toFixed(2)}`;
              }
            }
          }
        },
        scales: {
          x: { ticks: { font: { size: 11 } } },
          y: { ticks: { font: { size: 11 } } }
        }
      },
      plugins: [zeroLinePlugin, buildReforecastMarkerPlugin(forecasts, longestForecast.dailyEntries)]
    };

    chartInstance = new Chart(context, config);
  });

  onDestroy(() => {
    chartInstance?.destroy();
  });
</script>

<div class="chart-wrapper" data-testid="forecast-overlay">
  <canvas bind:this={canvasElement} aria-label="Forecast overlay chart"></canvas>
</div>

<style>
  .chart-wrapper {
    height: 320px;
    width: 100%;
  }
</style>
