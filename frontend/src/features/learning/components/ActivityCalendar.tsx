import type { ActivityDay } from '../types/statistics.types';

export function ActivityCalendar({ days, from, to }: { days: ActivityDay[]; from: string; to: string }) {
  const end = new Date(`${to}T00:00:00Z`);
  const start = from ? new Date(`${from}T00:00:00Z`) : new Date(end.getTime() - 364 * 86400000);
  const count = Math.floor((end.getTime() - start.getTime()) / 86400000) + 1;
  if (!Number.isFinite(count) || count <= 0) return <p>Chọn khoảng ngày hợp lệ.</p>;
  if (count > 1096) return <p>Chọn tối đa ba năm để xem lịch hoạt động.</p>;
  const lookup = new Map(days.map(day => [day.date, day]));
  const cells = Array.from({ length: count }, (_, index) => {
    const date = new Date(start.getTime() + index * 86400000).toISOString().slice(0, 10);
    const day = lookup.get(date);
    const reviews = day?.reviewCount ?? 0;
    const color = reviews === 0 ? 'bg-stone-100' : day?.goalMet ? 'bg-green-700' : reviews >= 5 ? 'bg-green-500' : 'bg-green-200';
    return <div key={date} role="listitem" tabIndex={0} aria-label={`${date}: ${reviews} lượt ôn, ${day?.lessonsCompleted ?? 0} bài hoàn thành`} title={`${date}: ${reviews} lượt ôn, ${day?.lessonsCompleted ?? 0} bài hoàn thành`} className={`h-4 w-4 rounded-sm border border-stone-200 ${color}`} />;
  });
  return <div className="space-y-3 rounded-xl border bg-white p-4"><p className="text-sm">{start.toISOString().slice(0, 10)} — {to}</p><div className="overflow-x-auto pb-2"><div role="list" aria-label="Lịch hoạt động từng ngày" className="grid w-max grid-flow-col grid-rows-7 gap-1">{cells}</div></div><p className="text-xs text-stone-600">Mỗi ô là một ngày. Màu xanh đậm biểu thị ngày đạt mục tiêu; đưa chuột hoặc focus ô để xem số lượt ôn.</p></div>;
}
