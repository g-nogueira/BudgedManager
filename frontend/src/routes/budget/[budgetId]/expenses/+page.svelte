<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/state';
  import { onMount } from 'svelte';

  onMount(async () => {
    const budgetId = page.params.budgetId;

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
