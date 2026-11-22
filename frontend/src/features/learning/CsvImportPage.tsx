import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { learningApi } from './api/csvTransferApi';
import { downloadCsv } from './api/downloadCsv';
import { CsvPreviewPanel } from './CsvPreviewPanel';
import { LearningLayout } from './components/LearningLayout';

export function CsvImportPage() {
  const { deckId = '' } = useParams();
  const cache = useQueryClient();
  const [content, setContent] = useState<string | null>(null);
  const [fileVersion, setFileVersion] = useState(0);
  const commit = useMutation({ mutationFn: () => learningApi.importCsv(deckId, content!), onSuccess: () => {
    setContent(null); setFileVersion(version => version + 1);
    void cache.invalidateQueries({ queryKey: ['decks'] });
    void cache.invalidateQueries({ queryKey: ['cards'] });
  } });
  const download = useMutation({ mutationFn: () => downloadCsv(`/decks/${deckId}/csv/export`, 'cards.csv') });
  return <LearningLayout title="Nhập / xuất bộ thẻ CSV">
    <button disabled={download.isPending} onClick={() => download.mutate()} className="font-bold text-rose-700">Xuất bộ thẻ</button>
    <fieldset disabled={commit.isPending}><CsvPreviewPanel key={fileVersion} onValidated={setContent} /></fieldset>
    <button disabled={!content || commit.isPending} className="rounded bg-rose-700 px-5 py-3 text-white disabled:opacity-50" onClick={() => commit.mutate()}>{commit.isPending ? 'Đang nhập...' : 'Xác nhận nhập thẻ'}</button>
    {commit.isError && <p role="alert">{commit.error.message}</p>}
    {download.isError && <p role="alert">{download.error.message}</p>}
    {commit.isSuccess && <p role="status">Đã nhập {commit.data.importedCount} thẻ.</p>}
  </LearningLayout>;
}
