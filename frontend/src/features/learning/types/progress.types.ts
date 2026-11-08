export interface CourseProgress {
  courseId: string;
  title: string;
  percent: number;
  completedLessons: number;
  totalLessons: number;
  lessons: { lessonId: string; title: string; status: string }[];
}

export interface ProgressResponse {
  courses: CourseProgress[];
  resume: { lessonId: string; title: string; courseId: string } | null;
}
