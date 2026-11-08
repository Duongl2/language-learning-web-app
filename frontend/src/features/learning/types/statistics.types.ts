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
