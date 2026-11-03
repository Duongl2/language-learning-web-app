
import { db } from './db';

import { decksApi } from '../features/decks/api/decksApi';
import { getDeckCards } from '../features/decks/api/cardsApi';
import { getDeckSchedules } from '../features/decks/api/reviewSchedulesApi';

import type { Deck, OfflineCardSchedule } from '../types';

export async function downloadDeckForOffline(
  deckId: string
): Promise<number> {

  const [deckResponse, cards, schedules] = await Promise.all([
    decksApi.getDeckById(deckId),
    getDeckCards(deckId),
    getDeckSchedules(deckId),
  ]);

  const offlineDeck: Deck = {
    id: deckResponse.id,
    ownerId: deckResponse.ownerId,
    languageId: deckResponse.languageId,
    title: deckResponse.title,
    description: deckResponse.description,
    visibility: deckResponse.isPublic ? 'Public' : 'Private',
    totalCards: deckResponse.totalCards,
    dueCards: deckResponse.dueCards,
    newCards: deckResponse.newCards,
    tags: deckResponse.tags,
    version: deckResponse.version,
  };

  const offlineSchedules: OfflineCardSchedule[] =
    schedules.map((schedule) => ({
      ...schedule,
      deckId,
    }));

  await db.transaction(
    'rw',
    db.decks,
    db.cards,
    db.cardSchedules,
    db.syncQueue,
    async () => {

      const unsyncedCount = await db.syncQueue
        .where('status')
        .anyOf('Pending', 'Failed')
        .count();

      if (unsyncedCount > 0) {
        throw new Error(
          'Có dữ liệu offline chưa đồng bộ. Không thể tải lại bộ thẻ.'
        );
      }

      await db.decks.put(offlineDeck);

      await db.cards
        .where('deckId')
        .equals(deckId)
        .delete();

      await db.cards.bulkPut(cards);

      await db.cardSchedules
        .where('deckId')
        .equals(deckId)
        .delete();

      await db.cardSchedules.bulkPut(offlineSchedules);
    }
  );

  return cards.length;
}
