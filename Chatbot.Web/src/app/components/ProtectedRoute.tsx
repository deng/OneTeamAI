import { Navigate } from 'react-router-dom';
import { authStorageKey } from '../constants';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const hasToken = !!window.localStorage.getItem(authStorageKey);
  if (!hasToken) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}
