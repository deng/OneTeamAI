import { Navigate } from 'react-router-dom';
import { useAuth } from '../useAuth';
import { AppWorkspace } from '../AppWorkspace';

export function WorkspacePage() {
  const { token } = useAuth();

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  return <AppWorkspace />;
}
