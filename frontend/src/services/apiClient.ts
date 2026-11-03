import {
  clearAuthSession,
  getAuthSnapshot,
  getRefreshToken,
  getSessionVersion,
  saveAuthSession,
} from '../features/auth/authSession';

import type { AuthResponse } from '../features/auth/types/auth.types';

const API_BASE_URL = (
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1'
).replace(/\/$/, '');

export class ApiError extends Error {
  status: number;
  data: unknown;

  constructor(message: string, status: number, data: unknown) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
  }
}

interface RequestOptions {
  anonymous?: boolean;
}

async function parseResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const data: unknown = await response.json().catch(() => null);
    const detail =
      typeof data === 'object' && data !== null &&
      'detail' in data && typeof data.detail === 'string'
        ? data.detail
        : response.statusText || 'API request failed';

    throw new ApiError(detail, response.status, data);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

async function sendRequest(
  endpoint: string,
  options: RequestInit,
  accessToken: string | null
): Promise<Response> {
  const headers = new Headers(options.headers);

  if (options.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  return fetch(`${API_BASE_URL}${endpoint}`, { ...options, headers });
}

let refreshPromise: Promise<boolean> | null = null;

export function refreshSession(): Promise<boolean> {
  if (refreshPromise) {
    return refreshPromise;
  }

  const refreshToken = getRefreshToken();
  const version = getSessionVersion();

  if (!refreshToken) {
    return Promise.resolve(false);
  }

  refreshPromise = (async () => {
    try {
      const response = await sendRequest(
        '/auth/refresh',
        {
          method: 'POST',
          body: JSON.stringify({ refreshToken }),
        },
        null
      );

      const session = await parseResponse<AuthResponse>(response);

      if (getSessionVersion() !== version) {
        return false;
      }

      saveAuthSession(session);
      return true;
    } catch (error) {
      const isInvalidToken =
        error instanceof ApiError &&
        [400, 401, 403].includes(error.status);

      if (
        isInvalidToken &&
        getSessionVersion() === version
      ) {
        clearAuthSession();
      }

      return false;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {},
  requestOptions: RequestOptions = {}
): Promise<T> {
  const accessToken = requestOptions.anonymous
    ? null
    : getAuthSnapshot().accessToken;
  const version = getSessionVersion();
  let sentAccessToken = accessToken;
  let response = await sendRequest(endpoint, options, accessToken);

  if (response.status === 401 && !requestOptions.anonymous) {
    if (getSessionVersion() !== version) {
      return parseResponse<T>(response);
    }
    const refreshed = await refreshSession();

    if (refreshed) {
      sentAccessToken = getAuthSnapshot().accessToken;
      response = await sendRequest(
        endpoint,
        options,
        sentAccessToken
      );
    }
  }

  if (response.status === 401 && !requestOptions.anonymous &&
      getAuthSnapshot().accessToken === sentAccessToken) {
    clearAuthSession();
  }

  return parseResponse<T>(response);
}
