
import { useQuery } from '@tanstack/react-query';
import { getDueCards } from '../api/reviewsApi';
import { ApiError } from '../../../services/apiClient';
import { db } from '../../../offline/db';

import type { DueCardResponse } from '../types/review.types';

async function getPendingCardIds(): Promise<Set<string>> {
  const operations = await db.syncQueue
    .where('status')
    .anyOf('Pending', 'Failed')
    .toArray();

  return new Set(
    operations
      .filter((operation) => operation.type === 'Review')
      .map((operation) => operation.entityId)
  );
}

function parseDueAt(dueAt: string): number {
  const hasTimezone = /(?:Z|[+-]\d{2}:\d{2})$/i.test(dueAt);

  return Date.parse(
    hasTimezone ? dueAt : `${dueAt}Z`
  );
}

async function getOfflineDueCards(
  deckId: string,
  pendingCardIds: Set<string>
): Promise<DueCardResponse[]> {
  const deck = await db.decks.get(deckId);

  if (!deck) {
    throw new Error(
      'Bộ thẻ chưa được tải về để học offline.'
    );
  }

  const [cards, schedules] = await Promise.all([
    db.cards.where('deckId').equals(deckId).toArray(),
    db.cardSchedules.where('deckId').equals(deckId).toArray(),
  ]);

  const cardMap = new Map(
    cards.map((card) => [card.id, card])
  );

  const now = Date.now();

  return schedules
    .filter((schedule) => {
      const dueTime = parseDueAt(schedule.dueAt);

      return (
        Number.isFinite(dueTime) &&
        dueTime <= now &&
        !pendingCardIds.has(schedule.cardId) &&
        cardMap.has(schedule.cardId)
      );
    })
    .map((schedule) => {
      const card = cardMap.get(schedule.cardId)!;

      return {
        cardId: card.id,
        deckId,
        front: card.front,
        back: card.back,
        dueAt: schedule.dueAt,
        algorithmVersion: schedule.algorithmVersion,
        intervals: null,
      };
    });
}

async function loadDueCards(
  deckId: string
): Promise<DueCardResponse[]> {
  const pendingCardIds = await getPendingCardIds();

  if (!navigator.onLine) {
    return getOfflineDueCards(deckId, pendingCardIds);
  }

  try {
    const cards = await getDueCards(deckId);

    return cards.filter(
      (card) => !pendingCardIds.has(card.cardId)
    );
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }

    return getOfflineDueCards(deckId, pendingCardIds);
  }
}

export function useDueCards(deckId: string) {
  return useQuery({
    queryKey: ['reviews', 'due', deckId],
    queryFn: () => loadDueCards(deckId),
    enabled: Boolean(deckId),
    networkMode: 'always',
    retry: false,
    refetchOnWindowFocus: false,
  });
}
