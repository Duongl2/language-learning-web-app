import { useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, ArrowRight, BookOpen, List } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';

import { coursesApi } from './api/coursesApi';
import { learningApi } from '../learning/api/progressApi';

export function LessonPage() {
  const { lessonId = '' } = useParams<{ lessonId: string }>();
  const lesson = useQuery({
    queryKey: ['lesson-viewer', lessonId],
    queryFn: () => coursesApi.lessonViewer(lessonId),
    enabled: Boolean(lessonId),
  });
  const cache = useQueryClient();
  const update = useMutation({
    mutationFn: (status: string) => learningApi.updateProgress(lessonId, status),
    onSuccess: () => {
      void cache.invalidateQueries({ queryKey: ['learning-progress'] });
    },
  });
  const { mutate: markProgress } = update;
  useEffect(() => {
    if (lesson.isSuccess) markProgress('InProgress');
  }, [lessonId, lesson.isSuccess, markProgress]);

  return (
    <main className="graph-paper min-h-screen px-4 py-8 text-stone-900 sm:px-8">
      <div className="mx-auto max-w-4xl">
        {lesson.data && (
          <div className="mb-6 flex flex-wrap gap-4">
            <button disabled={update.isPending} onClick={() => update.mutate('Completed')} className="rounded border-2 px-4 py-3 font-bold">Đánh dấu hoàn thành</button>
            <Link to="/learning" className="p-3 font-bold">Xem tiến độ</Link>
          </div>
        )}
        {update.isError && <p role="alert">{update.error.message}</p>}
        {update.isSuccess && update.variables === 'Completed' && <p role="status">Đã lưu hoàn thành bài học.</p>}
        {lesson.isPending && <p role="status" className="rounded-xl bg-white p-6">Đang tải bài học...</p>}
        {lesson.isError && (
          <div role="alert" className="rounded-xl border border-red-200 bg-red-50 p-5 text-red-700">
            <p>Không tải được bài học: {lesson.error.message}</p>
            <Link to="/courses" className="mt-3 inline-block font-bold underline">Về danh mục khóa học</Link>
          </div>
        )}
        {lesson.data && (
          <>
            <nav aria-label="Đường dẫn bài học" className="mb-5 flex flex-wrap items-center gap-2 text-sm font-semibold text-stone-600">
              <Link to="/courses" className="hover:text-rose-700">Khóa học</Link>
              <span aria-hidden="true">/</span>
              <Link to={`/courses/${lesson.data.courseId}`} className="hover:text-rose-700">{lesson.data.courseTitle}</Link>
              <span aria-hidden="true">/</span>
              <span>{lesson.data.unitTitle}</span>
            </nav>

            <article className="rounded-2xl border-2 border-stone-900 bg-white p-6 shadow-[5px_5px_0_#262626] sm:p-8">
              <p className="flex items-center gap-2 text-xs font-extrabold uppercase tracking-widest text-rose-600"><BookOpen size={16} aria-hidden="true" /> Bài học</p>
              <h1 className="mt-3 text-3xl font-extrabold">{lesson.data.title}</h1>
              <section aria-labelledby="lesson-theory-heading" className="mt-8">
                <h2 id="lesson-theory-heading" className="text-xl font-extrabold">Lý thuyết và nội dung</h2>
                <div className="mt-4 whitespace-pre-wrap break-words leading-8 text-stone-700">
                  {lesson.data.content || 'Nội dung bài học đang được cập nhật.'}
                </div>
              </section>
            </article>

            <section aria-labelledby="lesson-vocabulary-heading" className="mt-7 rounded-2xl border-2 border-stone-900 bg-white p-6 shadow-[4px_4px_0_#262626] sm:p-8">
              <h2 id="lesson-vocabulary-heading" className="text-xl font-extrabold">Từ vựng liên quan</h2>
              <p className="mt-1 text-sm text-stone-500">{lesson.data.vocabulary.length} từ vựng</p>
              {lesson.data.vocabulary.length === 0 ? (
                <p className="mt-5 rounded-lg bg-stone-50 p-4 text-sm text-stone-600">Bài học này chưa có từ vựng.</p>
              ) : (
                <ul className="mt-5 grid gap-4 sm:grid-cols-2">
                  {lesson.data.vocabulary.map((word) => (
                    <li key={word.id} className="rounded-xl border-2 border-stone-200 p-4">
                      <div className="flex items-start justify-between gap-2">
                        <div><h3 className="text-lg font-extrabold">{word.term}</h3>{word.phonetic && <p className="text-sm text-stone-500">/{word.phonetic}/</p>}</div>
                        {word.audioUrl && <audio controls preload="none" aria-label={`Phát âm ${word.term}`} src={word.audioUrl} className="h-8 w-32" />}
                      </div>
                      <p className="mt-3 font-semibold text-rose-700">{word.meaning}</p>
                      {word.example && <p className="mt-2 text-sm italic text-stone-600">{word.example}</p>}
                      {word.imageUrl && <img src={word.imageUrl} alt={`Minh họa ${word.term}`} loading="lazy" className="mt-3 max-h-44 rounded-lg object-contain" />}
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <nav aria-label="Điều hướng bài học" className="mt-7 flex flex-wrap items-center justify-between gap-3">
              {lesson.data.previousLessonId ? (
                <Link to={`/lessons/${lesson.data.previousLessonId}`} className="inline-flex items-center gap-2 rounded-lg border-2 border-stone-900 bg-white px-4 py-3 text-sm font-bold"><ArrowLeft size={17} aria-hidden="true" /> Bài trước</Link>
              ) : <span />}
              <Link to={`/courses/${lesson.data.courseId}`} className="inline-flex items-center gap-2 text-sm font-bold text-stone-600 hover:text-rose-700"><List size={18} aria-hidden="true" /> Lộ trình</Link>
              {lesson.data.nextLessonId ? (
                <Link to={`/lessons/${lesson.data.nextLessonId}`} className="inline-flex items-center gap-2 rounded-lg border-2 border-stone-900 bg-rose-500 px-4 py-3 text-sm font-bold text-white"><span>Bài tiếp theo</span><ArrowRight size={17} aria-hidden="true" /></Link>
              ) : (
                <Link to="/?view=quiz" className="inline-flex items-center gap-2 rounded-lg border-2 border-stone-900 bg-rose-500 px-4 py-3 text-sm font-bold text-white">Chuyển sang làm Quiz luyện tập chung <ArrowRight size={17} aria-hidden="true" /></Link>
              )}
            </nav>
            <Link to={`/lessons/${lessonId}/quiz`} className="mt-6 inline-block rounded bg-rose-700 px-4 py-3 font-bold text-white">Làm Quiz</Link>
          </>
        )}
      </div>
    </main>
  );
}