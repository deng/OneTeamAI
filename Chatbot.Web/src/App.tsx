import { AppWorkspace } from './app/AppWorkspace';
import { PublicConciergePage } from './app/components/PublicConciergePage';

export default function App() {
  const params = new URLSearchParams(window.location.search);
  const view = params.get('view');
  const appId = params.get('appId') ?? '';

  if (view === 'concierge' && appId) {
    return <PublicConciergePage appId={appId} />;
  }

  return <AppWorkspace />;
}
