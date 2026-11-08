import { apiRequest } from '../../../services/apiClient';
import type { ActivityDay, StatisticsOverview } from '../types/statistics.types';


export const learningApi = {
  overview: (filter: string) => apiRequest<StatisticsOverview>(`/statistics/overview?${filter}`),
  heatmap: (filter: string) => apiRequest<ActivityDay[]>(`/statistics/heatmap?${filter}`),
};
