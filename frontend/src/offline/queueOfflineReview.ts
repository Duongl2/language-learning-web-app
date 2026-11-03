
import { db } from './db';

import type {
  ReviewEvent,
  SyncOperation,
  SrsRating,
} from '../types';

export async function queueOfflineReview(
  deckId: string,
  cardId: string,
  rating: SrsRating,
  algorithmVersion?: string
): Promise<ReviewEvent> {
  if (!deckId || !cardId) {
    throw new Error('Deck ID and Card ID are required.');
  }

  const validRatings: SrsRating[] = [
    'Again',
    'Hard',
    'Good',
    'Easy',
  ];

  if (!validRatings.includes(rating)) {
    throw new Error('Invalid review rating.');
  }

  const clientEventId = crypto.randomUUID();
  const operationId = crypto.randomUUID();
  const reviewedAt = new Date().toISOString();

  return db.transaction(
    'rw',
    db.decks,
    db.cards,
    db.cardSchedules,
    db.offlineReviews,
    db.syncQueue,
    async () => {
      const deck = await db.decks.get(deckId);
      const card = await db.cards.get(cardId);
      const schedule = await db.cardSchedules.get(cardId);

      const isDownloaded = Boolean(
        deck &&
        card &&
        schedule &&
        card.deckId === deckId &&
        schedule.deckId === deckId
      );

      if (!navigator.onLine && !isDownloaded) {
        throw new Error(
          'Deck or card is not available offline.'
        );
      }

      const resolvedAlgorithmVersion = navigator.onLine
        ? algorithmVersion ??
          (isDownloaded ? schedule?.algorithmVersion : undefined)
        : isDownloaded
          ? schedule?.algorithmVersion
          : undefined;

      if (!resolvedAlgorithmVersion) {
        throw new Error(
          'Review algorithm version is required.'
        );
      }

      const reviewEvent: ReviewEvent = {
        clientEventId,
        cardId,
        rating,
        reviewedAt,
        algorithmVersion: resolvedAlgorithmVersion,
      };

      const syncOperation: SyncOperation = {
        operationId,
        type: 'Review',
        entityId: cardId,
        payload: {
          clientEventId,
          cardId,
          rating,
          reviewedAt,
          algorithmVersion: resolvedAlgorithmVersion,
        },
        occurredAt: reviewedAt,
        status: 'Pending',
      };

      await db.offlineReviews.add(reviewEvent);
      await db.syncQueue.add(syncOperation);

      return reviewEvent;
    }
  );
}
