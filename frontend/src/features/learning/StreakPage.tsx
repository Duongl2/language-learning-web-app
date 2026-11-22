import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { learningApi } from './api/streakApi';
import { LearningLayout, QueryStatus } from './components/LearningLayout';

export function StreakPage() {
  const cache = useQueryClient();
  const [target, setTarget] = useState(10);
  const streak = useQuery({ queryKey: ['streak'], queryFn: learningApi.streak });
  const saveGoal = useMutation({ mutationFn: () => learningApi.goal(target), onSuccess: () => { void cache.invalidateQueries({ queryKey: ['streak'] }); } });
  return <LearningLayout title="Chuỗi ngày học và mục tiêu">
    <QueryStatus pending={streak.isPending} error={streak.error} empty={false} />
    {streak.data && <section className="space-y-3 rounded-xl border-2 bg-white p-5"><h2 className="text-xl font-bold">Chuỗi hiện tại: {streak.data.currentStreak} ngày</h2><p>Kỷ lục: {streak.data.longestStreak} ngày · Múi giờ: {streak.data.timezone}</p>
      <label>Mục tiêu hôm nay: {streak.data.todayReviews}/{streak.data.dailyTarget}<progress className="block w-full" aria-label="Mục tiêu hôm nay" max={streak.data.dailyTarget} value={Math.min(streak.data.todayReviews, streak.data.dailyTarget)} /></label>
      <form onSubmit={e => { e.preventDefault(); saveGoal.mutate(); }} className="flex flex-wrap gap-3"><label>Mục tiêu mới<input type="number" min={1} max={1000} required className="ml-2 w-24 rounded border p-2" value={target} onChange={e => setTarget(Number(e.target.value))} /></label><button disabled={saveGoal.isPending} className="rounded bg-rose-700 px-4 text-white">Lưu mục tiêu</button></form>
      {saveGoal.isError && <p role="alert">{saveGoal.error.message}</p>}{saveGoal.isSuccess && <p role="status">Đã lưu mục tiêu.</p>}
    </section>}
    {streak.data && <div className="flex flex-wrap gap-2">{streak.data.days.map(day => <div key={day.date} tabIndex={0} title={`${day.date}: ${day.reviewCount} lượt ôn`} className={`rounded border p-3 text-xs ${day.goalMet ? 'bg-orange-500 text-white' : 'bg-stone-100'}`}><time>{day.date}</time><p>{day.reviewCount} lượt ôn</p></div>)}</div>}
  </LearningLayout>;
}
