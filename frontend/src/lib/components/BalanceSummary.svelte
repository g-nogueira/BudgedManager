<script lang="ts">
  interface Props {
    endOfMonthBalance: number;
    todayBalance: number | null;
    todayDate: string;
    yearMonth: string;
  }

  let { endOfMonthBalance, todayBalance, todayDate, yearMonth }: Props = $props();

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('en-IE', {
      style: 'currency',
      currency: 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  };

  const formatBudgetMonth = (value: string): string => {
    const [yearToken, monthToken] = value.split('-');
    const year = Number.parseInt(yearToken ?? '', 10);
    const month = Number.parseInt(monthToken ?? '', 10);

    if (Number.isNaN(year) || Number.isNaN(month) || month < 1 || month > 12) {
      return value;
    }

    return new Intl.DateTimeFormat('en-US', {
      month: 'long',
      year: 'numeric',
      timeZone: 'UTC'
    }).format(new Date(Date.UTC(year, month - 1, 1)));
  };

  const isNegative = $derived(endOfMonthBalance < 0);
</script>

<section
  class="balance-summary"
  class:balance-summary-warning={isNegative}
  data-testid="balance-summary"
>
  <p class="budget-month" data-testid="budget-month-label">
    Budget month: {formatBudgetMonth(yearMonth)}
  </p>

  <div class="end-of-month">
    <p class="label">End of month balance</p>
    <p class="end-value" data-testid="end-of-month-balance">{formatCurrency(endOfMonthBalance)}</p>

    {#if isNegative}
      <p class="warning-message" data-testid="negative-warning">
        Warning: you may finish this month below zero.
      </p>
    {/if}
  </div>

  <div class="today">
    <p class="label" data-testid="today-date">{todayDate}</p>
    <p class="today-value" data-testid="today-balance">
      {#if todayBalance === null}
        N/A
      {:else}
        {formatCurrency(todayBalance)}
      {/if}
    </p>
  </div>
</section>

<style>
  .balance-summary {
    display: grid;
    gap: 1rem;
    padding: 1.25rem;
    border-radius: 0.75rem;
    border: 1px solid #cbd5e1;
    background: #ffffff;
  }

  .budget-month {
    margin: 0;
    color: #475569;
  }

  .end-of-month {
    display: grid;
    gap: 0.35rem;
  }

  .today {
    display: grid;
    gap: 0.35rem;
    padding-top: 0.75rem;
    border-top: 1px solid #e2e8f0;
  }

  .label {
    margin: 0;
    color: #334155;
    font-weight: 600;
  }

  .end-value {
    margin: 0;
    font-size: 2.5rem;
    line-height: 1.1;
    font-weight: 800;
    color: #0f172a;
  }

  .today-value {
    margin: 0;
    font-size: 1.25rem;
    font-weight: 700;
    color: #0f172a;
  }

  .warning-message {
    margin: 0;
    color: #991b1b;
    font-weight: 600;
  }

  .balance-summary-warning {
    border-color: #fca5a5;
    background: #fef2f2;
  }

  .balance-summary-warning .end-value,
  .balance-summary-warning .today-value {
    color: #991b1b;
  }
</style>
