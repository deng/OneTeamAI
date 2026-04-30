import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { HealthResponse, LoginRequest, RegisterRequest, UserResponse } from '../generated/api';
import { authStorageKey } from './constants';
import { applyAuth, createWorkspaceApis, fetchJson } from './workspaceApi';

export type AuthState = {
  token: string | null;
  currentUser: UserResponse | null;
  health: HealthResponse | null;
  login: (form: LoginRequest) => Promise<void>;
  register: (form: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
};

export const AuthContext = createContext<AuthState>(null!);

function clearToken() {
  window.localStorage.removeItem(authStorageKey);
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => window.localStorage.getItem(authStorageKey));
  const [currentUser, setCurrentUser] = useState<UserResponse | null>(null);
  const [health, setHealth] = useState<HealthResponse | null>(null);
  const { authApi } = useMemo(() => createWorkspaceApis(token), [token]);

  useEffect(() => {
    void (async () => {
      try {
        setHealth(await fetchJson<HealthResponse>('/health'));
      } catch {
        setHealth(null);
      }
    })();
  }, []);

  useEffect(() => {
    if (!token) {
      setCurrentUser(null);
      return;
    }

    let cancelled = false;

    void (async () => {
      try {
        const user = await authApi.getCurrentUser();
        if (!cancelled) {
          setCurrentUser(user);
        }
      } catch {
        if (!cancelled) {
          clearToken();
          setToken(null);
          setCurrentUser(null);
        }
      }
    })();

    return () => { cancelled = true; };
  }, [authApi, token]);

  const refreshUser = useCallback(async () => {
    if (!token) {
      return;
    }
    try {
      const user = await authApi.getCurrentUser();
      setCurrentUser(user);
    } catch {
      clearToken();
      setToken(null);
      setCurrentUser(null);
    }
  }, [authApi, token]);

  const login = useCallback(async (form: LoginRequest) => {
    const { authApi: api } = createWorkspaceApis(null);
    const auth = await api.login({ loginRequest: form });
    const accessToken = applyAuth(auth);
    setToken(accessToken);
    setCurrentUser(auth.user ?? null);
  }, []);

  const register = useCallback(async (form: RegisterRequest) => {
    const { authApi: api } = createWorkspaceApis(null);
    const auth = await api.register({ registerRequest: form });
    const accessToken = applyAuth(auth);
    setToken(accessToken);
    setCurrentUser(auth.user ?? null);
  }, []);

  const logout = useCallback(async () => {
    try {
      if (token) {
        await authApi.logout();
      }
    } catch {
      // ignore logout errors
    }
    clearToken();
    setToken(null);
    setCurrentUser(null);
  }, [authApi, token]);

  return (
    <AuthContext.Provider value={{ token, currentUser, health, login, register, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}
