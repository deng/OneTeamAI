import { useCallback, useState } from 'react';
import type { Feedback } from './types';
import { getErrorMessage } from './workspaceApi';

export function useWorkspaceStatus() {
  const [feedback, setFeedback] = useState<Feedback | null>(null);
  const [busyAction, setBusyAction] = useState<string | null>(null);

  const runAction = useCallback(async (name: string, action: () => Promise<void>) => {
    setBusyAction(name);
    setFeedback(null);

    try {
      await action();
    } catch (error) {
      setFeedback({
        kind: 'error',
        text: getErrorMessage(error),
      });
    } finally {
      setBusyAction(null);
    }
  }, []);

  return {
    busyAction,
    feedback,
    runAction,
    setFeedback,
  };
}
