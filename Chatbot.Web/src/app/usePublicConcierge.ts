import { useEffect, useState } from 'react';
import { TicketPriority } from '../generated/api';
import type { Feedback, PublicConciergeApp } from './types';
import { fetchJson } from './workspaceApi';

type PublicConciergeIntakeResponse = {
  customerId: string;
  conversationId: string;
  ticketId?: string | null;
  message: string;
};

type PublicConciergeIntakeForm = {
  displayName: string;
  email: string;
  phoneNumber: string;
  companyName: string;
  message: string;
  autoCreateTicket: boolean;
  autoTicketPriority: TicketPriority;
};

function emptyIntakeForm(): PublicConciergeIntakeForm {
  return {
    displayName: '',
    email: '',
    phoneNumber: '',
    companyName: '',
    message: '',
    autoCreateTicket: true,
    autoTicketPriority: TicketPriority.NUMBER_1,
  };
}

export function usePublicConcierge(appId: string) {
  const [conciergeApp, setConciergeApp] = useState<PublicConciergeApp | null>(null);
  const [intakeForm, setIntakeForm] = useState<PublicConciergeIntakeForm>(emptyIntakeForm);
  const [busyAction, setBusyAction] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<Feedback | null>(null);

  useEffect(() => {
    if (!appId) {
      setFeedback({
        kind: 'error',
        text: '缺少坐台程序 ID。',
      });
      return;
    }

    let cancelled = false;
    setBusyAction('load-public-concierge');
    setFeedback(null);

    void (async () => {
      const app = await fetchJson<PublicConciergeApp>(`/api/public/concierge-apps/${appId}`);
      if (cancelled) {
        return;
      }

      setConciergeApp(app);
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: error instanceof Error ? error.message : '客户入口加载失败。',
        });
      }
    }).finally(() => {
      if (!cancelled) {
        setBusyAction(null);
      }
    });

    return () => {
      cancelled = true;
    };
  }, [appId]);

  async function handleSubmitIntake() {
    if (!appId) {
      return;
    }

    if (conciergeApp?.requireEmail && !intakeForm.email.trim()) {
      setFeedback({
        kind: 'error',
        text: '当前坐台程序要求填写邮箱。',
      });
      return;
    }

    if (conciergeApp?.requirePhoneNumber && !intakeForm.phoneNumber.trim()) {
      setFeedback({
        kind: 'error',
        text: '当前坐台程序要求填写手机号。',
      });
      return;
    }

    setBusyAction('submit-public-intake');
    setFeedback(null);

    try {
      const result = await fetchJson<PublicConciergeIntakeResponse>(
        `/api/public/concierge-apps/${appId}/intake`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            ...intakeForm,
            phoneNumber: intakeForm.phoneNumber.trim(),
          }),
        },
      );

      setIntakeForm(current => ({
        ...current,
        message: '',
      }));
      setFeedback({
        kind: 'success',
        text: result.message,
      });
    } catch (error) {
      setFeedback({
        kind: 'error',
        text: error instanceof Error ? error.message : '需求提交失败。',
      });
    } finally {
      setBusyAction(null);
    }
  }

  return {
    busyAction,
    conciergeApp,
    feedback,
    intakeForm,
    setIntakeForm,
    handleSubmitIntake,
  };
}
