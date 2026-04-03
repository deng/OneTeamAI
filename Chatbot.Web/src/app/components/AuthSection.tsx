import type { Dispatch, SetStateAction } from 'react';
import type { HealthResponse, UserResponse } from '../../generated/api';
import { formatNullableText } from '../formatters';
import type { Feedback } from '../types';

type RegisterForm = {
  email: string;
  password: string;
  displayName: string;
  companyName: string;
};

type LoginForm = {
  email: string;
  password: string;
};

type AuthSectionProps = {
  busyAction: string | null;
  currentUser: UserResponse | null;
  feedback: Feedback | null;
  health: HealthResponse | null;
  loginForm: LoginForm;
  registerForm: RegisterForm;
  onLogin: () => void;
  onLogout: () => void;
  onRegister: () => void;
  onLoginFormChange: Dispatch<SetStateAction<LoginForm>>;
  onRegisterFormChange: Dispatch<SetStateAction<RegisterForm>>;
};

export function AuthSection({
  busyAction,
  currentUser,
  feedback,
  health,
  loginForm,
  registerForm,
  onLogin,
  onLogout,
  onRegister,
  onLoginFormChange,
  onRegisterFormChange,
}: AuthSectionProps) {
  return (
    <>
      <div className="panel-title">系统状态</div>
      <div className="meta-item">
        <span>API 服务</span>
        <strong>{health ? '可连接' : '未连接'}</strong>
      </div>
      {health ? (
        <div className="meta-item">
          <span>健康状态</span>
          <strong>{formatNullableText(health.status, '未知')}</strong>
        </div>
      ) : (
        <span className="entity-placeholder">还没有获取到健康检查信息。</span>
      )}

      {feedback ? (
        <div className={feedback.kind === 'error' ? 'status-box status-box-error' : 'status-box'}>
          <strong>{feedback.kind === 'error' ? '操作失败' : '操作成功'}</strong>
          <span>{feedback.text}</span>
        </div>
      ) : null}

      <div className="panel-title panel-title-gap">身份</div>
      {currentUser ? (
        <div className="entity-card">
          <div className="entity-card-title">当前用户</div>
          <div className="entity-card-body">
            <strong>{formatNullableText(currentUser.displayName, '未命名用户')}</strong>
            <span>{formatNullableText(currentUser.email)}</span>
            <span>公司：{formatNullableText(currentUser.companyName, '未设置')}</span>
            <button
              className="secondary-button"
              disabled={busyAction !== null}
              type="button"
              onClick={onLogout}
            >
              {busyAction === 'logout' ? '退出中...' : '退出登录'}
            </button>
          </div>
        </div>
      ) : (
        <div className="setup-stack">
          <div className="form-card">
            <div className="form-card-title">注册</div>
            <label className="field">
              <span>邮箱</span>
              <input
                className="text-input"
                value={registerForm.email}
                onChange={event =>
                  onRegisterFormChange(current => ({ ...current, email: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>密码</span>
              <input
                className="text-input"
                type="password"
                value={registerForm.password}
                onChange={event =>
                  onRegisterFormChange(current => ({ ...current, password: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>显示名</span>
              <input
                className="text-input"
                value={registerForm.displayName}
                onChange={event =>
                  onRegisterFormChange(current => ({ ...current, displayName: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>公司名</span>
              <input
                className="text-input"
                value={registerForm.companyName}
                onChange={event =>
                  onRegisterFormChange(current => ({ ...current, companyName: event.currentTarget.value }))
                }
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null}
              type="button"
              onClick={onRegister}
            >
              {busyAction === 'register' ? '提交中...' : '注册并登录'}
            </button>
          </div>

          <div className="form-card">
            <div className="form-card-title">登录</div>
            <label className="field">
              <span>邮箱</span>
              <input
                className="text-input"
                value={loginForm.email}
                onChange={event =>
                  onLoginFormChange(current => ({ ...current, email: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>密码</span>
              <input
                className="text-input"
                type="password"
                value={loginForm.password}
                onChange={event =>
                  onLoginFormChange(current => ({ ...current, password: event.currentTarget.value }))
                }
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null}
              type="button"
              onClick={onLogin}
            >
              {busyAction === 'login' ? '登录中...' : '登录'}
            </button>
          </div>
        </div>
      )}
    </>
  );
}
