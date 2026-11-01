
import { Link } from 'react-router-dom';
import {
  BookOpen,
  CalendarClock,
  Layers3,
  Pencil,
  Globe2,
  LockKeyhole,
  Trash2,
  Play,
} from 'lucide-react';

import type { DeckResponse } from '../types/deck.types';

interface DeckCardProps {
  deck: DeckResponse;
  languageName?: string;
  onEdit: (deck: DeckResponse) => void;
  onDelete: (deck: DeckResponse) => void;
}

export function DeckCard({
  deck,
  languageName,
  onEdit,
  onDelete,
}: DeckCardProps) {
  return (
    <article className="rounded-xl border-2 border-stone-800 bg-white p-5 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-start gap-3">
          <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-rose-100 text-rose-700">
            <BookOpen size={22} />
          </div>

          <div className="min-w-0">
            <h3 className="break-words text-base font-bold text-stone-900">
              {deck.title}
            </h3>

            <p className="mt-1 line-clamp-2 text-sm text-stone-500">
              {deck.description || 'Chưa có mô tả'}
            </p>
          </div>
        </div>

        <span
          className={`inline-flex shrink-0 items-center gap-1 rounded-full px-2.5 py-1 text-xs font-semibold ${
            deck.isPublic
              ? 'bg-emerald-50 text-emerald-700'
              : 'bg-stone-100 text-stone-600'
          }`}
        >
          {deck.isPublic ? (
            <Globe2 size={12} />
          ) : (
            <LockKeyhole size={12} />
          )}

          {deck.isPublic ? 'Công khai' : 'Riêng tư'}
        </span>
      </div>

      <div className="mt-5 grid grid-cols-2 gap-3">
        <div className="rounded-lg bg-orange-50 p-3">
          <div className="flex items-center gap-1.5 text-xs font-medium text-orange-700">
            <CalendarClock size={14} />
            Cần ôn
          </div>

          <p className="mt-2 text-2xl font-bold text-orange-800">
            {deck.dueCards}
          </p>
        </div>

        <div className="rounded-lg bg-stone-100 p-3">
          <div className="flex items-center gap-1.5 text-xs font-medium text-stone-600">
            <Layers3 size={14} />
            Tổng số thẻ
          </div>

          <p className="mt-2 text-2xl font-bold text-stone-800">
            {deck.totalCards}
          </p>
        </div>
      </div>

      <div className="mt-4 flex flex-wrap items-center gap-2">
        {deck.tags.map((tag) => (
          <span
            key={tag}
            className="rounded-md border border-stone-200 bg-stone-50 px-2 py-1 text-xs text-stone-600"
          >
            {tag}
          </span>
        ))}
      </div>

      <div className="mt-5 flex flex-wrap items-center justify-between gap-3 border-t border-stone-200 pt-4">
        <span className="min-w-0 truncate text-xs font-medium text-stone-500">
          {languageName || 'Ngôn ngữ chưa xác định'}
        </span>

        <div className="flex flex-wrap items-center gap-2">
          <Link
            to={`/reviews/${deck.id}`}
            className="inline-flex shrink-0 items-center gap-2 rounded-lg bg-green-600 px-3 py-2 text-xs font-semibold text-white transition-colors hover:bg-green-700"
            aria-label={`Ôn tập bộ thẻ ${deck.title}`}
          >
            <Play size={14} />
            Ôn tập
          </Link>

          <button
            type="button"
            onClick={() => onEdit(deck)}
            className="inline-flex shrink-0 items-center gap-2 rounded-lg bg-stone-900 px-3 py-2 text-xs font-semibold text-white transition-colors hover:bg-stone-700"
            aria-label={`Chỉnh sửa bộ thẻ ${deck.title}`}
          >
            <Pencil size={14} />
            Chỉnh sửa
          </button>

          <button
            type="button"
            onClick={() => onDelete(deck)}
            className="inline-flex shrink-0 items-center gap-2 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-xs font-semibold text-red-700 transition-colors hover:bg-red-100"
            aria-label={`Xóa bộ thẻ ${deck.title}`}
          >
            <Trash2 size={14} />
            Xóa
          </button>
        </div>
      </div>
    </article>
  );
}
