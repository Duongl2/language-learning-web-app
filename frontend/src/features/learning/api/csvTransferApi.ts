import { apiRequest } from '../../../services/apiClient';

export const learningApi = {
  importCsv: (id: string, content: string) => apiRequest<{ importedCount: number }>(`/decks/${id}/csv/import`, { method: 'POST', body: JSON.stringify({ content }) }),
};
