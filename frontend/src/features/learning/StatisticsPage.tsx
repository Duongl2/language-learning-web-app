import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { learningApi } from './api/learningApi';
import { LearningLayout, QueryStatus } from './components/LearningLayout';
import { ActivityCalendar } from './components/ActivityCalendar';

export function StatisticsPage() {
  const cache = useQueryClient();
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [target, setTarget] = useState(10);
  const filter = new URLSearchParams({ ...(from && { from }), ...(to && { to }) }).toString();
  const overview = useQuery({ queryKey: ['statistics', filter], queryFn: () => learningApi.overview(filter) });
  const heatmap = useQuery({ queryKey: ['heatmap', filter], queryFn: () => learningApi.heatmap(filter) });
  const streak = useQuery({ queryKey: ['streak'], queryFn: learningApi.streak });
  const saveGoal = useMutation({ mutationFn: () => learningApi.goal(target), onSuccess: () => { void cache.invalidateQueries({ queryKey: ['streak'] }); } });
  const total = overview.data?.reviewCount ?? 0;
  const today = new Intl.DateTimeFormat('en-CA', { timeZone: streak.data?.timezone || 'UTC', year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date());
  return <LearningLayout title="Thống kê và chuỗi ngày học">
    <div className="flex flex-wrap gap-4"><label>Từ ngày<input className="ml-2 rounded border p-2" type="date" value={from} onChange={e => setFrom(e.target.value)} /></label><label>Đến ngày<input className="ml-2 rounded border p-2" type="date" value={to} min={from} onChange={e => setTo(e.target.value)} /></label></div>
    <QueryStatus pending={overview.isPending} error={overview.error} empty={false} onRetry={() => { void overview.refetch(); }} />
    {overview.data && <>
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">{[['Thẻ mới', overview.data.newCards], ['Đã học', overview.data.learnedCards], ['Đến hạn', overview.data.dueCards], ['Lượt ôn', total], ['Tỷ lệ nhớ', `${overview.data.retention}%`]].map(([label, value]) => <div key={label} className="rounded-xl border-2 border-stone-900 bg-white p-4"><p>{label}</p><strong className="text-2xl">{value}</strong></div>)}</div>
      <section className="rounded-xl border-2 bg-white p-5"><h2 className="mb-4 text-xl font-bold">Phân bố đánh giá</h2>
        {overview.data.ratings.map(rating => <div key={rating.rating} className="my-3"><label>{rating.rating}: {rating.count}<progress className="ml-3 w-1/2" aria-label={rating.rating} max={Math.max(1, total)} value={rating.count} /></label></div>)}
      </section>
    </>}
    <QueryStatus pending={streak.isPending} error={streak.error} empty={false} />
    {streak.data && <section className="space-y-3 rounded-xl border-2 bg-white p-5"><h2 className="text-xl font-bold">Chuỗi hiện tại: {streak.data.currentStreak} ngày</h2><p>Kỷ lục: {streak.data.longestStreak} ngày · Múi giờ: {streak.data.timezone}</p>
      <label>Mục tiêu hôm nay: {streak.data.todayReviews}/{streak.data.dailyTarget}<progress className="block w-full" aria-label="Mục tiêu hôm nay" max={streak.data.dailyTarget} value={Math.min(streak.data.todayReviews, streak.data.dailyTarget)} /></label>
      <form onSubmit={e => { e.preventDefault(); saveGoal.mutate(); }} className="flex flex-wrap gap-3"><label>Mục tiêu mới<input type="number" min={1} max={1000} required className="ml-2 w-24 rounded border p-2" value={target} onChange={e => setTarget(Number(e.target.value))} /></label><button disabled={saveGoal.isPending} className="rounded bg-rose-700 px-4 text-white">Lưu mục tiêu</button></form>
      {saveGoal.isError && <p role="alert">{saveGoal.error.message}</p>}{saveGoal.isSuccess && <p role="status">Đã lưu mục tiêu.</p>}
    </section>}
    <section><h2 className="mb-3 text-xl font-bold">Lịch hoạt động</h2>
      <QueryStatus pending={heatmap.isPending} error={heatmap.error} empty={heatmap.data?.length === 0} />
      {heatmap.data && <ActivityCalendar days={heatmap.data} from={from} to={to || today} />}
    </section>
    <p className="text-sm text-stone-600">Thống kê dựa trên dữ liệu đã đồng bộ. Các lượt ôn còn chờ trên thiết bị chưa được tính. Tỷ lệ nhớ là tỷ lệ đánh giá Hard, Good hoặc Easy trong các lượt ôn.</p>
  </LearningLayout>;
}
