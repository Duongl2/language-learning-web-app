import { apiRequest } from '../../../services/apiClient';
import type { CsvPreview } from '../types/csvPreview.types';

const json = (method: string, body: unknown): RequestInit => ({ method, body: JSON.stringify(body) });

export const learningApi = {
  previewCsv: (content: string) => apiRequest<CsvPreview>('/csv/preview', json('POST', { content })),
};
