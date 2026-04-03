import { useEffect, useMemo, useState } from 'react';
import type { HealthResponse, UserResponse } from '../generated/api';
import { authStorageKey } from './constants';
import { applyAuth, createWorkspaceApis, fetchJson, getErrorMessage } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

export function useWorkspaceAuth({
  runAction,
  setFeedback,
}: {
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [token, setToken] = useState<string | null>(() => window.localStorage.getItem(authStorageKey));
  const [currentUser, setCurrentUser] = useState<UserResponse | null>(null);
  const [health, setHealth] = useState<HealthResponse | null>(null);
  const [registerForm, setRegisterForm] = useState({
    email: '',
    password: '',
    displayName: '',
    companyName: '',
  });
  const [loginForm, setLoginForm] = useState({
    email: '',
    password: '',
  });

  const { authApi } = useMemo(() => createWorkspaceApis(token), [token]);

  useEffect(() => {
    void (async () => {
      try {
        setHealth(await fetchJson<HealthResponse>('/health'));
      } catch (error) {
        setHealth(null);
        setFeedback({
          kind: 'error',
          text: getErrorMessage(error),
        });
      }
    })();
  }, [setFeedback]);

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
      } catch (error) {
        if (!cancelled) {
          clearAuth();
          setFeedback({
            kind: 'error',
            text: getErrorMessage(error),
          });
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [authApi, setFeedback, token]);

  function clearAuth() {
    window.localStorage.removeItem(authStorageKey);
    setToken(null);
    setCurrentUser(null);
  }

  async function handleRegister() {
    await runAction('register', async () => {
      const auth = await authApi.register({
        registerRequest: registerForm,
      });
      const accessToken = applyAuth(auth);

      setToken(accessToken);
      setCurrentUser(auth.user ?? null);
      setRegisterForm({
        email: '',
        password: '',
        displayName: '',
        companyName: '',
      });
      setFeedback({
        kind: 'success',
        text: '注册并登录成功。',
      });
    });
  }

  async function handleLogin() {
    await runAction('login', async () => {
      const auth = await authApi.login({
        loginRequest: loginForm,
      });
      const accessToken = applyAuth(auth);

      setToken(accessToken);
      setCurrentUser(auth.user ?? null);
      setLoginForm({
        email: '',
        password: '',
      });
      setFeedback({
        kind: 'success',
        text: '登录成功。',
      });
    });
  }

  async function handleLogout() {
    await runAction('logout', async () => {
      if (token) {
        await authApi.logout();
      }

      clearAuth();
      setFeedback({
        kind: 'success',
        text: '已退出登录。',
      });
    });
  }

  return {
    currentUser,
    health,
    loginForm,
    registerForm,
    setLoginForm,
    setRegisterForm,
    token,
    handleLogin,
    handleLogout,
    handleRegister,
  };
}
