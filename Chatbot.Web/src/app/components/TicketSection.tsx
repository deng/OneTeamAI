import type { Dispatch, SetStateAction } from 'react';
import {
  TicketPriority,
  type ConversationSummaryResponse,
  type CreateTicketRequest,
  type MemberResponse,
  type TicketResponse,
  type UpdateTicketRequest,
} from '../../generated/api';
import type { TicketDetailItem } from '../types';
import { TicketPanel } from './TicketPanel';

type TicketSectionProps = {
  autoRunTicketWorkflow: boolean;
  busyAction: string | null;
  canManageTickets: boolean;
  createTicketForm: CreateTicketRequest;
  filteredTickets: TicketResponse[];
  isTicketDetailLoading: boolean;
  relatedTickets: TicketResponse[];
  selectedConversation: ConversationSummaryResponse | null;
  selectedTicket: TicketResponse | null;
  selectedTicketId: string;
  teamMembers: MemberResponse[];
  ticketCommentDraft: string;
  ticketDetail: TicketDetailItem | null;
  ticketDetailError: string | null;
  ticketUpdateDrafts: Record<string, UpdateTicketRequest>;
  onAddComment: () => void;
  onAutoRunTicketWorkflowChange: (value: boolean) => void;
  onCreateTicket: () => void;
  onCreateTicketFormChange: Dispatch<SetStateAction<CreateTicketRequest>>;
  onSaveTicket: (ticket: TicketResponse) => void;
  onSelectRelatedConciergeAppId: (conciergeAppId: string) => void;
  onSelectRelatedConversationId: (conversationId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  onSelectRelatedProjectId: (projectId: string) => void;
  onSelectTicketId: (ticketId: string) => void;
  onTicketCommentDraftChange: (value: string) => void;
  onTicketDraftsChange: Dispatch<SetStateAction<Record<string, UpdateTicketRequest>>>;
};

export function TicketSection({
  autoRunTicketWorkflow,
  busyAction,
  canManageTickets,
  createTicketForm,
  filteredTickets,
  isTicketDetailLoading,
  relatedTickets,
  selectedConversation,
  selectedTicket,
  selectedTicketId,
  teamMembers,
  ticketCommentDraft,
  ticketDetail,
  ticketDetailError,
  ticketUpdateDrafts,
  onAddComment,
  onAutoRunTicketWorkflowChange,
  onCreateTicket,
  onCreateTicketFormChange,
  onSaveTicket,
  onSelectRelatedConciergeAppId,
  onSelectRelatedConversationId,
  onSelectRelatedCustomerId,
  onSelectRelatedProjectId,
  onSelectTicketId,
  onTicketCommentDraftChange,
  onTicketDraftsChange,
}: TicketSectionProps) {
  return (
    <>
      <div className="panel-title panel-title-gap">工单</div>
      <div className="form-card">
        <div className="form-card-title">手动建单</div>
        <label className="field">
          <span>当前会话</span>
          <input
            className="text-input"
            disabled
            value={selectedConversation?.customerName ?? '请先选择会话'}
          />
        </label>
        <label className="field">
          <span>标题</span>
          <input
            className="text-input"
            value={createTicketForm.title ?? ''}
            onChange={event =>
              onCreateTicketFormChange(current => ({ ...current, title: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>摘要</span>
          <textarea
            className="text-area"
            rows={3}
            value={createTicketForm.summary ?? ''}
            onChange={event =>
              onCreateTicketFormChange(current => ({ ...current, summary: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>优先级</span>
          <select
            className="text-input"
            value={createTicketForm.priority ?? TicketPriority.NUMBER_1}
            onChange={event =>
              onCreateTicketFormChange(current => ({
                ...current,
                priority: Number(event.currentTarget.value) as typeof current.priority,
              }))
            }
          >
            <option value={TicketPriority.NUMBER_0}>低</option>
            <option value={TicketPriority.NUMBER_1}>中</option>
            <option value={TicketPriority.NUMBER_2}>高</option>
            <option value={TicketPriority.NUMBER_3}>紧急</option>
          </select>
        </label>
        <label className="field">
          <span>负责人</span>
          <select
            className="text-input"
            value={createTicketForm.assignedMemberId ?? ''}
            onChange={event =>
              onCreateTicketFormChange(current => ({
                ...current,
                assignedMemberId: event.currentTarget.value || undefined,
              }))
            }
          >
            <option value="">暂不分配</option>
            {teamMembers.map(member => (
              <option key={member.id} value={member.id}>
                {member.displayName}
              </option>
            ))}
          </select>
        </label>
        <label className="checkbox-field">
          <input
            checked={autoRunTicketWorkflow}
            type="checkbox"
            onChange={event => onAutoRunTicketWorkflowChange(event.currentTarget.checked)}
          />
          创建工单后自动启动 AI 协作
        </label>
        <button
          className="secondary-button"
          disabled={busyAction !== null || !selectedConversation || !createTicketForm.title?.trim()}
          type="button"
          onClick={onCreateTicket}
        >
          {busyAction === 'create-ticket' ? '创建中...' : '创建工单'}
        </button>
      </div>

      <TicketPanel
        filteredTickets={filteredTickets}
        selectedTicketId={selectedTicketId}
        onSelectTicketId={onSelectTicketId}
        onSelectRelatedConciergeAppId={onSelectRelatedConciergeAppId}
        onSelectRelatedProjectId={onSelectRelatedProjectId}
        onSelectRelatedCustomerId={onSelectRelatedCustomerId}
        onSelectRelatedConversationId={onSelectRelatedConversationId}
        teamMembers={teamMembers}
        ticketUpdateDrafts={ticketUpdateDrafts}
        onTicketDraftsChange={onTicketDraftsChange}
        busyAction={busyAction}
        canManageTickets={canManageTickets}
        onSaveTicket={onSaveTicket}
        selectedTicket={selectedTicket}
        ticketDetail={ticketDetail}
        isTicketDetailLoading={isTicketDetailLoading}
        ticketDetailError={ticketDetailError}
        ticketCommentDraft={ticketCommentDraft}
        onTicketCommentDraftChange={onTicketCommentDraftChange}
        onAddComment={onAddComment}
      />
    </>
  );
}
