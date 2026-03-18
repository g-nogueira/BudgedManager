<script lang="ts">
  import favicon from '$lib/assets/favicon.svg';
  import '../app.css';
  import { clearTokens } from '$lib/stores/authStore';

  let { children } = $props();

  const logout = (): void => {
    clearTokens();
  };
</script>

<svelte:head>
  <link rel="icon" href={favicon} />
  <title>MonthlyBudget</title>
</svelte:head>

<div class="app-shell">
  <nav class="nav-bar">
    <a href="/">Dashboard</a>
    <a href="/budget">Budget</a>
    <a href="/forecast">Forecast</a>
    <a href="/household">Household</a>
    <button type="button" onclick={logout}>Logout</button>
  </nav>

  <main class="content">
    {@render children()}
  </main>
</div>
