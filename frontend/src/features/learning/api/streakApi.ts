import { apiRequest } from '../../../services/apiClient';
import type { Streak } from '../types/streak.types';

const json = (method: string, body: unknown): RequestInit => ({ method, body: JSON.stringify(body) });

export const learningApi = {
  streak: () => apiRequest<Streak>('/me/streak'),
  goal: (dailyTarget: number) => apiRequest<void>('/me/goal', json('PATCH', { dailyTarget })),
};
