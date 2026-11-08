import { apiRequest } from '../../../services/apiClient';
import type {
  ActivityDay, CsvPreview, Paged, ProgressResponse, PublicDeck,
  PublicDeckPreview, QuizAnswer, QuizQuestion, QuizResult, StatisticsOverview, Streak,
} from '../types/learning.types';

const json = (method: string, body: unknown): RequestInit => ({ method, body: JSON.stringify(body) });

export const learningApi = {
  quiz: (id: string) => apiRequest<QuizQuestion[]>(`/lessons/${id}/quiz`),
  submit: (id: string, attemptId: string, answers: { questionId: string; answer: QuizAnswer }[]) =>
    apiRequest<QuizResult>(`/lessons/${id}/quiz/attempts`, json('POST', { attemptId, answers })),
  history: (id: string) => apiRequest<QuizResult[]>(`/lessons/${id}/quiz/attempts`),
  progress: () => apiRequest<ProgressResponse>('/me/progress'),
  enroll: (id: string) => apiRequest<void>(`/courses/${id}/enroll`, { method: 'POST' }),
  updateProgress: (id: string, status: string) => apiRequest<void>(`/lessons/${id}/progress`, json('PATCH', { status })),
  overview: (filter: string) => apiRequest<StatisticsOverview>(`/statistics/overview?${filter}`),
  heatmap: (filter: string) => apiRequest<ActivityDay[]>(`/statistics/heatmap?${filter}`),
  streak: () => apiRequest<Streak>('/me/streak'),
  goal: (dailyTarget: number) => apiRequest<void>('/me/goal', json('PATCH', { dailyTarget })),
  previewCsv: (content: string) => apiRequest<CsvPreview>('/csv/preview', json('POST', { content })),
  importCsv: (id: string, content: string) => apiRequest<{ importedCount: number }>(`/decks/${id}/csv/import`, json('POST', { content })),
  publicDecks: (filter: string) => apiRequest<Paged<PublicDeck>>(`/public/decks?${filter}`),
  publicDeck: (id: string) => apiRequest<PublicDeckPreview>(`/public/decks/${id}`),
  clone: (id: string) => apiRequest<{ id: string }>(`/public/decks/${id}/clone`, { method: 'POST' }),
};

export async function downloadCsv(endpoint: string, filename: string) {
  const blob = await apiRequest<Blob>(endpoint, {}, { responseType: 'blob' });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
