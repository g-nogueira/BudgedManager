<script lang="ts">
  interface Props {
    visible: boolean;
  }

  let { visible }: Props = $props();
  let dismissed = $state(false);

  const dismiss = (): void => {
    dismissed = true;
  };

  const showBanner = $derived(visible && !dismissed);
</script>

{#if showBanner}
  <section class="stale-indicator" role="status" data-testid="stale-banner">
    <p class="message">Budget changed - forecast may be outdated</p>
    <button
      type="button"
      class="dismiss-button"
      onclick={dismiss}
      data-testid="dismiss-stale-banner"
      aria-label="Dismiss stale warning"
    >
      Dismiss
    </button>
  </section>
{/if}

<style>
  .stale-indicator {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.75rem;
    padding: 0.85rem 1rem;
    border-radius: 0.75rem;
    border: 1px solid #f59e0b;
    background: #fffbeb;
  }

  .message {
    margin: 0;
    color: #92400e;
    font-weight: 600;
  }

  .dismiss-button {
    border: 1px solid #f59e0b;
    border-radius: 0.5rem;
    background: #ffffff;
    color: #92400e;
    padding: 0.35rem 0.7rem;
    cursor: pointer;
    font-weight: 600;
  }
</style>
