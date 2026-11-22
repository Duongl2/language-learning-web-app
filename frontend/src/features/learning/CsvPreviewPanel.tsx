import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { learningApi } from './api/csvPreviewApi';
import { downloadCsv } from './api/downloadCsv';

export function CsvPreviewPanel({ onValidated }: { onValidated?: (content: string | null) => void }) {
  const [fileError, setFileError] = useState('');
  const [reading, setReading] = useState(false);
  const preview = useMutation({ mutationFn: (content: string) => learningApi.previewCsv(content), onSuccess: (data, content) => onValidated?.(data.invalidCount === 0 && data.validCount > 0 ? content : null) });
  const download = useMutation({ mutationFn: () => downloadCsv('/csv/template', 'cards-template.csv') });
  return <section className="space-y-4">
    <button className="font-bold text-rose-700" disabled={download.isPending} onClick={() => download.mutate()}>Tải CSV mẫu</button>
    <label className="block rounded-xl border-2 bg-white p-5">Chọn file CSV UTF-8 (tối đa 2 MB, 5000 dòng)
      <input type="file" accept=".csv,text/csv" disabled={reading || preview.isPending} className="mt-3 block" onChange={async event => {
        const file = event.target.files?.[0];
        preview.reset(); setFileError(''); onValidated?.(null);
        if (!file) return;
        if (file.size > 2_000_000) { setFileError('File vượt quá 2 MB.'); return; }
        setReading(true);
        try { preview.mutate(new TextDecoder('utf-8', { fatal: true }).decode(await file.arrayBuffer())); }
        catch { setFileError('Không đọc được file UTF-8 hợp lệ.'); }
        finally { setReading(false); }
      }} />
    </label>
    {(reading || preview.isPending) && <p role="status">Đang kiểm tra CSV...</p>}
    {fileError && <p role="alert">{fileError}</p>}
    {preview.isError && <p role="alert">{preview.error.message}</p>}
    {download.isError && <p role="alert">{download.error.message}</p>}
    {preview.data && <><p>{preview.data.validCount} dòng hợp lệ · {preview.data.invalidCount} dòng lỗi</p><div className="max-h-96 overflow-auto"><table className="w-full bg-white text-left"><thead><tr><th>Dòng</th><th>Mặt trước</th><th>Mặt sau</th><th>Lỗi</th></tr></thead><tbody>{preview.data.rows.map(row => <tr key={row.line} className="border-t"><td className="p-2">{row.line}</td><td className="p-2">{row.front}</td><td className="p-2">{row.back}</td><td className="p-2 text-red-700">{row.error}</td></tr>)}</tbody></table></div></>}
  </section>;
}
