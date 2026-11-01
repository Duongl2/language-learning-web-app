
import { useQuery } from '@tanstack/react-query';
import { getDueCards } from '../api/reviewsApi';

export function useDueCards(deckId: string) {
  return useQuery({
    queryKey: ['reviews', 'due', deckId],
    queryFn: () => getDueCards(deckId),
    enabled: Boolean(deckId),
  });
}
