export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  timezone: string;
  role: string;
}

export interface UpdateUserProfileRequest {
  displayName?: string;
  timezone?: string;
}
