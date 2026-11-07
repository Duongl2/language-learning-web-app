
export interface DeckResponse {
  id: string;
  ownerId: string;
  languageId: string;
  title: string;
  description: string;
  isPublic: boolean;
  version: number;
  tags: string[];
  totalCards: number;
  newCards: number;
  dueCards: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  search?: string;
}

export interface CreateDeckRequest {
  title: string;
  description: string;
  languageId: string;
  isPublic: boolean;
  tags: string[];
}

export interface UpdateDeckRequest {
  baseVersion: number;
  title?: string;
  description?: string;
  languageId?: string;
  isPublic?: boolean;
  tags?: string[];
}
