
import {
  useEffect,
  useState,
  useSyncExternalStore,
} from 'react';

import { useQueryClient } from '@tanstack/react-query';

import { db } from '../../offline/db';
import { refreshSession } from '../../services/apiClient';
import { authApi } from './api/authApi';
import { AuthContext } from './authContext';

import {
  clearAuthSession,
  getAuthSnapshot,
  getOfflineUser,
  getRefreshToken,
  saveAuthSession,
  subscribeToAuth,
} from './authSession';

import type { ReactNode } from 'react';
import type {
  AuthUser,
  LoginRequest,
  RegisterRequest,
} from './types/auth.types';

export function AuthProvider({
  children,
}: {
  children: ReactNode;
}) {
  const queryClient = useQueryClient();

  const session = useSyncExternalStore(
    subscribeToAuth,
    getAuthSnapshot,
    getAuthSnapshot
  );

  const [isRestoring, setIsRestoring] = useState(
    () => getRefreshToken() !== null
  );

  const [offlineUser, setOfflineUser] =
    useState<AuthUser | null>(null);

  useEffect(() => {
    let active = true;

    try {
      window.localStorage.removeItem('access_token');
    } catch {}

    if (getRefreshToken()) {
      void refreshSession()
        .then((restored) => {
          if (!active) return;

          if (!restored && !navigator.onLine) {
            setOfflineUser(getOfflineUser());
          }
        })
        .finally(() => {
          if (active) {
            setIsRestoring(false);
          }
        });
    } else {
      setIsRestoring(false);
    }

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    const handleOffline = () => {
      if (!session.user && getRefreshToken()) {
        setOfflineUser(getOfflineUser());
      }
    };

    const handleOnline = () => {
      setOfflineUser(null);

      if (!session.user && getRefreshToken()) {
        setIsRestoring(true);

        void refreshSession().finally(() => {
          setIsRestoring(false);
        });
      }
    };

    window.addEventListener('offline', handleOffline);
    window.addEventListener('online', handleOnline);

    return () => {
      window.removeEventListener('offline', handleOffline);
      window.removeEventListener('online', handleOnline);
    };
  }, [session.user]);

  useEffect(() => {
    if (!session.user) {
      queryClient.clear();
    } else {
      setOfflineUser(null);
    }
  }, [queryClient, session.user]);

  async function login(request: LoginRequest) {
    const response = await authApi.login(request);

    queryClient.clear();
    saveAuthSession(response);
    setOfflineUser(null);
  }

  async function register(request: RegisterRequest) {
    const response = await authApi.register(request);

    queryClient.clear();
    saveAuthSession(response);
    setOfflineUser(null);
  }

  async function logout(): Promise<{
    revoked: boolean;
    offlineCleared: boolean;
  }> {
    const unsyncedCount = await db.syncQueue
      .where('status')
      .anyOf('Pending', 'Failed')
      .count();

    if (unsyncedCount > 0) {
      throw new Error(
        `Không thể đăng xuất khi còn ${unsyncedCount} thao tác chưa đồng bộ.`
      );
    }

    const refreshToken = getRefreshToken();

    let revoked = true;
    let offlineCleared = true;

    try {
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    } catch {
      revoked = false;
    }

    clearAuthSession();
    setOfflineUser(null);
    queryClient.clear();

    try {
      await db.transaction(
        'rw',
        db.tables,
        async () => {
          for (const table of db.tables) {
            await table.clear();
          }
        }
      );
    } catch {
      offlineCleared = false;
    }

    return {
      revoked,
      offlineCleared,
    };
  }

  return (
    <AuthContext.Provider
      value={{
        ...session,
        isRestoring,
        offlineUser,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
