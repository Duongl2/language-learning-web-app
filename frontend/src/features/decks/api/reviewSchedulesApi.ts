
import { apiRequest } from '../../../services/apiClient';
import type { OfflineCardSchedule } from '../../../types';

export type DeckScheduleApiResponse = Omit<
  OfflineCardSchedule,
  'deckId'
>;

export async function getDeckSchedules(
  deckId: string
): Promise<DeckScheduleApiResponse[]> {
  return apiRequest<DeckScheduleApiResponse[]>(
    `/reviews/decks/${encodeURIComponent(deckId)}/schedules`
  );
}
