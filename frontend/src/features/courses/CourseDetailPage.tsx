import { useQuery } from '@tanstack/react-query';
import { BookOpen, ChevronRight, Play } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';

import { coursesApi } from './api/coursesApi';

export function CourseDetailPage() {
  const { courseId = '' } = useParams<{ courseId: string }>();
  const course = useQuery({ queryKey: ['course', courseId], queryFn: () => coursesApi.get(courseId), enabled: Boolean(courseId) });
  const units = useQuery({ queryKey: ['course-units', courseId], queryFn: () => coursesApi.unitsWithLessons(courseId), enabled: Boolean(courseId) && course.isSuccess });
  const firstLesson = units.data?.flatMap((unit) => unit.lessons)[0];

  return (
    <main className="graph-paper min-h-screen px-4 py-8 text-stone-900 sm:px-8">
      <div className="mx-auto max-w-4xl">
        <Link to="/courses" className="text-sm font-bold text-rose-700">← Danh mục khóa học</Link>
        {(course.isPending || units.isPending && course.isSuccess) && <p role="status" className="mt-8">Đang tải lộ trình...</p>}
        {course.isError && <p role="alert" className="mt-8 rounded-lg bg-red-50 p-4 text-red-700">Không tải được khóa học: {course.error.message}</p>}
        {units.isError && <p role="alert" className="mt-8 rounded-lg bg-red-50 p-4 text-red-700">Không tải được lộ trình: {units.error.message}</p>}
        {course.data && (
          <>
            <header className="mt-6 rounded-2xl border-2 border-stone-900 bg-white p-6 shadow-[5px_5px_0_#262626]">
              <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-extrabold text-amber-800">{course.data.level}</span>
              <h1 className="mt-4 text-3xl font-extrabold">{course.data.title}</h1>
              <p className="mt-3 text-stone-600">{course.data.description || 'Lộ trình học theo từng chương và bài học.'}</p>
              {firstLesson && <Link to={`/lessons/${firstLesson.id}`} className="mt-6 inline-flex items-center gap-2 rounded-lg border-2 border-stone-900 bg-rose-500 px-5 py-3 font-extrabold text-white hover:bg-rose-600"><Play size={18} aria-hidden="true" /> Bắt đầu học</Link>}
            </header>
            {units.data && (
              <section className="mt-8 space-y-5" aria-label="Lộ trình khóa học">
                {units.data.length === 0 && <p className="rounded-xl border-2 border-dashed border-stone-300 bg-white p-6 text-stone-600">Khóa học chưa có chương được xuất bản.</p>}
                {units.data.map((unit, index) => (
                  <article key={unit.id} className="rounded-xl border-2 border-stone-900 bg-white p-5 shadow-[3px_3px_0_#262626]">
                    <div className="flex items-center gap-3"><span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-rose-100 font-extrabold text-rose-700">{index + 1}</span><h2 className="text-xl font-extrabold">{unit.title}</h2></div>
                    {unit.lessons.length === 0 ? <p className="mt-4 pl-12 text-sm text-stone-500">Chưa có bài học được xuất bản.</p> : (
                      <ol className="mt-4 space-y-2">
                        {unit.lessons.map((lesson, lessonIndex) => (
                          <li key={lesson.id}>
                            <Link to={`/lessons/${lesson.id}`} className="flex items-center gap-3 rounded-lg border border-stone-200 p-3 hover:border-rose-400 hover:bg-rose-50">
                              <BookOpen size={18} className="text-rose-600" aria-hidden="true" />
                              <span className="flex-1 text-sm font-semibold">{lessonIndex + 1}. {lesson.title}</span>
                              <span className="text-xs font-bold text-rose-700">Bắt đầu học</span><ChevronRight size={17} aria-hidden="true" />
                            </Link>
                          </li>
                        ))}
                      </ol>
                    )}
                  </article>
                ))}
              </section>
            )}
          </>
        )}
      </div>
    </main>
  );
}
