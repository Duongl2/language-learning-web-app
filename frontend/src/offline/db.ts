
import Dexie, { type Table } from 'dexie';

import type {
  Deck,
  Flashcard,
  ReviewEvent,
  SyncOperation,
  OfflineCardSchedule,
} from '../types';

export class AppIndexedDb extends Dexie {
  decks!: Table<Deck, string>;
  cards!: Table<Flashcard, string>;
  offlineReviews!: Table<ReviewEvent, string>;
  syncQueue!: Table<SyncOperation, string>;
  cardSchedules!: Table<OfflineCardSchedule, string>;

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
  }
}

export const db = new AppIndexedDb();
