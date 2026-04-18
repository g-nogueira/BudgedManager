import { cleanup, fireEvent, render, screen } from '@testing-library/svelte';
import { afterEach, describe, expect, it } from 'vitest';
import StaleIndicator from '$lib/components/StaleIndicator.svelte';

describe('StaleIndicator', () => {
  afterEach(() => {
    cleanup();
  });

  it('renders banner when visible=true', () => {
    render(StaleIndicator, {
      props: {
        visible: true
      }
    });

    expect(screen.getByTestId('stale-banner')).toBeInTheDocument();
  });

  it('hides banner when visible=false', () => {
    render(StaleIndicator, {
      props: {
        visible: false
      }
    });

    expect(screen.queryByTestId('stale-banner')).not.toBeInTheDocument();
  });

  it('dismisses banner on close button click', async () => {
    render(StaleIndicator, {
      props: {
        visible: true
      }
    });

    await fireEvent.click(screen.getByTestId('dismiss-stale-banner'));

    expect(screen.queryByTestId('stale-banner')).not.toBeInTheDocument();
  });

  it('shows correct warning text', () => {
    render(StaleIndicator, {
      props: {
        visible: true
      }
    });

    expect(screen.getByText('Budget changed - forecast may be outdated')).toBeInTheDocument();
  });
});
