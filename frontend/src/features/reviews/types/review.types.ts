
export interface ReviewIntervalsResponse {
  againMinutes: number;
  hardMinutes: number;
  goodMinutes: number;
  easyMinutes: number;
}

export interface DueCardResponse {
  cardId: string;
  deckId: string;
  front: string;
  back: string;
  dueAt: string;
  algorithmVersion: string;
  intervals: ReviewIntervalsResponse | null;
}
