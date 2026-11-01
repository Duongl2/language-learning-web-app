import { apiRequest } from '../../../services/apiClient';
import type { UpdateUserProfileRequest, UserProfile } from '../types/profile.types';

export const profileApi = {
  get(): Promise<UserProfile> {
    return apiRequest<UserProfile>('/me');
  },
  update(request: UpdateUserProfileRequest): Promise<UserProfile> {
    return apiRequest<UserProfile>('/me', {
      method: 'PATCH',
      body: JSON.stringify(request),
    });
  },
};
