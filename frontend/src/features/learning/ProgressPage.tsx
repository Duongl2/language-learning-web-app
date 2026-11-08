import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { learningApi } from './api/progressApi';
import { LearningLayout, QueryStatus } from './components/LearningLayout';

export function ProgressPage() {
  const progress = useQuery({ queryKey: ['learning-progress'], queryFn: learningApi.progress });
  return <LearningLayout title="Tiến độ học tập">
    <QueryStatus pending={progress.isPending} error={progress.error} empty={progress.data?.courses.length === 0} onRetry={() => { void progress.refetch(); }} />
    {progress.data?.resume && <Link className="block rounded-xl bg-rose-700 p-5 font-bold text-white" to={`/lessons/${progress.data.resume.lessonId}`}>Tiếp tục học: {progress.data.resume.title}</Link>}
    {progress.data?.courses.map(course => <section key={course.courseId} className="space-y-3 rounded-xl border-2 border-stone-900 bg-white p-5">
      <Link to={`/courses/${course.courseId}`} className="text-xl font-bold">{course.title}</Link>
      <p>{course.completedLessons}/{course.totalLessons} bài hoàn thành — {course.percent}%</p>
      <progress aria-label={`Tiến độ ${course.title}`} className="w-full" max={100} value={course.percent} />
      {course.lessons.map(lesson => <Link className="flex justify-between border-t py-2" key={lesson.lessonId} to={`/lessons/${lesson.lessonId}`}><span>{lesson.title}</span><span>{lesson.status === 'Completed' ? 'Đã hoàn thành' : lesson.status === 'InProgress' ? 'Đang học' : 'Chưa học'}</span></Link>)}
    </section>)}
    <Link to="/courses" className="inline-block font-bold text-rose-700">Khám phá khóa học</Link>
  </LearningLayout>;
}
