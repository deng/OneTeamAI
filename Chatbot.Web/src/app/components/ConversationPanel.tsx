import type {
  AgentWorkflowResponse,
  ConversationDetailResponse,
  ConversationSummaryResponse,
  TicketResponse,
} from '../../generated/api';
import {
  formatConversationParticipant,
  formatConversationStatus,
  formatDateTime,
  formatNullableText,
  formatTicketPriority,
  formatTicketStatus,
} from '../formatters';

type ConversationPanelProps = {
  filteredConversations: ConversationSummaryResponse[];
  selectedConversationId: string;
  onSelectConversationId: (conversationId: string) => void;
  onSelectRelatedConciergeAppId: (conciergeAppId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  isConversationDetailLoading: boolean;
  conversationDetailError: string | null;
  conversationDetail: ConversationDetailResponse | null;
  selectedConversation: ConversationSummaryResponse | null | undefined;
  relatedTickets: TicketResponse[];
  onSelectRelatedTicketId: (ticketId: string) => void;
  busyAction: string | null;
  canManageConversations: boolean;
  onRunConversationWorkflow: () => void;
};

export function ConversationPanel({
  filteredConversations,
  selectedConversationId,
  onSelectConversationId,
  onSelectRelatedConciergeAppId,
  onSelectRelatedCustomerId,
  isConversationDetailLoading,
  conversationDetailError,
  conversationDetail,
  selectedConversation,
  relatedTickets,
  onSelectRelatedTicketId,
  busyAction,
  canManageConversations,
  onRunConversationWorkflow,
}: ConversationPanelProps) {
  return (
    <>
      <div className="entity-card">
        <div className="entity-card-title">会话</div>
        {filteredConversations.length > 0 ? (
          <div className="entity-chip-list">
            {filteredConversations.map(conversation => (
              <button
                className={
                  conversation.id === selectedConversationId
                    ? 'entity-chip entity-chip-button entity-chip-selected'
                    : 'entity-chip entity-chip-button'
                }
                key={conversation.id}
                type="button"
                onClick={() => onSelectConversationId(conversation.id ?? '')}
              >
                <strong>{conversation.customerName ?? conversation.id}</strong>
                <span>{formatConversationStatus(conversation.status)}</span>
                <span>{conversation.latestMessage ?? '暂无最新消息'}</span>
                <span>
                  消息数：{conversation.messageCount ?? 0} · 创建于：{formatDateTime(conversation.createdAt)}
                </span>
              </button>
            ))}
          </div>
        ) : (
          <span className="entity-placeholder">暂无会话</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">会话详情</div>
        {isConversationDetailLoading ? (
          <span className="entity-placeholder">正在加载会话详情...</span>
        ) : conversationDetailError ? (
          <span className="entity-placeholder">{conversationDetailError}</span>
        ) : conversationDetail ? (
          <div className="entity-card-body">
            <strong>{conversationDetail.customer?.displayName ?? selectedConversation?.customerName ?? '匿名客户'}</strong>
            <span>
              状态：{formatConversationStatus(conversationDetail.status)} · 消息数：
              {conversationDetail.messages?.length ?? 0}
            </span>
            <span>客户邮箱：{formatNullableText(conversationDetail.customer?.email)}</span>
            <span>会话 ID：{formatNullableText(conversationDetail.id, '未生成')}</span>
            <div className="entity-chip-list">
              {conversationDetail.customerId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedCustomerId(conversationDetail.customerId ?? '')}
                >
                  <strong>关联客户</strong>
                  <span>{formatNullableText(conversationDetail.customer?.displayName, conversationDetail.customerId)}</span>
                </button>
              ) : null}
              {conversationDetail.conciergeAppId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedConciergeAppId(conversationDetail.conciergeAppId ?? '')}
                >
                  <strong>关联坐台程序</strong>
                  <span>{conversationDetail.conciergeAppId}</span>
                </button>
              ) : null}
            </div>
            <span>
              关联工单：
              {relatedTickets.length > 0 ? null : '暂无'}
            </span>
            {relatedTickets.length > 0 ? (
              <div className="entity-chip-list">
                {relatedTickets.map(ticket => (
                  <button
                    className="entity-chip entity-chip-button"
                    key={ticket.id}
                    type="button"
                    onClick={() => onSelectRelatedTicketId(ticket.id ?? '')}
                  >
                    <strong>{formatNullableText(ticket.title, '未命名工单')}</strong>
                    <span>
                      {formatTicketPriority(ticket.priority)} / {formatTicketStatus(ticket.status)}
                    </span>
                    <span>{formatNullableText(ticket.id, '未生成 ID')}</span>
                  </button>
                ))}
              </div>
            ) : null}
            <span>
              关联工单 ID：
              {conversationDetail.ticketIds && conversationDetail.ticketIds.length > 0
                ? conversationDetail.ticketIds.join(' / ')
                : '暂无'}
            </span>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManageConversations || !conversationDetail.id}
              type="button"
              onClick={onRunConversationWorkflow}
            >
              {busyAction === 'run-conversation-workflow' ? '运行中...' : '运行会话协作'}
            </button>
            <div className="detail-timeline">
              {(conversationDetail.messages ?? []).map(message => (
                <div className="detail-message" key={message.id}>
                  <div className="detail-message-meta">
                    <strong>{message.senderName ?? formatConversationParticipant(message.participantType)}</strong>
                    <span>
                      {formatConversationParticipant(message.participantType)} · {formatDateTime(message.createdAt)}
                    </span>
                  </div>
                  <div className="detail-message-body">{formatNullableText(message.content, '空消息')}</div>
                </div>
              ))}
            </div>
          </div>
        ) : (
          <span className="entity-placeholder">选择一条会话后，这里会展示客户、消息时间线和关联工单。</span>
        )}
      </div>
    </>
  );
}
