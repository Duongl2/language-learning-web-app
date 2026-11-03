import { apiRequest } from '../../../services/apiClient';

import type { Course, CourseUnit, Lesson, PagedResponse, Unit } from '../types/course.types';

async function allPages<T>(endpoint: string): Promise<T[]> {
  const items: T[] = [];
  let page = 1;
  while (true) {
    const separator = endpoint.includes('?') ? '&' : '?';
    const response = await apiRequest<PagedResponse<T>>(`${endpoint}${separator}page=${page}&pageSize=50`);
    items.push(...response.items);
    if (!response.hasNextPage) return items;
    page += 1;
  }
}

export const coursesApi = {
  list(page: number, languageId: string, level: string): Promise<PagedResponse<Course>> {
    const params = new URLSearchParams({ page: String(page), pageSize: '12' });
    if (languageId) params.set('languageId', languageId);
    if (level) params.set('level', level);
    return apiRequest<PagedResponse<Course>>(`/courses?${params.toString()}`);
  },
  get(id: string): Promise<Course> {
    return apiRequest<Course>(`/courses/${encodeURIComponent(id)}`);
  },
  async unitsWithLessons(courseId: string): Promise<CourseUnit[]> {
    const units = await allPages<Unit>(`/courses/${encodeURIComponent(courseId)}/units`);
    return Promise.all(units.map(async (unit) => ({
      ...unit,
      lessons: await allPages<Lesson>(`/units/${encodeURIComponent(unit.id)}/lessons`),
    })));
  },
  lesson(id: string): Promise<Lesson> {
    return apiRequest<Lesson>(`/lessons/${encodeURIComponent(id)}`);
  },
};
