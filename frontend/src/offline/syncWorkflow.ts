
import { db } from './db';
import { getAuthSnapshot } from '../features/auth/authSession';
import { pushPendingSyncQueue } from './syncEngine';
import { reconcileSyncPull } from './syncReconciliation';

import type { SyncPushSummary } from './syncEngine';
import type { SyncPullSummary } from './syncReconciliation';

export interface SyncWorkflowSummary {
  push: SyncPushSummary;
  pull: SyncPullSummary | null;
  unresolvedCount: number;
}

let activeWorkflow: Promise<SyncWorkflowSummary> | null = null;

async function executeSyncWorkflow(): Promise<SyncWorkflowSummary> {
  const user = getAuthSnapshot().user;

  if (!user || !navigator.onLine) {
    throw new Error(
      'Synchronization requires an online authenticated user.'
    );
  }

  const push = await pushPendingSyncQueue();

  if (getAuthSnapshot().user?.id !== user.id) {
    throw new Error(
      'User session changed during synchronization.'
    );
  }

  const unresolvedCount = await db.syncQueue
    .where('status')
    .anyOf('Pending', 'Failed')
    .count();

  if (unresolvedCount > 0) {
    return {
      push,
      pull: null,
      unresolvedCount,
    };
  }

  const pullEnabled =
    import.meta.env.VITE_ENABLE_SYNC_PULL === 'true';

  if (!pullEnabled) {
    return {
      push,
      pull: null,
      unresolvedCount: 0,
    };
  }

  const pull = await reconcileSyncPull();

  return {
    push,
    pull,
    unresolvedCount: 0,
  };
}

export function runSyncWorkflow(): Promise<SyncWorkflowSummary> {
  if (activeWorkflow) {
    return activeWorkflow;
  }

  activeWorkflow = executeSyncWorkflow().finally(() => {
    activeWorkflow = null;
  });

  return activeWorkflow;
}
