import { NavLink } from 'react-router-dom';
import { LearningNavigation } from '../LearningNavigation';
import type { ReactNode } from 'react';

export function LearningLayout({ title, children }: { title: string; children: ReactNode }) {
  return <main className="graph-paper min-h-screen px-4 py-8 text-stone-900 sm:px-8">
    <div className="mx-auto max-w-5xl space-y-6">
      <nav aria-label="Điều hướng học tập" className="flex flex-wrap gap-4 rounded-xl border-2 border-stone-900 bg-white p-4 text-sm font-bold">
        <NavLink to="/?view=myDecks">Bộ thẻ của tôi</NavLink>
        <NavLink to="/courses">Khóa học</NavLink>
        <LearningNavigation />
      </nav>
      <h1 className="text-3xl font-extrabold">{title}</h1>
      {children}
    </div>
  </main>;
}

export function QueryStatus({ pending, error, empty, onRetry }: {
  pending: boolean; error: Error | null; empty: boolean; onRetry?: () => void;
}) {
  if (pending) return <p role="status">Đang tải...</p>;
  if (error) return <div role="alert"><p>{error.message}</p>{onRetry && <button onClick={onRetry}>Thử lại</button>}</div>;
  if (empty) return <p>Chưa có dữ liệu.</p>;
  return null;
}
