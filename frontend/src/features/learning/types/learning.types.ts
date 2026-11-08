export interface QuizQuestion {
  id: string;
  type: 'MultipleChoice' | 'FillInBlank' | 'Matching';
  prompt: string;
  options: { id: string; content: string }[];
  left: { id: string; text: string }[];
  right: { id: string; text: string }[];
}
export type QuizAnswer = string | Record<string, string>;
export interface QuizResult {
  id: string;
  lessonId: string;
  score: number;
  maxScore: number;
  completedAt: string;
  answers: { questionId: string; isCorrect: boolean; explanation: string; correctAnswer: string }[];
}
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
export interface ActivityDay {
  date: string;
  reviewCount: number;
  lessonsCompleted: number;
  goalMet: boolean;
}
export interface StatisticsOverview {
  newCards: number;
  learnedCards: number;
  dueCards: number;
  reviewCount: number;
  retention: number;
  ratings: { rating: string; count: number }[];
}
export interface Streak {
  currentStreak: number;
  longestStreak: number;
  dailyTarget: number;
  todayReviews: number;
  timezone: string;
  days: ActivityDay[];
}
export interface CsvPreview {
  validCount: number;
  invalidCount: number;
  rows: { line: number; front: string; back: string; error: string | null }[];
}
export interface PublicDeck {
  id: string;
  languageId: string;
  title: string;
  description: string;
  cardCount: number;
  tags: string[];
}
export interface PublicDeckPreview {
  deck: PublicDeck;
  cards: { front: string; back: string }[];
}
export interface Paged<T> {
  items: T[];
  page: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
