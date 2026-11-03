
export type UserRole = 'Learner' | 'Admin';

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  role: UserRole;
  timezone: string;
  avatarUrl?: string;
  dailyGoal: number;
  currentStreak: number;
}

export interface Course {
  id: string;
  languageId: string;
  title: string;
  level: string;
  description: string;
  status: 'Draft' | 'Published';
  unitsCount: number;
  totalLessons: number;
}

export interface Unit {
  id: string;
  courseId: string;
  title: string;
  sortOrder: number;
  lessons: LessonSummary[];
}

export interface LessonSummary {
  id: string;
  title: string;
  sortOrder: number;
  status: 'NotStarted' | 'InProgress' | 'Completed';
  vocabCount: number;
}

export interface Vocabulary {
  id: string;
  lessonId: string;
  term: string;
  phonetic: string;
  meaning: string;
  example: string;
  audioUrl?: string;
  imageUrl?: string;
}

export interface Deck {
  id: string;
  ownerId: string;
  languageId: string;
  title: string;
  description: string;
  visibility: 'Private' | 'Public';
  totalCards: number;
  dueCards: number;
  newCards: number;
  tags: string[];
  version: number;
}

export interface Flashcard {
  id: string;
  deckId: string;
  front: string;
  back: string;
  audioUrl?: string;
  imageUrl?: string;
  version: number;
}

export type SrsRating = 'Again' | 'Hard' | 'Good' | 'Easy';

export interface ReviewEvent {
  clientEventId: string;
  cardId: string;
  rating: SrsRating;
  reviewedAt: string;
  algorithmVersion: string;
}

export interface SyncOperation {
  operationId: string;
  type: string;
  entityId: string;
  payload: Record<string, unknown>;
  occurredAt: string;
  status: 'Pending' | 'Synced' | 'Failed';
}

export interface OfflineCardSchedule {
  cardId: string;
  deckId: string;
  dueAt: string;
  stateJson: string;
  algorithmVersion: string;
  lastReviewedAt: string | null;
}
