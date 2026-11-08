import { useQuery } from '@tanstack/react-query';
import { Link, useParams } from 'react-router-dom';
import { coursesApi } from './api/coursesApi';

export function LessonPage() {
  const { lessonId = '' } = useParams<{ lessonId: string }>();
  const lesson = useQuery({ queryKey: ['lesson', lessonId], queryFn: () => coursesApi.lesson(lessonId), enabled: Boolean(lessonId) });

  return (
    <main className="graph-paper min-h-screen px-4 py-8 text-stone-900 sm:px-8">
      <div className="mx-auto max-w-3xl rounded-2xl border-2 border-stone-900 bg-white p-6 shadow-[5px_5px_0_#262626]">
        <Link to="/courses" className="text-sm font-bold text-rose-700">← Danh mục khóa học</Link>
        {lesson.isPending && <p role="status" className="mt-6">Đang tải bài học...</p>}
        {lesson.isError && <p role="alert" className="mt-6 text-red-700">Không tải được bài học: {lesson.error.message}</p>}
        {lesson.data && <article className="mt-6"><h1 className="text-3xl font-extrabold">{lesson.data.title}</h1><div className="mt-5 whitespace-pre-wrap text-base leading-7 text-stone-700">{lesson.data.content || 'Nội dung bài học đang được cập nhật.'}</div></article>}
        {lesson.data && <Link to={`/lessons/${lessonId}/quiz`} className="mt-6 inline-block rounded bg-rose-700 px-4 py-3 font-bold text-white">Làm Quiz</Link>}
      </div>
    </main>
  );
}
