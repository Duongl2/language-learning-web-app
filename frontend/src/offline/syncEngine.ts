
import { db } from './db';
import { pushSyncOperations } from './syncApi';
import { getAuthSnapshot } from '../features/auth/authSession';

import type { SyncOperation } from '../types';

export interface SyncPushSummary {
  syncedCount: number;
  failedCount: number;
  pendingCount: number;
}

let activePush: Promise<SyncPushSummary> | null = null;

export function getBackoffDelay(attempt: number): number {
  const safeAttempt = Math.max(0, Math.floor(attempt));

  return Math.min(
    30000,
    1000 * Math.pow(2, Math.min(safeAttempt, 10))
  );
}

async function getPendingCount(): Promise<number> {
  return db.syncQueue
    .where('status')
    .equals('Pending')
    .count();
}

async function processPendingQueue(): Promise<SyncPushSummary> {
  let syncedCount = 0;
  let failedCount = 0;

  if (!navigator.onLine || !getAuthSnapshot().user) {
    return {
      syncedCount,
      failedCount,
      pendingCount: await getPendingCount(),
    };
  }

  const pendingOperations = await db.syncQueue
    .where('status')
    .equals('Pending')
    .sortBy('occurredAt');

  for (let index = 0; index < pendingOperations.length; index += 100) {
    if (!navigator.onLine) {
      break;
    }

    const batch: SyncOperation[] = pendingOperations.slice(
      index,
      index + 100
    );

    const response = await pushSyncOperations(batch);

    const results = new Map(
      response.results.map((result) => [
        result.operationId.toLowerCase(),
        result,
      ])
    );

    if (
      response.results.length !== batch.length ||
      results.size !== batch.length ||
      batch.some(
        (operation) =>
          !results.has(operation.operationId.toLowerCase())
      )
    ) {
      throw new Error('Sync response does not match the queue.');
    }

    await db.transaction(
      'rw',
      db.syncQueue,
      async () => {
        for (const operation of batch) {
          const current = await db.syncQueue.get(
            operation.operationId
          );

          if (!current || current.status !== 'Pending') {
            continue;
          }

          const result = results.get(
            operation.operationId.toLowerCase()
          );

          if (!result) {
            throw new Error('Missing sync operation result.');
          }

          if (
            result.status === 'accepted' ||
            result.status === 'duplicate'
          ) {
            await db.syncQueue.update(
              operation.operationId,
              { status: 'Synced' }
            );

            syncedCount++;
          } else if (
            result.status === 'conflict' ||
            result.status === 'rejected'
          ) {
            await db.syncQueue.update(
              operation.operationId,
              { status: 'Failed' }
            );

            failedCount++;
          } else {
            throw new Error('Unknown sync operation status.');
          }
        }
      }
    );
  }

  return {
    syncedCount,
    failedCount,
    pendingCount: await getPendingCount(),
  };
}

export function pushPendingSyncQueue(): Promise<SyncPushSummary> {
  if (activePush) {
    return activePush;
  }

  activePush = processPendingQueue().finally(() => {
    activePush = null;
  });

  return activePush;
}
