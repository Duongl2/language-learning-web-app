
import { useEffect, useState } from 'react';
import { liveQuery } from 'dexie';
import { Wifi, WifiOff, CloudUpload } from 'lucide-react';

import { db } from './db';

export function OfflineStatusBar() {
  const [isOnline, setIsOnline] = useState(
    () => navigator.onLine
  );

  const [pendingCount, setPendingCount] = useState<
    number | null
  >(null);

  useEffect(() => {
    const updateOnlineStatus = () => {
      setIsOnline(navigator.onLine);
    };

    window.addEventListener('online', updateOnlineStatus);
    window.addEventListener('offline', updateOnlineStatus);

    const subscription = liveQuery(() =>
      db.syncQueue
        .where('status')
        .anyOf('Pending', 'Failed')
        .count()
    ).subscribe({
      next: (count) => setPendingCount(count),
      error: () => setPendingCount(null),
    });

    return () => {
      window.removeEventListener(
        'online',
        updateOnlineStatus
      );

      window.removeEventListener(
        'offline',
        updateOnlineStatus
      );

      subscription.unsubscribe();
    };
  }, []);

  if (isOnline && pendingCount === 0) {
    return null;
  }

  const isError = pendingCount === null;

  return (
    <div
      role="status"
      aria-live="polite"
      className={`relative z-50 flex items-center justify-center gap-3 border-b px-4 py-3 text-sm ${
        !isOnline
          ? 'border-orange-200 bg-orange-50 text-orange-800'
          : 'border-blue-200 bg-blue-50 text-blue-800'
      }`}
    >
      {!isOnline ? (
        <WifiOff size={18} aria-hidden="true" />
      ) : isError ? (
        <Wifi size={18} aria-hidden="true" />
      ) : (
        <CloudUpload size={18} aria-hidden="true" />
      )}

      <div>
        <p className="font-semibold">
          {isError
            ? 'Không thể đọc hàng đợi đồng bộ'
            : !isOnline
              ? `Đang offline - ${pendingCount} thao tác chờ đồng bộ`
              : `Đang online - ${pendingCount} thao tác chờ đồng bộ`}
        </p>

        <p className="text-xs opacity-80">
          {isOnline
            ? 'Dữ liệu đang chờ chức năng đồng bộ với máy chủ.'
            : 'Kết quả ôn tập được lưu trên thiết bị.'}
        </p>
      </div>
    </div>
  );
}
