import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { downloadCsv, learningApi } from './api/learningApi';
import { LearningLayout } from './components/LearningLayout';

export function CsvPage() {
  const { deckId = '' } = useParams();
  const cache = useQueryClient();
  const [content, setContent] = useState('');
  const [fileError, setFileError] = useState('');
  const [reading, setReading] = useState(false);
  const preview = useMutation({ mutationFn: (text: string) => learningApi.previewCsv(text) });
  const commit = useMutation({ mutationFn: () => learningApi.importCsv(deckId, content), onSuccess: () => {
    void cache.invalidateQueries({ queryKey: ['decks'] });
    void cache.invalidateQueries({ queryKey: ['cards'] });
    preview.reset();
    setContent('');
  } });
  const download = useMutation({ mutationFn: ({ endpoint, filename }: { endpoint: string; filename: string }) => downloadCsv(endpoint, filename) });
  return <LearningLayout title="Nhập / xuất bộ thẻ CSV">
    <div className="flex gap-4"><button disabled={download.isPending} className="font-bold text-rose-700" onClick={() => download.mutate({ endpoint: '/csv/template', filename: 'cards-template.csv' })}>Tải CSV mẫu</button><button disabled={download.isPending} className="font-bold text-rose-700" onClick={() => download.mutate({ endpoint: `/decks/${deckId}/csv/export`, filename: 'cards.csv' })}>Xuất bộ thẻ</button></div>
    <label className="block rounded-xl border-2 bg-white p-5">Chọn file CSV UTF-8 (tối đa 2 MB, 5000 dòng)
      <input className="mt-3 block" type="file" accept=".csv,text/csv" disabled={reading || commit.isPending} onChange={async event => {
        const file = event.target.files?.[0];
        preview.reset(); commit.reset(); setContent(''); setFileError('');
        if (!file) return;
        if (file.size > 2_000_000) { setFileError('File vượt quá 2 MB.'); return; }
        setReading(true);
        try {
          const text = new TextDecoder('utf-8', { fatal: true }).decode(await file.arrayBuffer());
          setContent(text);
          preview.mutate(text);
        } catch { setFileError('Không đọc được file UTF-8 hợp lệ.'); }
        finally { setReading(false); }
      }} />
    </label>
    {(reading || preview.isPending) && <p role="status">Đang kiểm tra CSV...</p>}
    {fileError && <p role="alert">{fileError}</p>}
    {preview.isError && <p role="alert">{preview.error.message}</p>}
    {commit.isError && <p role="alert">{commit.error.message}</p>}
    {download.isError && <p role="alert">{download.error.message}</p>}
    {commit.isSuccess && <p role="status">Đã nhập {commit.data.importedCount} thẻ.</p>}
    {preview.data && <section className="space-y-4"><p>{preview.data.validCount} dòng hợp lệ · {preview.data.invalidCount} dòng lỗi</p>
      <div className="max-h-96 overflow-auto"><table className="w-full bg-white text-left"><thead><tr><th>Dòng</th><th>Mặt trước</th><th>Mặt sau</th><th>Lỗi</th></tr></thead><tbody>{preview.data.rows.map(row => <tr key={row.line} className="border-t"><td className="p-2">{row.line}</td><td className="p-2">{row.front}</td><td className="p-2">{row.back}</td><td className="p-2 text-red-700">{row.error}</td></tr>)}</tbody></table></div>
      <button disabled={commit.isPending || preview.data.invalidCount > 0 || preview.data.validCount === 0} className="rounded bg-rose-700 px-5 py-3 text-white disabled:opacity-50" onClick={() => commit.mutate()}>{commit.isPending ? 'Đang nhập...' : 'Xác nhận nhập thẻ'}</button>
    </section>}
  </LearningLayout>;
}
