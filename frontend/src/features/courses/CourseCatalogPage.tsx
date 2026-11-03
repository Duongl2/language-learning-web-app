import { useQuery } from '@tanstack/react-query';
import { BookOpen, ChevronRight, Globe2 } from 'lucide-react';
import { Link, useSearchParams } from 'react-router-dom';

import { languagesApi } from '../decks/api/languagesApi';
import { coursesApi } from './api/coursesApi';

const LEVELS = ['A1', 'A2', 'B1', 'B2', 'C1', 'C2'];

export function CourseCatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const languageId = searchParams.get('languageId') ?? '';
  const level = searchParams.get('level') ?? '';
  const requestedPage = Number(searchParams.get('page') ?? '1');
  const page = Number.isSafeInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1;
  const languages = useQuery({ queryKey: ['languages'], queryFn: languagesApi.getLanguages });
  const courses = useQuery({
    queryKey: ['courses', languageId, level, page],
    queryFn: () => coursesApi.list(page, languageId, level),
  });

  function updateFilter(key: 'languageId' | 'level', value: string) {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value);
    else next.delete(key);
    next.delete('page');
    setSearchParams(next);
  }

  function goToPage(nextPage: number) {
    const next = new URLSearchParams(searchParams);
    next.set('page', String(nextPage));
    setSearchParams(next);
  }

  return (
    <main className="graph-paper min-h-screen px-4 py-8 text-stone-900 sm:px-8">
      <div className="mx-auto max-w-5xl">
        <Link to="/" className="text-sm font-bold text-rose-700">← Về trang học</Link>
        <div className="mt-6 rounded-2xl border-2 border-stone-900 bg-white p-6 shadow-[5px_5px_0_#262626]">
          <p className="text-xs font-extrabold uppercase tracking-widest text-rose-600">KOTOBA · Lộ trình học</p>
          <h1 className="mt-2 text-3xl font-extrabold">Danh mục khóa học</h1>
          <p className="mt-2 text-sm text-stone-600">Chọn ngôn ngữ và trình độ phù hợp để bắt đầu.</p>
          <div className="mt-6 grid gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="catalog-language" className="mb-1 block text-sm font-bold">Ngôn ngữ</label>
              <select id="catalog-language" value={languageId} onChange={(event) => updateFilter('languageId', event.target.value)} className="w-full rounded-lg border-2 border-stone-300 bg-white p-2.5 focus:border-rose-500">
                <option value="">Tất cả ngôn ngữ</option>
                {languages.data?.map((language) => <option key={language.id} value={language.id}>{language.name}</option>)}
              </select>
            </div>
            <div>
              <label htmlFor="catalog-level" className="mb-1 block text-sm font-bold">Trình độ</label>
              <select id="catalog-level" value={level} onChange={(event) => updateFilter('level', event.target.value)} className="w-full rounded-lg border-2 border-stone-300 bg-white p-2.5 focus:border-rose-500">
                <option value="">Tất cả trình độ</option>
                {LEVELS.map((item) => <option key={item} value={item}>{item}</option>)}
              </select>
            </div>
          </div>
          {languages.isError && <p role="alert" className="mt-3 text-sm text-red-700">Không tải được danh sách ngôn ngữ: {languages.error.message}</p>}
        </div>

        {courses.isPending && <p role="status" className="mt-8 text-center text-stone-600">Đang tải khóa học...</p>}
        {courses.isError && <p role="alert" className="mt-8 rounded-lg bg-red-50 p-4 text-red-700">Không tải được khóa học: {courses.error.message}</p>}
        {courses.data && (
          <>
            <p className="mt-8 text-sm font-semibold text-stone-600">{courses.data.totalItems} khóa học</p>
            {courses.data.items.length === 0 ? (
              <div className="mt-4 rounded-xl border-2 border-dashed border-stone-300 bg-white p-8 text-center text-stone-600">Chưa có khóa học phù hợp.</div>
            ) : (
              <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {courses.data.items.map((course) => (
                  <Link key={course.id} to={`/courses/${course.id}`} className="group flex flex-col rounded-xl border-2 border-stone-900 bg-white p-5 shadow-[3px_3px_0_#262626] transition hover:-translate-y-1">
                    <div className="flex items-center justify-between">
                      <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-rose-100 text-rose-700"><BookOpen size={21} aria-hidden="true" /></span>
                      <span className="rounded-full bg-amber-100 px-2.5 py-1 text-xs font-extrabold text-amber-800">{course.level}</span>
                    </div>
                    <h2 className="mt-4 text-lg font-extrabold group-hover:text-rose-700">{course.title}</h2>
                    <p className="mt-2 line-clamp-3 flex-1 text-sm text-stone-600">{course.description || 'Khám phá lộ trình học của khóa này.'}</p>
                    <p className="mt-4 flex items-center gap-2 text-sm font-bold text-rose-700"><Globe2 size={16} aria-hidden="true" />{languages.data?.find((item) => item.id === course.languageId)?.name ?? 'Ngôn ngữ'}</p>
                    <span className="mt-4 inline-flex items-center gap-1 text-sm font-bold">Xem lộ trình <ChevronRight size={16} aria-hidden="true" /></span>
                  </Link>
                ))}
              </div>
            )}
            {courses.data.totalPages > 1 && (
              <nav aria-label="Phân trang khóa học" className="mt-7 flex items-center justify-center gap-4">
                <button type="button" disabled={!courses.data.hasPreviousPage} onClick={() => goToPage(page - 1)} className="rounded-lg border-2 border-stone-900 bg-white px-4 py-2 font-bold disabled:opacity-40">Trước</button>
                <span className="text-sm font-semibold">Trang {page}/{courses.data.totalPages}</span>
                <button type="button" disabled={!courses.data.hasNextPage} onClick={() => goToPage(page + 1)} className="rounded-lg border-2 border-stone-900 bg-white px-4 py-2 font-bold disabled:opacity-40">Tiếp</button>
              </nav>
            )}
          </>
        )}
      </div>
    </main>
  );
}
