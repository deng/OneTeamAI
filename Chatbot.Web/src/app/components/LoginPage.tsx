import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../useAuth';
import { getErrorMessage } from '../workspaceApi';
import { validateEmail, isLoginFormValid, isRegisterFormValid } from '../authValidation';
import { PasswordStrengthIndicator } from './PasswordStrengthIndicator';

export function LoginPage() {
  const { health, login, register } = useAuth();
  const navigate = useNavigate();
  const [busyAction, setBusyAction] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [registerForm, setRegisterForm] = useState({ email: '', password: '', displayName: '', companyName: '' });
  const [loginForm, setLoginForm] = useState({ email: '', password: '' });

  const [regEmailBlurred, setRegEmailBlurred] = useState(false);
  const [regPasswordBlurred, setRegPasswordBlurred] = useState(false);
  const [regDisplayNameBlurred, setRegDisplayNameBlurred] = useState(false);
  const [loginEmailBlurred, setLoginEmailBlurred] = useState(false);
  const [loginPasswordBlurred, setLoginPasswordBlurred] = useState(false);

  const regEmailError = regEmailBlurred ? validateEmail(registerForm.email) : null;
  const loginEmailError = loginEmailBlurred ? validateEmail(loginForm.email) : null;

  async function handleRegister() {
    setBusyAction('register');
    setError(null);
    try {
      await register(registerForm);
      setRegisterForm({ email: '', password: '', displayName: '', companyName: '' });
      navigate('/workspace', { replace: true });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyAction(null);
    }
  }

  async function handleLogin() {
    setBusyAction('login');
    setError(null);
    try {
      await login(loginForm);
      setLoginForm({ email: '', password: '' });
      navigate('/workspace', { replace: true });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusyAction(null);
    }
  }

  return (
    <div className="shell">
      <div className="hero">
        <p className="eyebrow">AI Chatbot</p>
        <h1>虚拟团队工作台</h1>
        <p className="lede">
          登录或注册以进入你的工作空间。
          {health ? null : <span className="status-pill status-pill-warning"> API 未连接</span>}
        </p>
      </div>

      {error ? (
        <div className="status-box status-box-error" style={{ maxWidth: 480, margin: '0 auto 16px' }}>
          <strong>操作失败</strong>
          <span>{error}</span>
        </div>
      ) : null}

      <div className="setup-stack setup-stack-centered">
        <div className="form-card">
          <div className="form-card-title">登录</div>
          <label className="field">
            <span>邮箱</span>
            <input
              className={`text-input${loginEmailError ? ' text-input-error' : ''}`}
              value={loginForm.email}
              onChange={e => setLoginForm(f => ({ ...f, email: e.target.value }))}
              onBlur={() => setLoginEmailBlurred(true)}
            />
            {loginEmailError ? <span className="field-error">{loginEmailError}</span> : null}
          </label>
          <label className="field">
            <span>密码</span>
            <input
              className="text-input"
              type="password"
              value={loginForm.password}
              onChange={e => setLoginForm(f => ({ ...f, password: e.target.value }))}
              onBlur={() => setLoginPasswordBlurred(true)}
            />
          </label>
          <button
            className="secondary-button"
            disabled={busyAction !== null || !isLoginFormValid(loginForm.email, loginForm.password)}
            type="button"
            onClick={handleLogin}
          >
            {busyAction === 'login' ? '登录中...' : '登录'}
          </button>
        </div>

        <div className="form-card">
          <div className="form-card-title">注册</div>
          <label className="field">
            <span>邮箱</span>
            <input
              className={`text-input${regEmailError ? ' text-input-error' : ''}`}
              value={registerForm.email}
              onChange={e => setRegisterForm(f => ({ ...f, email: e.target.value }))}
              onBlur={() => setRegEmailBlurred(true)}
            />
            {regEmailError ? <span className="field-error">{regEmailError}</span> : null}
          </label>
          <label className="field">
            <span>密码</span>
            <input
              className="text-input"
              type="password"
              value={registerForm.password}
              onChange={e => setRegisterForm(f => ({ ...f, password: e.target.value }))}
              onBlur={() => setRegPasswordBlurred(true)}
            />
            <PasswordStrengthIndicator password={registerForm.password} />
          </label>
          <label className="field">
            <span>显示名</span>
            <input
              className="text-input"
              value={registerForm.displayName}
              onChange={e => setRegisterForm(f => ({ ...f, displayName: e.target.value }))}
              onBlur={() => setRegDisplayNameBlurred(true)}
            />
          </label>
          <label className="field">
            <span>公司名</span>
            <input
              className="text-input"
              value={registerForm.companyName}
              onChange={e => setRegisterForm(f => ({ ...f, companyName: e.target.value }))}
            />
          </label>
          <button
            className="secondary-button"
            disabled={busyAction !== null || !isRegisterFormValid(registerForm.email, registerForm.password, registerForm.displayName)}
            type="button"
            onClick={handleRegister}
          >
            {busyAction === 'register' ? '提交中...' : '注册并登录'}
          </button>
        </div>
      </div>
    </div>
  );
}
