import { getAuthHeader, setTokens } from '$lib/stores/authStore';
import { API_BASE, handleResponse } from '$lib/api/apiBase';
import type {
  CreateHouseholdRequest,
  CreateHouseholdResult,
  Household,
  InviteMemberResult,
  InviteRequest,
  JoinHouseholdResult,
  JoinRequest,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse
} from '$lib/types/auth';

export const register = async (request: RegisterRequest): Promise<RegisterResponse> => {
  const response = await fetch(`${API_BASE}/api/v1/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request)
  });

  return handleResponse<RegisterResponse>(response);
};

export const login = async (request: LoginRequest): Promise<LoginResponse> => {
  const response = await fetch(`${API_BASE}/api/v1/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request)
  });

  const result = await handleResponse<LoginResponse>(response);
  setTokens(result.accessToken, result.refreshToken);
  return result;
};

export const createHousehold = async (
  request: CreateHouseholdRequest
): Promise<CreateHouseholdResult> => {
  const response = await fetch(`${API_BASE}/api/v1/households`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<CreateHouseholdResult>(response);
};

export const getHousehold = async (householdId: string): Promise<Household> => {
  const response = await fetch(`${API_BASE}/api/v1/households/${householdId}`, {
    headers: { ...getAuthHeader() }
  });

  return handleResponse<Household>(response);
};

export const inviteMember = async (
  householdId: string,
  request: InviteRequest
): Promise<InviteMemberResult> => {
  const response = await fetch(`${API_BASE}/api/v1/households/${householdId}/invite`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<InviteMemberResult>(response);
};

export const joinHousehold = async (request: JoinRequest): Promise<JoinHouseholdResult> => {
  const response = await fetch(`${API_BASE}/api/v1/households/join`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    body: JSON.stringify(request)
  });

  return handleResponse<JoinHouseholdResult>(response);
};
