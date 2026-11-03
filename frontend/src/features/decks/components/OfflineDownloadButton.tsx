
import { useState } from 'react';
import { CheckCircle2, Download, LoaderCircle } from 'lucide-react';

import { downloadDeckForOffline } from '../../../offline/downloadDeck';

interface OfflineDownloadButtonProps {
  deckId: string;
  deckTitle: string;
}

export function OfflineDownloadButton({
  deckId,
  deckTitle,
}: OfflineDownloadButtonProps) {
  const [isDownloading, setIsDownloading] = useState(false);
  const [downloadedCount, setDownloadedCount] = useState<number | null>(null);
  const [error, setError] = useState('');

  const handleDownload = async () => {
    if (isDownloading) return;

    setIsDownloading(true);
    setError('');

    try {
      const count = await downloadDeckForOffline(deckId);
      setDownloadedCount(count);
    } catch {
      setError('Không thể tải bộ thẻ. Vui lòng thử lại.');
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <div>
      <button
        type="button"
        onClick={handleDownload}
        disabled={isDownloading}
        aria-label={`Tải bộ thẻ ${deckTitle} về học offline`}
        className="inline-flex items-center gap-2 rounded-lg border border-blue-200 bg-blue-50 px-3 py-2 text-xs font-semibold text-blue-700 transition-colors hover:bg-blue-100 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isDownloading ? (
          <LoaderCircle size={14} className="animate-spin" />
        ) : downloadedCount !== null ? (
          <CheckCircle2 size={14} />
        ) : (
          <Download size={14} />
        )}

        {isDownloading
          ? 'Đang tải...'
          : downloadedCount !== null
            ? 'Đã tải offline'
            : 'Tải về học offline'}
      </button>

      {downloadedCount !== null && !error && (
        <p role="status" className="mt-1 text-xs text-green-700">
          Đã lưu {downloadedCount} thẻ.
        </p>
      )}

      {error && (
        <p role="alert" className="mt-1 text-xs text-red-600">
          {error}
        </p>
      )}
    </div>
  );
}
