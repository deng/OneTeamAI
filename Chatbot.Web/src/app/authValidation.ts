export type PasswordStrength = 'none' | 'weak' | 'medium' | 'strong';

export function validateEmail(email: string): string | null {
  if (!email.trim()) return '邮箱不能为空';
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return '邮箱格式不正确';
  return null;
}

export function validateRequired(value: string, label: string): string | null {
  if (!value.trim()) return `${label}不能为空`;
  return null;
}

export function calculatePasswordStrength(password: string): PasswordStrength {
  if (!password) return 'none';
  let score = 0;
  if (password.length >= 8) score += 1;
  if (/[a-z]/.test(password)) score += 1;
  if (/[A-Z]/.test(password)) score += 1;
  if (/[0-9]/.test(password)) score += 1;
  if (score <= 1) return 'weak';
  if (score <= 2) return 'medium';
  return 'strong';
}

export const PASSWORD_REQUIREMENTS = [
  { label: '至少 8 位字符', test: (p: string) => p.length >= 8 },
  { label: '包含小写字母', test: (p: string) => /[a-z]/.test(p) },
  { label: '包含大写字母', test: (p: string) => /[A-Z]/.test(p) },
  { label: '包含数字', test: (p: string) => /[0-9]/.test(p) },
] as const;

export function isLoginFormValid(email: string, password: string): boolean {
  return Boolean(email.trim() && password);
}

export function isRegisterFormValid(email: string, password: string, displayName: string): boolean {
  return Boolean(email.trim() && password && displayName.trim());
}
