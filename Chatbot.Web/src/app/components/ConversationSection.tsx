import { useState, type Dispatch, SetStateAction } from 'react';
import {
  TicketPriority,
  type ConversationDetailResponse,
  type ConversationSummaryResponse,
  type CreateConversationRequest,
  type CustomerResponse,
  type TicketResponse,
} from '../../generated/api';
import { Modal } from './Modal';
import { ConversationPanel } from './ConversationPanel';

type ConversationSectionProps = {
  autoRunConversationWorkflow: boolean;
  busyAction: string | null;
  canManageConversations: boolean;
  conversationDetail: ConversationDetailResponse | null;
  conversationDetailError: string | null;
  createConversationForm: CreateConversationRequest;
  filteredConversations: ConversationSummaryResponse[];
  conversationsLoading?: boolean;
  conversationsError?: string | null;
  onRetryConversations?: () => void;
  isConversationDetailLoading: boolean;
  relatedTickets: TicketResponse[];
  selectedConversation: ConversationSummaryResponse | null;
  selectedConversationId: string;
  selectedCustomer: CustomerResponse | null;
  onCreateConversation: () => void;
  onCreateConversationFormChange: Dispatch<SetStateAction<CreateConversationRequest>>;
  onAutoRunConversationWorkflowChange: (value: boolean) => void;
  onRunConversationWorkflow: () => void;
  onSelectConversationId: (conversationId: string) => void;
  onSelectRelatedConciergeAppId: (conciergeAppId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
};

export function ConversationSection({
  autoRunConversationWorkflow,
  busyAction,
  canManageConversations,
  conversationDetail,
  conversationDetailError,
  createConversationForm,
  filteredConversations,
  conversationsLoading = false,
  conversationsError = null,
  onRetryConversations,
  isConversationDetailLoading,
  relatedTickets,
  selectedConversation,
  selectedConversationId,
  selectedCustomer,
  onCreateConversation,
  onCreateConversationFormChange,
  onAutoRunConversationWorkflowChange,
  onRunConversationWorkflow,
  onSelectConversationId,
  onSelectRelatedConciergeAppId,
  onSelectRelatedCustomerId,
  onSelectRelatedTicketId,
}: ConversationSectionProps) {
  const [showForm, setShowForm] = useState(false);
  return (
    <>
      <div className="panel-title panel-title-gap">会话</div>
      {canManageConversations ? (
        <button className="secondary-button" type="button" onClick={() => setShowForm(true)}>
          + 创建会话
        </button>
      ) : null}

      <Modal open={showForm} onClose={() => setShowForm(false)} title="创建会话">
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
        <label className="checkbox-field">
          <input
            checked={autoRunConversationWorkflow}
            type="checkbox"
            onChange={event => onAutoRunConversationWorkflowChange(event.currentTarget.checked)}
          />
          创建会话后自动启动 AI 协作
        </label>
        <button
          className="secondary-button"
          disabled={busyAction !== null || !selectedCustomer || !createConversationForm.initialMessage?.trim()}
          type="button"
          onClick={() => { onCreateConversation(); setShowForm(false); }}
        >
          {busyAction === 'create-conversation' ? '创建中...' : '创建会话'}
        </button>
      </Modal>

      <ConversationPanel
        filteredConversations={filteredConversations}
        conversationsLoading={conversationsLoading}
        conversationsError={conversationsError}
        onRetryConversations={onRetryConversations}
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
