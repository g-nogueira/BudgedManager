import { derived, writable } from 'svelte/store';

const ACCESS_TOKEN_KEY = 'mb_access_token';
const REFRESH_TOKEN_KEY = 'mb_refresh_token';

const readToken = (key: string): string | null => {
  if (typeof window === 'undefined') {
    return null;
  }
  return window.localStorage.getItem(key);
};

export const accessToken = writable<string | null>(readToken(ACCESS_TOKEN_KEY));
export const refreshToken = writable<string | null>(readToken(REFRESH_TOKEN_KEY));

export const isAuthenticated = derived(accessToken, ($accessToken) => Boolean($accessToken));

export const setTokens = (nextAccessToken: string, nextRefreshToken: string): void => {
  accessToken.set(nextAccessToken);
  refreshToken.set(nextRefreshToken);

  if (typeof window !== 'undefined') {
    window.localStorage.setItem(ACCESS_TOKEN_KEY, nextAccessToken);
    window.localStorage.setItem(REFRESH_TOKEN_KEY, nextRefreshToken);
  }
};

export const clearTokens = (): void => {
  accessToken.set(null);
  refreshToken.set(null);

  if (typeof window !== 'undefined') {
    window.localStorage.removeItem(ACCESS_TOKEN_KEY);
    window.localStorage.removeItem(REFRESH_TOKEN_KEY);
  }
};

export const getAuthHeader = (): Record<string, string> => {
  const token = readToken(ACCESS_TOKEN_KEY);
  return token ? { Authorization: `Bearer ${token}` } : {};
};
