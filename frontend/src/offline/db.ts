
import Dexie, { type Table } from 'dexie';

import type {
  Deck,
  Flashcard,
  ReviewEvent,
  SyncOperation,
  OfflineCardSchedule,
} from '../types';

export interface SyncCursor {
  userId: string;
  cursor: string | null;
  lastSyncedAt: string | null;
}

export class AppIndexedDb extends Dexie {
  decks!: Table<Deck, string>;
  cards!: Table<Flashcard, string>;
  offlineReviews!: Table<ReviewEvent, string>;
  syncQueue!: Table<SyncOperation, string>;
  cardSchedules!: Table<OfflineCardSchedule, string>;
  syncCursors!: Table<SyncCursor, string>;

  constructor() {
    super('LanguageLearningOfflineDb');

    this.version(1).stores({
      decks: 'id, ownerId, languageId, visibility',
      cards: 'id, deckId',
      offlineReviews: 'clientEventId, cardId, reviewedAt',
      syncQueue: 'operationId, type, status, occurredAt',
    });

    this.version(2).stores({
      cardSchedules: 'cardId, deckId, dueAt',
    });

    this.version(3).stores({
      syncCursors: 'userId',
    });
  }
}

export const db = new AppIndexedDb();
