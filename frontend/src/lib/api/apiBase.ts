import type { ApiError } from '$lib/types/api';

const ENV_BASE = import.meta.env.VITE_API_BASE_URL as string | undefined;

export const API_BASE = ENV_BASE && ENV_BASE.trim().length > 0 ? ENV_BASE.trim() : '';

export const mapApiError = async (response: Response): Promise<Error> => {
  const fallback = `HTTP ${response.status}`;
  try {
    const body = (await response.json()) as Partial<ApiError>;
    if (body.error) {
      return new Error(body.error);
    }
    return new Error(fallback);
  } catch {
    return new Error(fallback);
  }
};

export const handleResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    throw await mapApiError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
};
