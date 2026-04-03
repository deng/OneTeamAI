import { useEffect, useMemo, useState } from 'react';
import {
  TicketPriority,
  TicketStatus,
  type ConversationSummaryResponse,
  type CreateTicketRequest,
  type MemberResponse,
  type TicketResponse,
  type UpdateTicketRequest,
} from '../generated/api';
import { normalizeTicketDetailItem } from './normalizers';
import type { TicketDetailItem } from './types';
import { createWorkspaceApis, fetchJson, getErrorMessage } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

function emptyCreateTicketForm(): CreateTicketRequest {
  return {
    title: '',
    summary: '',
    priority: TicketPriority.NUMBER_1,
    assignedMemberId: '',
  };
}

function buildDraft(ticket: TicketResponse): UpdateTicketRequest {
  return {
    status: ticket.status ?? TicketStatus.NUMBER_0,
    priority: ticket.priority ?? TicketPriority.NUMBER_1,
    assignedMemberId: ticket.assignedMemberId ?? undefined,
    category: ticket.category ?? '',
    dueAt: ticket.dueAt ?? undefined,
    activityNote: '',
  };
}

export function useWorkspaceTickets({
  currentTeamId,
  selectedConversation,
  selectedConversationId,
  selectedCustomerId,
  token,
  runAction,
  setFeedback,
}: {
  currentTeamId: string;
  selectedConversation: ConversationSummaryResponse | null;
  selectedConversationId: string;
  selectedCustomerId: string;
  token: string | null;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [tickets, setTickets] = useState<TicketResponse[]>([]);
  const [selectedTicketId, setSelectedTicketId] = useState('');
  const [ticketUpdateDrafts, setTicketUpdateDrafts] = useState<Record<string, UpdateTicketRequest>>({});
  const [ticketDetail, setTicketDetail] = useState<TicketDetailItem | null>(null);
  const [isTicketDetailLoading, setIsTicketDetailLoading] = useState(false);
  const [ticketDetailError, setTicketDetailError] = useState<string | null>(null);
  const [ticketCommentDraft, setTicketCommentDraft] = useState('');
  const [createTicketForm, setCreateTicketForm] = useState<CreateTicketRequest>(emptyCreateTicketForm);

  const { ticketsApi } = useMemo(() => createWorkspaceApis(token), [token]);
  const selectedTicket = tickets.find(ticket => ticket.id === selectedTicketId) ?? null;
  const relatedTickets = selectedConversationId
    ? tickets.filter(ticket => ticket.conversationId === selectedConversationId)
    : [];
  const filteredTickets = selectedConversationId
    ? relatedTickets
    : selectedCustomerId
      ? tickets.filter(ticket => ticket.customerId === selectedCustomerId)
      : tickets;

  useEffect(() => {
    if (!token || !currentTeamId) {
      setTickets([]);
      setSelectedTicketId('');
      setTicketUpdateDrafts({});
      setTicketDetail(null);
      setTicketDetailError(null);
      return;
    }

    let cancelled = false;

    void (async () => {
      const nextTickets = await ticketsApi.listTickets({ teamId: currentTeamId });

      if (cancelled) {
        return;
      }

      setTickets(nextTickets);
      setTicketUpdateDrafts(
        Object.fromEntries(nextTickets.filter(ticket => ticket.id).map(ticket => [ticket.id as string, buildDraft(ticket)])),
      );
      setSelectedTicketId(current =>
        nextTickets.some(ticket => ticket.id === current)
          ? current
          : (nextTickets[0]?.id ?? ''),
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
  }, [currentTeamId, setFeedback, ticketsApi, token]);

  useEffect(() => {
    if (!selectedConversation) {
      setCreateTicketForm(emptyCreateTicketForm());
      return;
    }

    setCreateTicketForm(current => ({
      ...current,
      title: current.title || `${selectedConversation.customerName ?? '客户'}需求`,
      summary: current.summary || (selectedConversation.latestMessage ?? ''),
    }));
  }, [selectedConversation]);

  useEffect(() => {
    if (!token || !currentTeamId || !selectedTicketId) {
      setTicketDetail(null);
      setTicketDetailError(null);
      setIsTicketDetailLoading(false);
      return;
    }

    let cancelled = false;
    setIsTicketDetailLoading(true);
    setTicketDetailError(null);

    void (async () => {
      try {
        const detail = await fetchJson<Parameters<typeof normalizeTicketDetailItem>[0]>(
          `/api/teams/${currentTeamId}/tickets/${selectedTicketId}`,
          {
            headers: {
              Authorization: `Bearer ${token ?? ''}`,
            },
          },
        );

        if (!cancelled) {
          setTicketDetail(normalizeTicketDetailItem(detail));
        }
      } catch (error) {
        if (!cancelled) {
          setTicketDetail(null);
          setTicketDetailError(getErrorMessage(error));
        }
      } finally {
        if (!cancelled) {
          setIsTicketDetailLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [currentTeamId, selectedTicketId, token]);

  async function refreshTickets() {
    if (!currentTeamId) {
      return;
    }

    const nextTickets = await ticketsApi.listTickets({ teamId: currentTeamId });
    setTickets(nextTickets);
    setTicketUpdateDrafts(
      Object.fromEntries(nextTickets.filter(ticket => ticket.id).map(ticket => [ticket.id as string, buildDraft(ticket)])),
    );
    setSelectedTicketId(current =>
      nextTickets.some(ticket => ticket.id === current)
        ? current
        : (nextTickets[0]?.id ?? ''),
    );
  }

  async function handleCreateTicket() {
    if (!selectedConversation?.id) {
      return;
    }

    const conversationId = selectedConversation.id;

    await runAction('create-ticket', async () => {
      const created = await ticketsApi.createTicket({
        conversationId,
        createTicketRequest: createTicketForm,
      });

      await refreshTickets();
      setSelectedTicketId(created.id ?? '');
      setCreateTicketForm(emptyCreateTicketForm());
      setFeedback({
        kind: 'success',
        text: `已创建工单 ${created.title ?? ''}。`,
      });
    });
  }

  async function handleSaveTicket(ticket: TicketResponse) {
    if (!currentTeamId || !ticket.id) {
      return;
    }

    const ticketId = ticket.id;
    const draft = ticketUpdateDrafts[ticketId] ?? buildDraft(ticket);

    await runAction('update-ticket', async () => {
      await ticketsApi.updateTicket({
        teamId: currentTeamId,
        ticketId,
        updateTicketRequest: draft,
      });

      await refreshTickets();
      setFeedback({
        kind: 'success',
        text: `已更新工单 ${ticket.title ?? ''}。`,
      });
    });
  }

  async function handleAddComment() {
    if (!currentTeamId || !selectedTicket?.id || !ticketCommentDraft.trim()) {
      return;
    }

    const ticketId = selectedTicket.id;

    await runAction('comment-ticket', async () => {
      await fetchJson(`/api/teams/${currentTeamId}/tickets/${ticketId}/comments`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token ?? ''}`,
        },
        body: JSON.stringify({
          content: ticketCommentDraft.trim(),
        }),
      });

      setTicketCommentDraft('');
      await refreshTickets();
      const detail = await fetchJson<Parameters<typeof normalizeTicketDetailItem>[0]>(
        `/api/teams/${currentTeamId}/tickets/${ticketId}`,
        {
          headers: {
            Authorization: `Bearer ${token ?? ''}`,
          },
        },
      );
      setTicketDetail(normalizeTicketDetailItem(detail));
      setFeedback({
        kind: 'success',
        text: '已添加工单评论。',
      });
    });
  }

  return {
    createTicketForm,
    filteredTickets,
    isTicketDetailLoading,
    relatedTickets,
    tickets,
    selectedTicket,
    selectedTicketId,
    ticketCommentDraft,
    ticketDetail,
    ticketDetailError,
    ticketUpdateDrafts,
    setCreateTicketForm,
    setSelectedTicketId,
    setTicketCommentDraft,
    setTicketUpdateDrafts,
    handleAddComment,
    handleCreateTicket,
    handleSaveTicket,
  };
}
