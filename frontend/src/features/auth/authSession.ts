
import type {
  AuthResponse,
  AuthSnapshot,
  AuthUser,
} from './types/auth.types';

const REFRESH_TOKEN_KEY = 'auth_refresh_token';
const LEGACY_ACCESS_TOKEN_KEY = 'access_token';
const OFFLINE_USER_KEY = 'auth_offline_user';

let snapshot: AuthSnapshot = {
  accessToken: null,
  user: null,
};

let sessionVersion = 0;

const listeners = new Set<() => void>();

function notify(): void {
  listeners.forEach((listener) => listener());
}

function getStorage(): Storage | null {
  return typeof window === 'undefined'
    ? null
    : window.sessionStorage;
}

function parseUser(accessToken: string): AuthUser {
  const payloadPart = accessToken.split('.')[1];

  if (!payloadPart) {
    throw new Error('Invalid access token.');
  }

  const encoded = payloadPart
    .replace(/-/g, '+')
    .replace(/_/g, '/');

  const padded = encoded.padEnd(
    Math.ceil(encoded.length / 4) * 4,
    '='
  );

  const payload: unknown = JSON.parse(
    new TextDecoder().decode(
      Uint8Array.from(
        atob(padded),
        (character) => character.charCodeAt(0)
      )
    )
  );

  if (
    typeof payload !== 'object' ||
    payload === null ||
    !('sub' in payload) ||
    typeof payload.sub !== 'string' ||
    !('email' in payload) ||
    typeof payload.email !== 'string' ||
    !('role' in payload) ||
    (payload.role !== 'Learner' &&
      payload.role !== 'Admin') ||
    !('exp' in payload) ||
    typeof payload.exp !== 'number' ||
    payload.exp * 1000 <= Date.now()
  ) {
    throw new Error('Invalid access token.');
  }

  return {
    id: payload.sub,
    email: payload.email,
    role: payload.role,
  };
}

export function getAuthSnapshot(): AuthSnapshot {
  return snapshot;
}

export function getSessionVersion(): number {
  return sessionVersion;
}

export function subscribeToAuth(
  listener: () => void
): () => void {
  listeners.add(listener);

  return () => listeners.delete(listener);
}

export function getRefreshToken(): string | null {
  try {
    return getStorage()?.getItem(REFRESH_TOKEN_KEY) ?? null;
  } catch {
    return null;
  }
}

export function saveAuthSession(
  response: AuthResponse
): void {
  const user = parseUser(response.accessToken);

  if (!response.refreshToken) {
    throw new Error('Invalid refresh token.');
  }

  try {
    const storage = getStorage();

    storage?.setItem(
      REFRESH_TOKEN_KEY,
      response.refreshToken
    );

    storage?.setItem(
      OFFLINE_USER_KEY,
      JSON.stringify(user)
    );
  } catch {

  }

  snapshot = {
    accessToken: response.accessToken,
    user,
  };

  sessionVersion++;
  notify();
}

export function getOfflineUser(): AuthUser | null {
  if (!getRefreshToken()) {
    return null;
  }

  try {
    const value = getStorage()?.getItem(
      OFFLINE_USER_KEY
    );

    if (!value) {
      return null;
    }

    const user: unknown = JSON.parse(value);

    if (
      typeof user !== 'object' ||
      user === null ||
      !('id' in user) ||
      typeof user.id !== 'string' ||
      !user.id ||
      !('email' in user) ||
      typeof user.email !== 'string' ||
      !user.email ||
      !('role' in user) ||
      (user.role !== 'Learner' &&
        user.role !== 'Admin')
    ) {
      return null;
    }

    return {
      id: user.id,
      email: user.email,
      role: user.role,
    };
  } catch {
    return null;
  }
}

export function clearAuthSession(): void {
  try {
    getStorage()?.removeItem(REFRESH_TOKEN_KEY);
  } catch {

  }

  try {
    getStorage()?.removeItem(OFFLINE_USER_KEY);
  } catch {

  }

  try {
    if (typeof window !== 'undefined') {
      window.localStorage.removeItem(
        LEGACY_ACCESS_TOKEN_KEY
      );
    }
  } catch {

  }

  snapshot = {
    accessToken: null,
    user: null,
  };

  sessionVersion++;
  notify();
}