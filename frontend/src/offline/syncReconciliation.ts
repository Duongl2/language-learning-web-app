
import { db } from './db';
import { pullSyncChanges } from './syncApi';
import { getAuthSnapshot } from '../features/auth/authSession';

export class SyncBlockedByPendingReviewsError extends Error {
  constructor() {
    super('Cannot delete an offline deck with unsynced reviews.');
    this.name = 'SyncBlockedByPendingReviewsError';
  }
}

export interface SyncPullSummary {
  updatedCount: number;
  deletedCount: number;
  pageCount: number;
}

let activePull: Promise<SyncPullSummary> | null = null;

async function processSyncPull(): Promise<SyncPullSummary> {
  const user = getAuthSnapshot().user;

  const summary: SyncPullSummary = {
    updatedCount: 0,
    deletedCount: 0,
    pageCount: 0,
  };

  if (!user || !navigator.onLine) {
    return summary;
  }

  const savedCursor = await db.syncCursors.get(user.id);
  let cursor = savedCursor?.cursor ?? null;

  while (true) {
    if (!navigator.onLine) {
      throw new Error('Network connection lost during sync.');
    }

    const response = await pullSyncChanges(cursor, 50);

    if (getAuthSnapshot().user?.id !== user.id) {
      throw new Error('User session changed during sync.');
    }

    if (
      !Array.isArray(response.changes) ||
      (response.changes.length > 0 && !response.nextCursor) ||
      (response.hasMore &&
        (response.changes.length === 0 ||
          response.nextCursor === cursor))
    ) {
      throw new Error('Invalid sync pull response.');
    }

    await db.transaction(
      'rw',
      db.decks,
      db.cards,
      db.cardSchedules,
      db.syncQueue,
      db.syncCursors,
      async () => {
        for (const change of response.changes) {
          if (change.entityType !== 'deck') {
            continue;
          }

          const localDeck = await db.decks.get(
            change.entityId
          );

          if (
            !localDeck ||
            localDeck.ownerId !== user.id ||
            localDeck.version > change.version
          ) {
            continue;
          }

          if (change.action === 'delete') {
            const cardIds = await db.cards
              .where('deckId')
              .equals(change.entityId)
              .primaryKeys();

            const cardIdSet = new Set(cardIds);

            const unsyncedReviews = await db.syncQueue
              .where('status')
              .anyOf('Pending', 'Failed')
              .filter(
                (operation) =>
                  operation.type === 'Review' &&
                  cardIdSet.has(operation.entityId)
              )
              .count();

            if (unsyncedReviews > 0) {
              throw new SyncBlockedByPendingReviewsError();
            }

            await db.cards
              .where('deckId')
              .equals(change.entityId)
              .delete();

            await db.cardSchedules
              .where('deckId')
              .equals(change.entityId)
              .delete();

            await db.decks.delete(change.entityId);

            summary.deletedCount++;
          } else if (
            change.action === 'upsert' &&
            change.deck
          ) {
            await db.decks.put({
              ...localDeck,
              languageId: change.deck.languageId,
              title: change.deck.title,
              description: change.deck.description,
              visibility: change.deck.isPublic
                ? 'Public'
                : 'Private',
              tags: change.deck.tags,
              version: change.version,
            });

            summary.updatedCount++;
          } else {
            throw new Error(
              'Unsupported sync change received.'
            );
          }
        }

        const currentCursor = await db.syncCursors.get(
          user.id
        );

        await db.syncCursors.put({
          userId: user.id,
          cursor: response.nextCursor,
          lastSyncedAt: response.hasMore
            ? currentCursor?.lastSyncedAt ?? null
            : new Date().toISOString(),
        });
      }
    );

    cursor = response.nextCursor;
    summary.pageCount++;

    if (!response.hasMore) {
      return summary;
    }
  }
}

export function reconcileSyncPull(): Promise<SyncPullSummary> {
  if (activePull) {
    return activePull;
  }

  activePull = processSyncPull().finally(() => {
    activePull = null;
  });

  return activePull;
}
