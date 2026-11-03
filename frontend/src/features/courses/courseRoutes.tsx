import { lazy } from 'react';

export const CourseCatalogPage = lazy(() =>
  import('./CourseCatalogPage').then((module) => ({ default: module.CourseCatalogPage }))
);

export const CourseDetailPage = lazy(() =>
  import('./CourseDetailPage').then((module) => ({ default: module.CourseDetailPage }))
);

export const LessonPage = lazy(() =>
  import('./LessonPage').then((module) => ({ default: module.LessonPage }))
);
