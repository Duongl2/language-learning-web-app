
import { apiRequest } from '../../../services/apiClient';
import type { Flashcard } from '../../../types';

export async function getDeckCards(
  deckId: string
): Promise<Flashcard[]> {
  return apiRequest<Flashcard[]>(
    `/decks/${encodeURIComponent(deckId)}/cards`
  );
}
