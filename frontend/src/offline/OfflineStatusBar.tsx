
import { useEffect, useState } from 'react';
import { liveQuery } from 'dexie';
import {
  Wifi,
  WifiOff,
  CloudUpload,
  CheckCircle2,
  AlertTriangle,
} from 'lucide-react';

import { db } from './db';
import { getBackoffDelay } from './syncEngine';
import { runSyncWorkflow } from './syncWorkflow';
import { getAuthSnapshot } from '../features/auth/authSession';
import { ApiError } from '../services/apiClient';
import { SyncBlockedByPendingReviewsError } from './syncReconciliation';

type SyncStatus =
  | 'idle'
  | 'syncing'
  | 'success'
  | 'error';

export function OfflineStatusBar() {
  const [isOnline, setIsOnline] = useState(
    () => navigator.onLine
  );

  const [pendingCount, setPendingCount] =
    useState<number | null>(null);

  const [failedCount, setFailedCount] = useState(0);

  const [status, setStatus] =
    useState<SyncStatus>('idle');

  const [message, setMessage] = useState('');

  useEffect(() => {
    const pullEnabled =
      import.meta.env.VITE_ENABLE_SYNC_PULL === 'true';

    let disposed = false;
    let running = false;
    let initialized = false;
    let retryAttempt = 0;
    let pending = 0;
    let failed = 0;

    let retryTimer: ReturnType<typeof setTimeout> | null =
      null;

    let notificationTimer:
      | ReturnType<typeof setTimeout>
      | null = null;

    function clearRetryTimer() {
      if (retryTimer !== null) {
        clearTimeout(retryTimer);
        retryTimer = null;
      }
    }

    function clearNotificationTimer() {
      if (notificationTimer !== null) {
        clearTimeout(notificationTimer);
        notificationTimer = null;
      }
    }

    function canSync() {
      return (
        !disposed &&
        !running &&
        navigator.onLine &&
        Boolean(getAuthSnapshot().user) &&
        (
          pending > 0 ||
          (pullEnabled && failed === 0)
        )
      );
    }

    function scheduleSync(delay: number) {
      if (!canSync() || retryTimer !== null) {
        return;
      }

      retryTimer = setTimeout(() => {
        retryTimer = null;
        void runSync();
      }, delay);
    }

    async function runSync() {
      if (!canSync()) {
        return;
      }

      running = true;
      clearNotificationTimer();

      setStatus('syncing');
      setMessage('Đang đồng bộ dữ liệu...');

      let retryDelay: number | null = null;

      try {
        const result = await runSyncWorkflow();

        if (disposed) {
          return;
        }

        retryAttempt = 0;
        pending = result.push.pendingCount;

        if (result.unresolvedCount > 0) {
          setStatus('error');

          setMessage(
            `Còn ${result.unresolvedCount} thao tác chưa đồng bộ hoặc cần xử lý.`
          );
        } else if (
          result.push.syncedCount > 0 ||
          result.pull !== null
        ) {
          setStatus('success');

          if (result.pull) {
            setMessage(
              `Đồng bộ hoàn tất. Đã gửi ${result.push.syncedCount} thao tác, cập nhật ${result.pull.updatedCount} bộ thẻ và xóa ${result.pull.deletedCount} bộ thẻ.`
            );
          } else {
            setMessage(
              `Đã gửi thành công ${result.push.syncedCount} thao tác lên máy chủ.`
            );
          }

          notificationTimer = setTimeout(() => {
            if (!disposed) {
              setStatus('idle');
              setMessage('');
            }
          }, 6000);
        } else {
          setStatus('idle');
          setMessage('');
        }
      } catch (error) {
        if (disposed) {
          return;
        }

        const blockedByReviews =
          error instanceof SyncBlockedByPendingReviewsError;

        const permanentError =
          blockedByReviews ||
          (
            error instanceof ApiError &&
            error.status >= 400 &&
            error.status < 500 &&
            error.status !== 408 &&
            error.status !== 429
          );

        setStatus('error');

        if (permanentError) {
          retryAttempt = 0;

          if (blockedByReviews) {
            setMessage(
              'Bộ thẻ còn kết quả ôn tập chưa đồng bộ. Dữ liệu đã được giữ lại. Vui lòng hoàn tất đồng bộ các lượt ôn tập trước khi thử lại.'
            );
          } else {
            setMessage(
              'Máy chủ từ chối đồng bộ. Vui lòng kiểm tra phiên đăng nhập hoặc dữ liệu.'
            );
          }
        } else {
          retryDelay = getBackoffDelay(retryAttempt);
          retryAttempt++;

          setMessage(
            `Đồng bộ thất bại. Sẽ thử lại sau ${Math.ceil(retryDelay / 1000)} giây.`
          );
        }
      } finally {
        running = false;

        if (!disposed && retryDelay !== null) {
          scheduleSync(retryDelay);
        }
      }
    }

    function updateNetworkStatus() {
      const online = navigator.onLine;

      setIsOnline(online);

      if (!online) {
        clearRetryTimer();
        retryAttempt = 0;
        setStatus('idle');
        setMessage('');
        return;
      }

      clearRetryTimer();
      retryAttempt = 0;
      scheduleSync(0);
    }

    window.addEventListener(
      'online',
      updateNetworkStatus
    );

    window.addEventListener(
      'offline',
      updateNetworkStatus
    );

    const subscription = liveQuery(async () => {
      const [pendingOperations, failedOperations] =
        await Promise.all([
          db.syncQueue
            .where('status')
            .equals('Pending')
            .count(),
          db.syncQueue
            .where('status')
            .equals('Failed')
            .count(),
        ]);

      return {
        pendingOperations,
        failedOperations,
      };
    }).subscribe({
      next: (result) => {
        if (disposed) {
          return;
        }

        const previousPending = pending;

        pending = result.pendingOperations;
        failed = result.failedOperations;

        setPendingCount(pending);
        setFailedCount(failed);

        if (!initialized) {
          initialized = true;
          scheduleSync(0);
        } else if (pending > previousPending) {
          clearRetryTimer();
          retryAttempt = 0;
          scheduleSync(0);
        }
      },
      error: () => {
        if (!disposed) {
          setPendingCount(null);
          setStatus('error');

          setMessage(
            'Không thể đọc hàng đợi IndexedDB.'
          );
        }
      },
    });

    return () => {
      disposed = true;

      clearRetryTimer();
      clearNotificationTimer();

      window.removeEventListener(
        'online',
        updateNetworkStatus
      );

      window.removeEventListener(
        'offline',
        updateNetworkStatus
      );

      subscription.unsubscribe();
    };
  }, []);

  if (
    isOnline &&
    pendingCount === 0 &&
    failedCount === 0 &&
    status === 'idle'
  ) {
    return null;
  }

  const hasError =
    pendingCount === null ||
    failedCount > 0 ||
    status === 'error';

  const backgroundClass = !isOnline
    ? 'border-orange-200 bg-orange-50 text-orange-800'
    : hasError
      ? 'border-red-200 bg-red-50 text-red-800'
      : status === 'success'
        ? 'border-green-200 bg-green-50 text-green-800'
        : 'border-blue-200 bg-blue-50 text-blue-800';

  const title = !isOnline
    ? `Đang offline - ${pendingCount ?? 0} thao tác chờ đồng bộ`
    : status === 'syncing'
      ? 'Đang đồng bộ dữ liệu'
      : status === 'success'
        ? 'Đồng bộ thành công'
        : hasError
          ? 'Đồng bộ gặp lỗi'
          : `${pendingCount ?? 0} thao tác đang chờ đồng bộ`;

  return (
    <div
      role={hasError ? 'alert' : 'status'}
      aria-live="polite"
      className={`relative z-50 flex items-center justify-center gap-3 border-b px-4 py-3 text-sm ${backgroundClass}`}
    >
      {!isOnline ? (
        <WifiOff size={18} aria-hidden="true" />
      ) : hasError ? (
        <AlertTriangle size={18} aria-hidden="true" />
      ) : status === 'success' ? (
        <CheckCircle2 size={18} aria-hidden="true" />
      ) : status === 'syncing' ? (
        <CloudUpload size={18} aria-hidden="true" />
      ) : (
        <Wifi size={18} aria-hidden="true" />
      )}

      <div>
        <p className="font-semibold">{title}</p>

        <p className="text-xs opacity-80">
          {!isOnline
            ? 'Kết quả ôn tập được lưu trên thiết bị.'
            : message ||
              (failedCount > 0
                ? 'Có thao tác bị từ chối hoặc xung đột, cần kiểm tra dữ liệu.'
                : 'Dữ liệu sẽ được tự động đồng bộ khi có kết nối.')}
        </p>

        {failedCount > 0 && (
          <p className="mt-1 text-xs font-medium">
            {failedCount} thao tác cần được kiểm tra.
          </p>
        )}
      </div>
    </div>
  );
}
