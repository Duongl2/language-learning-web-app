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
