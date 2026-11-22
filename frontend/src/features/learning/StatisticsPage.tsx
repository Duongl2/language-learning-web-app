import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { learningApi } from './api/statisticsApi';
import { LearningLayout, QueryStatus } from './components/LearningLayout';
import { ActivityCalendar } from './components/ActivityCalendar';

export function StatisticsPage() {
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const filter = new URLSearchParams({ ...(from && { from }), ...(to && { to }) }).toString();
  const overview = useQuery({ queryKey: ['statistics', filter], queryFn: () => learningApi.overview(filter) });
  const heatmap = useQuery({ queryKey: ['heatmap', filter], queryFn: () => learningApi.heatmap(filter) });
  const total = overview.data?.reviewCount ?? 0;
  const today = new Intl.DateTimeFormat('en-CA', { timeZone: 'UTC', year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date());
  return <LearningLayout title="Thống kê học tập">
    <div className="flex flex-wrap gap-4"><label>Từ ngày<input className="ml-2 rounded border p-2" type="date" value={from} onChange={e => setFrom(e.target.value)} /></label><label>Đến ngày<input className="ml-2 rounded border p-2" type="date" value={to} min={from} onChange={e => setTo(e.target.value)} /></label></div>
    <QueryStatus pending={overview.isPending} error={overview.error} empty={false} onRetry={() => { void overview.refetch(); }} />
    {overview.data && <>
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">{[['Thẻ mới', overview.data.newCards], ['Đã học', overview.data.learnedCards], ['Đến hạn', overview.data.dueCards], ['Lượt ôn', total], ['Tỷ lệ nhớ', `${overview.data.retention}%`]].map(([label, value]) => <div key={label} className="rounded-xl border-2 border-stone-900 bg-white p-4"><p>{label}</p><strong className="text-2xl">{value}</strong></div>)}</div>
      <section className="rounded-xl border-2 bg-white p-5"><h2 className="mb-4 text-xl font-bold">Phân bố đánh giá</h2>
        {overview.data.ratings.map(rating => <div key={rating.rating} className="my-3"><label>{rating.rating}: {rating.count}<progress className="ml-3 w-1/2" aria-label={rating.rating} max={Math.max(1, total)} value={rating.count} /></label></div>)}
      </section>
    </>}
    <section><h2 className="mb-3 text-xl font-bold">Lịch hoạt động</h2>
      <QueryStatus pending={heatmap.isPending} error={heatmap.error} empty={heatmap.data?.length === 0} />
      {heatmap.data && <ActivityCalendar days={heatmap.data} from={from} to={to || today} />}
    </section>
    <p className="text-sm text-stone-600">Thống kê dựa trên dữ liệu đã đồng bộ. Các lượt ôn còn chờ trên thiết bị chưa được tính. Tỷ lệ nhớ là tỷ lệ đánh giá Hard, Good hoặc Easy trong các lượt ôn.</p>
  </LearningLayout>;
}
