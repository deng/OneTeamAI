import { useEffect, useMemo, useState } from 'react';
import {
  TicketPriority,
  type ConciergeAppResponse,
  type ConversationDetailResponse,
  type ConversationSummaryResponse,
  type CreateConversationRequest,
  type CustomerResponse,
} from '../generated/api';
import { createWorkspaceApis, getErrorMessage } from './workspaceApi';

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
  selectedConciergeApp,
  selectedCustomer,
  selectedCustomerId,
  token,
  runAction,
  setFeedback,
}: {
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
        text: '已创建会话。',
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
    selectedConversation,
    selectedConversationId,
    setCreateConversationForm,
    setSelectedConversationId,
    handleCreateConversation,
  };
}
