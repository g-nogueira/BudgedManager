export type MemberRole = 'OWNER' | 'PARTNER';

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export interface RegisterResponse {
  userId: string;
}

export interface Household {
  householdId: string;
  name: string;
  members: HouseholdMember[];
}

export interface HouseholdMember {
  userId: string;
  role: string;
}

export interface CreateHouseholdResult {
  householdId: string;
}

export interface InviteMemberResult {
  invitationId: string;
}

export interface JoinHouseholdResult {
  householdId: string;
}

export interface RegisterRequest {
  email: string;
  displayName: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface CreateHouseholdRequest {
  name: string;
}

export interface InviteRequest {
  email: string;
}

export interface JoinRequest {
  token: string;
}
