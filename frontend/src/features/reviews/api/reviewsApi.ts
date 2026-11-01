
import { apiRequest } from '../../../services/apiClient';
import type { DueCardResponse } from '../types/review.types';

export async function getDueCards(
  deckId: string
): Promise<DueCardResponse[]> {
  return apiRequest<DueCardResponse[]>(
    `/reviews/due?deckId=${encodeURIComponent(deckId)}`
  );
}
