import { apiRequest } from '../../../services/apiClient';
import type { PublicDeck, PublicDeckPreview, Paged } from '../types/community.types';


export const learningApi = {
  publicDecks: (filter: string) => apiRequest<Paged<PublicDeck>>(`/public/decks?${filter}`),
  publicDeck: (id: string) => apiRequest<PublicDeckPreview>(`/public/decks/${id}`),
  clone: (id: string) => apiRequest<{ id: string }>(`/public/decks/${id}/clone`, { method: 'POST' }),
};
