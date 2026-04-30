import { useState } from 'react';
import { useAuth } from './useAuth';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

export function useWorkspaceAuth({
  runAction,
  setFeedback,
}: {
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const { token, currentUser, health, login: authLogin, register: authRegister, logout: authLogout } = useAuth();
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

  async function handleRegister() {
    await runAction('register', async () => {
      await authRegister(registerForm);
      setRegisterForm({ email: '', password: '', displayName: '', companyName: '' });
      setFeedback({ kind: 'success', text: '注册并登录成功。' });
    });
  }

  async function handleLogin() {
    await runAction('login', async () => {
      await authLogin(loginForm);
      setLoginForm({ email: '', password: '' });
      setFeedback({ kind: 'success', text: '登录成功。' });
    });
  }

  async function handleLogout() {
    await runAction('logout', async () => {
      await authLogout();
      setFeedback({ kind: 'success', text: '已退出登录。' });
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
