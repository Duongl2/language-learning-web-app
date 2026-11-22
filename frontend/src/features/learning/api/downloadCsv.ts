import { apiRequest } from '../../../services/apiClient';

export async function downloadCsv(endpoint: string, filename: string) {
  const blob = await apiRequest<Blob>(endpoint, {}, { responseType: 'blob' });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
