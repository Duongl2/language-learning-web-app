import { apiRequest } from '../../../services/apiClient';
import type { QuizQuestion, QuizAnswer, QuizResult } from '../types/quiz.types';

const json = (method: string, body: unknown): RequestInit => ({ method, body: JSON.stringify(body) });

export const learningApi = {
  quiz: (id: string) => apiRequest<QuizQuestion[]>(`/lessons/${id}/quiz`),
  submit: (id: string, attemptId: string, answers: { questionId: string; answer: QuizAnswer }[]) =>
    apiRequest<QuizResult>(`/lessons/${id}/quiz/attempts`, json('POST', { attemptId, answers })),
  history: (id: string) => apiRequest<QuizResult[]>(`/lessons/${id}/quiz/attempts`),
};
