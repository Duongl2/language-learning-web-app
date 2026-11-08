import { apiRequest } from '../../../services/apiClient';
import type { ProgressResponse } from '../types/progress.types';

const json = (method: string, body: unknown): RequestInit => ({ method, body: JSON.stringify(body) });

export const learningApi = {
  progress: () => apiRequest<ProgressResponse>('/me/progress'),
  enroll: (id: string) => apiRequest<void>(`/courses/${id}/enroll`, { method: 'POST' }),
  updateProgress: (id: string, status: string) => apiRequest<void>(`/lessons/${id}/progress`, json('PATCH', { status })),
};
