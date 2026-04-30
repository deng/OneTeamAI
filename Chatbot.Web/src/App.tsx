import { Navigate, Route, Routes, useParams } from 'react-router-dom';
import { LoginPage } from './app/components/LoginPage';
import { PublicConciergePage } from './app/components/PublicConciergePage';
import { WorkspacePage } from './app/components/WorkspacePage';

function ConciergeRoute() {
  const { appId } = useParams<{ appId: string }>();
  if (!appId) {
    return <Navigate to="/workspace" replace />;
  }
  return <PublicConciergePage appId={appId} />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/workspace" element={<WorkspacePage />} />
      <Route path="/concierge/:appId" element={<ConciergeRoute />} />
      <Route path="*" element={<Navigate to="/workspace" replace />} />
    </Routes>
  );
}
