<script lang="ts">
  import { Chart, type ChartConfiguration, type Plugin, registerables } from 'chart.js';
  import { onDestroy, onMount } from 'svelte';
  import type { DailyEntry } from '$lib/types/forecast';

  Chart.register(...registerables);

  interface Props {
    dailyEntries: DailyEntry[];
  }

  let { dailyEntries }: Props = $props();
  let canvasElement: HTMLCanvasElement | null = null;
  let chartInstance: Chart | null = null;

  const zeroLinePlugin: Plugin<'line'> = {
    id: 'zeroLine',
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

  onMount(() => {
    if (!canvasElement) {
      return;
    }

    const context = canvasElement.getContext('2d');
    if (!context) {
      return;
    }

    const gradient = context.createLinearGradient(0, 0, 0, 280);
    gradient.addColorStop(0, 'rgba(145, 22, 196, 0.35)');
    gradient.addColorStop(1, 'rgba(145, 22, 196, 0)');

    const config: ChartConfiguration<'line'> = {
      type: 'line',
      data: {
        labels: dailyEntries.map((entry) => `Day ${entry.dayNumber}`),
        datasets: [
          {
            label: 'Remaining Balance',
            data: dailyEntries.map((entry) => entry.remainingBalance),
            borderColor: '#9116c4',
            backgroundColor: gradient,
            fill: true,
            tension: 0.2,
            pointRadius: 2,
            segment: {
              borderColor: (ctx) =>
                (ctx.p1.parsed.y ?? 0) < 0 ? 'rgba(239, 68, 68, 0.9)' : '#9116c4',
              backgroundColor: (ctx) =>
                (ctx.p1.parsed.y ?? 0) < 0 ? 'rgba(239, 68, 68, 0.15)' : gradient
            }
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          tooltip: {
            callbacks: {
              label(tooltipItem) {
                const entry = dailyEntries[tooltipItem.dataIndex];
                const lines: string[] = [`Balance: €${(tooltipItem.parsed.y ?? 0).toFixed(2)}`];
                if (entry?.breakdown?.length) {
                  for (const item of entry.breakdown) {
                    lines.push(`  ${item.name}: €${item.amount.toFixed(2)}`);
                  }
                }
                return lines;
              }
            }
          }
        },
        scales: {
          x: {
            ticks: { font: { size: 11 } }
          },
          y: {
            ticks: { font: { size: 11 } }
          }
        }
      },
      plugins: [zeroLinePlugin]
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
