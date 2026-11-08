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
