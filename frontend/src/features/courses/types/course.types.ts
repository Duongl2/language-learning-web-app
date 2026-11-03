export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface Course {
  id: string;
  languageId: string;
  title: string;
  level: string;
  description: string;
  status: number;
}

export interface Unit {
  id: string;
  courseId: string;
  title: string;
  sortOrder: number;
}

export interface Lesson {
  id: string;
  unitId: string;
  title: string;
  content: string;
  sortOrder: number;
  status: number;
}

export interface CourseUnit extends Unit {
  lessons: Lesson[];
}
