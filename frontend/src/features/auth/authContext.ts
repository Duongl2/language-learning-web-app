
import { createContext, useContext } from 'react';

import type {
  AuthSnapshot,
  AuthUser,
  LoginRequest,
  RegisterRequest,
} from './types/auth.types';

export interface AuthContextValue extends AuthSnapshot {
  isRestoring: boolean;
  offlineUser: AuthUser | null;
  login(request: LoginRequest): Promise<void>;
  register(request: RegisterRequest): Promise<void>;
  logout(): Promise<{
    revoked: boolean;
    offlineCleared: boolean;
  }>;
}

export const AuthContext =
  createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('AuthProvider is required.');
  }

  return context;
}
