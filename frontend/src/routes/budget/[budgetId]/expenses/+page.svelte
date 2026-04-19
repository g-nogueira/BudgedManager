<script lang="ts">
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';

  onMount(async () => {
    if (typeof window === 'undefined') {
      return;
    }

    const match = window.location.pathname.match(/\/budget\/([^/]+)\//);
    const budgetId = match?.[1];

    if (budgetId) {
      await goto(`/budget/${budgetId}`, { replaceState: true });
    }
  });
</script>

<p class="redirect-message">Redirecting to budget overview...</p>

<style>
  .redirect-message {
    margin: 1rem;
    color: #595c5e;
  }
</style>
