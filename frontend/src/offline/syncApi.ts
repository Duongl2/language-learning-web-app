
import { apiRequest } from '../services/apiClient';

import type { SyncOperation } from '../types';

export interface SyncOperationResult {
  operationId: string;
  status: 'accepted' | 'duplicate' | 'conflict' | 'rejected';
  message: string | null;
}

export interface SyncPushResponse {
  results: SyncOperationResult[];
}

export interface SyncDeckSnapshot {
  id: string;
  languageId: string;
  title: string;
  description: string;
  isPublic: boolean;
  version: number;
  tags: string[];
}

export interface SyncChange {
  entityType: string;
  entityId: string;
  version: number;
  action: string;
  changedAt: string;
  deck: SyncDeckSnapshot | null;
}

export interface SyncPullResponse {
  changes: SyncChange[];
  nextCursor: string | null;
  hasMore: boolean;
}

export async function pushSyncOperations(
  operations: SyncOperation[]
): Promise<SyncPushResponse> {
  if (operations.length === 0) {
    return { results: [] };
  }

  return apiRequest<SyncPushResponse>('/sync/push', {
    method: 'POST',
    body: JSON.stringify({
      operations: operations.map((operation) => ({
        operationId: operation.operationId,
        type: operation.type,
        entityId: operation.entityId,
        payload: operation.payload,
        occurredAt: operation.occurredAt,
      })),
    }),
  });
}

export async function pullSyncChanges(
  cursor: string | null,
  limit = 50
): Promise<SyncPullResponse> {
  if (!Number.isInteger(limit) || limit < 1 || limit > 100) {
    throw new Error('Sync limit must be between 1 and 100.');
  }

  const params = new URLSearchParams({
    limit: String(limit),
  });

  if (cursor) {
    params.set('cursor', cursor);
  }

  return apiRequest<SyncPullResponse>(
    `/sync/pull?${params.toString()}`,
    {
      method: 'GET',
    }
  );
}
