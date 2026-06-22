import { calculatePasswordStrength, PASSWORD_REQUIREMENTS } from '../authValidation';

type PasswordStrengthIndicatorProps = {
  password: string;
};

export function PasswordStrengthIndicator({ password }: PasswordStrengthIndicatorProps) {
  const strength = calculatePasswordStrength(password);
  if (!password) return null;

  const barWidth = strength === 'weak' ? '33%' : strength === 'medium' ? '66%' : '100%';
  const barColor = strength === 'weak' ? '#c05a4a' : strength === 'medium' ? '#d4a34b' : '#4f9a73';
  const labels: Record<string, string> = { weak: '弱', medium: '中', strong: '强' };

  return (
    <>
      <div className="password-strength">
        <div className="password-strength-bar">
          <div className="password-strength-fill" style={{ width: barWidth, background: barColor }} />
        </div>
        <span className="password-strength-label" style={{ color: barColor }}>{labels[strength]}</span>
      </div>
      <div className="password-reqs">
        {PASSWORD_REQUIREMENTS.map(req => (
          <span key={req.label} className={`password-req${req.test(password) ? ' password-req-met' : ''}`}>
            {req.test(password) ? '✓' : '○'} {req.label}
          </span>
        ))}
      </div>
    </>
  );
}
