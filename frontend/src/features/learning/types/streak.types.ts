export interface ActivityDay {
  date: string;
  reviewCount: number;
  lessonsCompleted: number;
  goalMet: boolean;
}

export interface Streak {
  currentStreak: number;
  longestStreak: number;
  dailyTarget: number;
  todayReviews: number;
  timezone: string;
  days: ActivityDay[];
}
