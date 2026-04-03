import type { Dispatch, SetStateAction } from 'react';
import {
  TicketPriority,
  type ConversationDetailResponse,
  type ConversationSummaryResponse,
  type CreateConversationRequest,
  type CustomerResponse,
  type TicketResponse,
} from '../../generated/api';
import { ConversationPanel } from './ConversationPanel';

type ConversationSectionProps = {
  busyAction: string | null;
  canManageConversations: boolean;
  conversationDetail: ConversationDetailResponse | null;
  conversationDetailError: string | null;
  createConversationForm: CreateConversationRequest;
  filteredConversations: ConversationSummaryResponse[];
  isConversationDetailLoading: boolean;
  relatedTickets: TicketResponse[];
  selectedConversation: ConversationSummaryResponse | null;
  selectedConversationId: string;
  selectedCustomer: CustomerResponse | null;
  onCreateConversation: () => void;
  onCreateConversationFormChange: Dispatch<SetStateAction<CreateConversationRequest>>;
  onRunConversationWorkflow: () => void;
  onSelectConversationId: (conversationId: string) => void;
  onSelectRelatedConciergeAppId: (conciergeAppId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
};

export function ConversationSection({
  busyAction,
  canManageConversations,
  conversationDetail,
  conversationDetailError,
  createConversationForm,
  filteredConversations,
  isConversationDetailLoading,
  relatedTickets,
  selectedConversation,
  selectedConversationId,
  selectedCustomer,
  onCreateConversation,
  onCreateConversationFormChange,
  onRunConversationWorkflow,
  onSelectConversationId,
  onSelectRelatedConciergeAppId,
  onSelectRelatedCustomerId,
  onSelectRelatedTicketId,
}: ConversationSectionProps) {
  return (
    <>
      <div className="panel-title panel-title-gap">会话</div>
      <div className="form-card">
        <div className="form-card-title">创建会话</div>
        <label className="field">
          <span>当前客户</span>
          <input
            className="text-input"
            disabled
            value={selectedCustomer?.displayName ?? createConversationForm.customerDisplayName ?? ''}
          />
        </label>
        <label className="field">
          <span>客户邮箱</span>
          <input
            className="text-input"
            disabled
            value={selectedCustomer?.email ?? createConversationForm.customerEmail ?? ''}
          />
        </label>
        <label className="field">
          <span>首条消息</span>
          <textarea
            className="text-area"
            rows={3}
            value={createConversationForm.initialMessage ?? ''}
            onChange={event =>
              onCreateConversationFormChange(current => ({
                ...current,
                initialMessage: event.currentTarget.value,
              }))
            }
          />
        </label>
        <label className="field">
          <span>自动建单</span>
          <select
            className="text-input"
            value={createConversationForm.autoCreateTicket ? 'true' : 'false'}
            onChange={event =>
              onCreateConversationFormChange(current => ({
                ...current,
                autoCreateTicket: event.currentTarget.value === 'true',
              }))
            }
          >
            <option value="true">是</option>
            <option value="false">否</option>
          </select>
        </label>
        <label className="field">
          <span>自动建单优先级</span>
          <select
            className="text-input"
            value={createConversationForm.autoTicketPriority ?? TicketPriority.NUMBER_1}
            onChange={event =>
              onCreateConversationFormChange(current => ({
                ...current,
                autoTicketPriority: Number(event.currentTarget.value) as typeof current.autoTicketPriority,
              }))
            }
          >
            <option value={TicketPriority.NUMBER_0}>低</option>
            <option value={TicketPriority.NUMBER_1}>中</option>
            <option value={TicketPriority.NUMBER_2}>高</option>
            <option value={TicketPriority.NUMBER_3}>紧急</option>
          </select>
        </label>
        <button
          className="secondary-button"
          disabled={busyAction !== null || !selectedCustomer || !createConversationForm.initialMessage?.trim()}
          type="button"
          onClick={onCreateConversation}
        >
          {busyAction === 'create-conversation' ? '创建中...' : '创建会话'}
        </button>
      </div>

      <ConversationPanel
        filteredConversations={filteredConversations}
        selectedConversationId={selectedConversationId}
        onSelectConversationId={onSelectConversationId}
        onSelectRelatedConciergeAppId={onSelectRelatedConciergeAppId}
        onSelectRelatedCustomerId={onSelectRelatedCustomerId}
        isConversationDetailLoading={isConversationDetailLoading}
        conversationDetailError={conversationDetailError}
        conversationDetail={conversationDetail}
        selectedConversation={selectedConversation}
        relatedTickets={relatedTickets}
        onSelectRelatedTicketId={onSelectRelatedTicketId}
        busyAction={busyAction}
        canManageConversations={canManageConversations}
        onRunConversationWorkflow={onRunConversationWorkflow}
      />
    </>
  );
}
