import { useEffect, useMemo, useState } from 'react';
import {
  AgentWorkflowTriggerMode,
  TicketPriority,
  type ConciergeAppResponse,
  type ConversationDetailResponse,
  type ConversationSummaryResponse,
  type CreateConversationRequest,
  type CustomerResponse,
} from '../generated/api';
import { createWorkspaceApis, fetchJson, getErrorMessage } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

function emptyCreateConversationForm(): CreateConversationRequest {
  return {
    customerId: '',
    customerDisplayName: '',
    customerEmail: '',
    initialMessage: '',
    autoCreateTicket: true,
    autoTicketPriority: TicketPriority.NUMBER_1,
  };
}

export function useWorkspaceConversations({
  currentTeamId,
  selectedConciergeApp,
  selectedCustomer,
  selectedCustomerId,
  token,
  runAction,
  setFeedback,
}: {
  currentTeamId: string;
  selectedConciergeApp: ConciergeAppResponse | null | undefined;
  selectedCustomer: CustomerResponse | null;
  selectedCustomerId: string;
  token: string | null;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [conversations, setConversations] = useState<ConversationSummaryResponse[]>([]);
  const [selectedConversationId, setSelectedConversationId] = useState('');
  const [conversationDetail, setConversationDetail] = useState<ConversationDetailResponse | null>(null);
  const [conversationDetailError, setConversationDetailError] = useState<string | null>(null);
  const [isConversationDetailLoading, setIsConversationDetailLoading] = useState(false);
  const [createConversationForm, setCreateConversationForm] =
    useState<CreateConversationRequest>(emptyCreateConversationForm);
  const [autoRunConversationWorkflow, setAutoRunConversationWorkflow] = useState(false);

  const { conversationsApi } = useMemo(() => createWorkspaceApis(token), [token]);
  const selectedConversation =
    conversations.find(conversation => conversation.id === selectedConversationId) ?? null;
  const filteredConversations = selectedCustomerId
    ? conversations.filter(conversation => conversation.customerId === selectedCustomerId)
    : conversations;

  useEffect(() => {
    if (!selectedCustomer) {
      setCreateConversationForm(current => ({
        ...current,
        customerId: '',
        customerDisplayName: '',
        customerEmail: '',
      }));
      return;
    }

    setCreateConversationForm(current => ({
      ...current,
      customerId: selectedCustomer.id ?? '',
      customerDisplayName: selectedCustomer.displayName ?? '',
      customerEmail: selectedCustomer.email ?? '',
    }));
  }, [selectedCustomer]);

  useEffect(() => {
    if (!token || !selectedConciergeApp?.id) {
      setConversations([]);
      setSelectedConversationId('');
      setConversationDetail(null);
      setConversationDetailError(null);
      return;
    }

    let cancelled = false;
    const conciergeAppId = selectedConciergeApp.id;

    void (async () => {
      const nextConversations = await conversationsApi.listConversations({
        conciergeAppId,
      });

      if (cancelled) {
        return;
      }

      setConversations(nextConversations);
      setSelectedConversationId(current =>
        nextConversations.some(conversation => conversation.id === current)
          ? current
          : (nextConversations[0]?.id ?? ''),
      );
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: getErrorMessage(error),
        });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [conversationsApi, selectedConciergeApp, setFeedback, token]);

  useEffect(() => {
    if (!token || !selectedConversationId) {
      setConversationDetail(null);
      setConversationDetailError(null);
      setIsConversationDetailLoading(false);
      return;
    }

    let cancelled = false;
    setIsConversationDetailLoading(true);
    setConversationDetailError(null);

    void (async () => {
      try {
        const detail = await conversationsApi.getConversation({
          conversationId: selectedConversationId,
        });

        if (!cancelled) {
          setConversationDetail(detail);
        }
      } catch (error) {
        if (!cancelled) {
          setConversationDetail(null);
          setConversationDetailError(getErrorMessage(error));
        }
      } finally {
        if (!cancelled) {
          setIsConversationDetailLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [conversationsApi, selectedConversationId, token]);

  async function refreshConversations() {
    if (!selectedConciergeApp?.id) {
      return;
    }

    const nextConversations = await conversationsApi.listConversations({
      conciergeAppId: selectedConciergeApp.id,
    });
    setConversations(nextConversations);
    setSelectedConversationId(current =>
      nextConversations.some(conversation => conversation.id === current)
        ? current
        : (nextConversations[0]?.id ?? ''),
    );
  }

  async function handleCreateConversation() {
    if (!selectedConciergeApp?.id) {
      return;
    }

    const conciergeAppId = selectedConciergeApp.id;

    await runAction('create-conversation', async () => {
      const created = await conversationsApi.createConversation({
        conciergeAppId,
        createConversationRequest: createConversationForm,
      });

      if (autoRunConversationWorkflow && currentTeamId && created.id) {
        await fetchJson(`/api/teams/${currentTeamId}/conversations/${created.id}/workflows`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token ?? ''}`,
          },
          body: JSON.stringify({
            goal: `围绕新会话“${createConversationForm.customerDisplayName ?? '客户会话'}”自动完成接待分析与后续建议。`,
            triggerMode: AgentWorkflowTriggerMode.NUMBER_1,
          }),
        });
      }

      await refreshConversations();
      setSelectedConversationId(created.id ?? '');
      setCreateConversationForm(current => ({
        ...current,
        initialMessage: '',
        autoCreateTicket: true,
        autoTicketPriority: current.autoTicketPriority ?? TicketPriority.NUMBER_1,
      }));
      setFeedback({
        kind: 'success',
        text: autoRunConversationWorkflow ? '已创建会话并自动启动协作。' : '已创建会话。',
      });
    });
  }

  return {
    conversations,
    conversationDetail,
    conversationDetailError,
    createConversationForm,
    filteredConversations,
    isConversationDetailLoading,
    autoRunConversationWorkflow,
    selectedConversation,
    selectedConversationId,
    setAutoRunConversationWorkflow,
    setCreateConversationForm,
    setSelectedConversationId,
    handleCreateConversation,
  };
}
