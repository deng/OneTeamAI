import { useState, type Dispatch, type SetStateAction } from 'react';
import {
  TicketPriority,
  TicketStatus,
  type MemberResponse,
  type TicketResponse,
  type UpdateTicketRequest,
} from '../../generated/api';
import {
  formatDateTime,
  formatMemberRole,
  formatNullableText,
  formatTicketActivityType,
  formatTicketPriority,
  formatTicketSlaStatus,
  formatTicketStatus,
} from '../formatters';
import type { TicketDetailItem } from '../types';
import { LoadingSpinner } from './LoadingSpinner';

type TicketPanelProps = {
  filteredTickets: TicketResponse[];
  selectedTicketId: string;
  onSelectTicketId: (ticketId: string) => void;
  ticketsLoading?: boolean;
  ticketsError?: string | null;
  onRetryTickets?: () => void;
  onSelectRelatedConciergeAppId: (conciergeAppId: string) => void;
  onSelectRelatedProjectId: (projectId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  onSelectRelatedConversationId: (conversationId: string) => void;
  teamMembers: MemberResponse[];
  ticketUpdateDrafts: Record<string, UpdateTicketRequest>;
  onTicketDraftsChange: Dispatch<SetStateAction<Record<string, UpdateTicketRequest>>>;
  busyAction: string | null;
  canManageTickets: boolean;
  onSaveTicket: (ticket: TicketResponse) => void;
  selectedTicket: TicketResponse | null;
  ticketDetail: TicketDetailItem | null;
  isTicketDetailLoading: boolean;
  ticketDetailError: string | null;
  ticketCommentDraft: string;
  onTicketCommentDraftChange: (value: string) => void;
  onAddComment: () => void;
};

export function TicketPanel({
  filteredTickets,
  selectedTicketId,
  onSelectTicketId,
  ticketsLoading = false,
  ticketsError = null,
  onRetryTickets,
  onSelectRelatedConciergeAppId,
  onSelectRelatedProjectId,
  onSelectRelatedCustomerId,
  onSelectRelatedConversationId,
  teamMembers,
  ticketUpdateDrafts,
  onTicketDraftsChange,
  busyAction,
  canManageTickets,
  onSaveTicket,
  selectedTicket,
  ticketDetail,
  isTicketDetailLoading,
  ticketDetailError,
  ticketCommentDraft,
  onTicketCommentDraftChange,
  onAddComment,
}: TicketPanelProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const searchedTickets = searchQuery
    ? filteredTickets.filter(t => (t.title ?? '').toLowerCase().includes(searchQuery.toLowerCase()))
    : filteredTickets;
  function buildDraft(ticket: TicketResponse, overrides: Partial<UpdateTicketRequest> = {}): UpdateTicketRequest {
    const ticketId = ticket.id ?? '';
    const current = ticketUpdateDrafts[ticketId];

    return {
      status: current?.status ?? ticket.status ?? TicketStatus.NUMBER_0,
      priority: current?.priority ?? ticket.priority ?? TicketPriority.NUMBER_1,
      assignedMemberId: current?.assignedMemberId ?? ticket.assignedMemberId ?? undefined,
      category: current?.category ?? ticket.category ?? '',
      dueAt: current?.dueAt ?? ticket.dueAt ?? undefined,
      resolutionSummary: current?.resolutionSummary ?? ticket.resolutionSummary ?? '',
      activityNote: current?.activityNote ?? '',
      ...overrides,
    };
  }

  function updateDraft(ticket: TicketResponse, overrides: Partial<UpdateTicketRequest>) {
    const ticketId = ticket.id ?? '';
    onTicketDraftsChange(current => ({
      ...current,
      [ticketId]: buildDraft(ticket, overrides),
    }));
  }

  return (
    <>
      <div className="entity-card">
        <div className="entity-card-title">工单</div>
        {ticketsLoading ? (
          <LoadingSpinner text="正在加载工单..." />
        ) : ticketsError ? (
          <div className="entity-error">
            <span>{ticketsError}</span>
            {onRetryTickets && (
              <button className="retry-button" type="button" onClick={onRetryTickets}>重试</button>
            )}
          </div>
        ) : searchedTickets.length > 0 ? (
          <>
            <input
              className="search-input"
              placeholder="搜索工单..."
              value={searchQuery}
              onChange={e => setSearchQuery(e.currentTarget.value)}
            />
            <div className="entity-chip-list">
            {searchedTickets.map(ticket => (
              <div
                className={ticket.id === selectedTicketId ? 'entity-chip entity-chip-selected' : 'entity-chip'}
                key={ticket.id}
              >
                <strong>{ticket.title}</strong>
                <span>{formatTicketPriority(ticket.priority)} / {formatTicketStatus(ticket.status)}</span>
                <span>SLA：{formatTicketSlaStatus(ticket.dueAt, ticket.resolvedAt, ticket.status)}</span>
                <span>客户：{formatNullableText(ticket.customerName, '未关联客户')}</span>
                <span>负责人：{formatNullableText(ticket.assignedMemberName, '未分配')}</span>
                <button
                  className="secondary-button"
                  type="button"
                  onClick={() => onSelectTicketId(ticket.id ?? '')}
                >
                  查看详情
                </button>
                <label className="field">
                  <span>状态</span>
                  <select
                    className="text-input"
                    disabled={busyAction !== null || !canManageTickets || !ticket.id}
                    value={ticketUpdateDrafts[ticket.id ?? '']?.status ?? TicketStatus.NUMBER_0}
                    onChange={event =>
                      updateDraft(ticket, {
                        status: Number(event.currentTarget.value) as UpdateTicketRequest['status'],
                      })
                    }
                  >
                    <option value={TicketStatus.NUMBER_0}>待处理</option>
                    <option value={TicketStatus.NUMBER_1}>处理中</option>
                    <option value={TicketStatus.NUMBER_2}>待确认</option>
                    <option value={TicketStatus.NUMBER_3}>已完成</option>
                    <option value={TicketStatus.NUMBER_4}>已关闭</option>
                  </select>
                </label>
                <label className="field">
                  <span>优先级</span>
                  <select
                    className="text-input"
                    disabled={busyAction !== null || !canManageTickets || !ticket.id}
                    value={ticketUpdateDrafts[ticket.id ?? '']?.priority ?? TicketPriority.NUMBER_1}
                    onChange={event =>
                      updateDraft(ticket, {
                        priority: Number(event.currentTarget.value) as UpdateTicketRequest['priority'],
                      })
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
                    disabled={busyAction !== null || !canManageTickets || !ticket.id}
                    value={ticketUpdateDrafts[ticket.id ?? '']?.assignedMemberId ?? ''}
                    onChange={event =>
                      updateDraft(ticket, {
                        assignedMemberId: event.currentTarget.value || undefined,
                      })
                    }
                  >
                    <option value="">未分配</option>
                    {teamMembers.map(member => (
                      <option key={member.id} value={member.id}>
                        {member.displayName} ·{' '}
                        {formatNullableText(member.aiProfile?.jobTitle ?? member.title, formatMemberRole(member.role))}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="field">
                  <span>分类</span>
                  <input
                    className="text-input"
                    disabled={busyAction !== null || !canManageTickets || !ticket.id}
                    value={ticketUpdateDrafts[ticket.id ?? '']?.category ?? ticket.category ?? ''}
                    onChange={event =>
                      updateDraft(ticket, {
                        category: event.currentTarget.value,
                      })
                    }
                  />
                </label>
                <label className="field">
                  <span>期望处理时间</span>
                  <input
                    className="text-input"
                    type="datetime-local"
                    disabled={busyAction !== null || !canManageTickets || !ticket.id}
                    value={
                      ticketUpdateDrafts[ticket.id ?? '']?.dueAt
                        ? new Date(ticketUpdateDrafts[ticket.id ?? '']?.dueAt as Date).toISOString().slice(0, 16)
                        : (ticket.dueAt ? new Date(ticket.dueAt).toISOString().slice(0, 16) : '')
                    }
                    onChange={event =>
                      updateDraft(ticket, {
                        dueAt: event.currentTarget.value ? new Date(event.currentTarget.value) : undefined,
                      })
                    }
                  />
                </label>
                <label className="field">
                  <span>处理备注</span>
                  <textarea
                    className="text-area"
                    rows={2}
                    disabled={busyAction !== null || !canManageTickets || !ticket.id}
                    value={ticketUpdateDrafts[ticket.id ?? '']?.activityNote ?? ''}
                    onChange={event =>
                      updateDraft(ticket, {
                        activityNote: event.currentTarget.value,
                      })
                    }
                  />
                </label>
                <label className="field">
                  <span>解决结果</span>
                  <textarea
                    className="text-area"
                    rows={2}
                    disabled={busyAction !== null || !canManageTickets || !ticket.id}
                    value={ticketUpdateDrafts[ticket.id ?? '']?.resolutionSummary ?? ticket.resolutionSummary ?? ''}
                    onChange={event =>
                      updateDraft(ticket, {
                        resolutionSummary: event.currentTarget.value,
                      })
                    }
                  />
                </label>
                <button
                  className="secondary-button"
                  disabled={busyAction !== null || !canManageTickets || !ticket.id}
                  type="button"
                  onClick={() => onSaveTicket(ticket)}
                >
                  {busyAction === 'update-ticket' ? '保存中...' : '保存工单'}
                </button>
              </div>
            ))}
          </div>
          </>
        ) : searchQuery && searchedTickets.length === 0 ? (
          <span className="entity-placeholder">没有搜索到匹配的工单。</span>
        ) : (
          <span className="entity-placeholder">暂无工单，请先在会话详情中创建。</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">当前工单详情</div>
        {selectedTicket && ticketDetail ? (
          <div className="entity-card-body">
            <strong>{ticketDetail.title}</strong>
            <span>{ticketDetail.summary}</span>
            <span>状态：{formatTicketStatus(ticketDetail.status)} · 优先级：{formatTicketPriority(ticketDetail.priority)}</span>
            <span>SLA：{formatTicketSlaStatus(ticketDetail.dueAt, ticketDetail.resolvedAt, ticketDetail.status)}</span>
            <span>分类：{formatNullableText(ticketDetail.category, '未分类')}</span>
            <span>关联客户：{formatNullableText(ticketDetail.customerName, '未关联客户')}</span>
            <span>负责人：{formatNullableText(ticketDetail.assignedMemberName, '未分配')}</span>
            <span>期望处理时间：{formatDateTime(ticketDetail.dueAt)}</span>
            <span>完成时间：{formatDateTime(ticketDetail.resolvedAt)}</span>
            <span>解决结果：{formatNullableText(ticketDetail.resolutionSummary, '尚未填写')}</span>
            <span>最近活动：{formatDateTime(ticketDetail.lastActivityAt)}</span>
            <div className="entity-chip-list">
              {ticketDetail.projectId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedProjectId(ticketDetail.projectId)}
                >
                  <strong>关联项目</strong>
                  <span>{ticketDetail.projectId}</span>
                </button>
              ) : null}
              {ticketDetail.conciergeAppId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedConciergeAppId(ticketDetail.conciergeAppId ?? '')}
                >
                  <strong>关联坐台程序</strong>
                  <span>{ticketDetail.conciergeAppId}</span>
                </button>
              ) : null}
              {ticketDetail.customerId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedCustomerId(ticketDetail.customerId ?? '')}
                >
                  <strong>关联客户</strong>
                  <span>{formatNullableText(ticketDetail.customerName, ticketDetail.customerId)}</span>
                </button>
              ) : null}
              {ticketDetail.conversationId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedConversationId(ticketDetail.conversationId ?? '')}
                >
                  <strong>关联会话</strong>
                  <span>{ticketDetail.conversationId}</span>
                </button>
              ) : null}
            </div>
            <label className="field">
              <span>新增评论</span>
              <textarea
                className="text-area"
                rows={2}
                disabled={busyAction !== null}
                value={ticketCommentDraft}
                onChange={event => onTicketCommentDraftChange(event.currentTarget.value)}
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManageTickets || !selectedTicket.id}
              type="button"
              onClick={onAddComment}
            >
              {busyAction === 'comment-ticket' ? '提交中...' : '添加评论'}
            </button>
            <div className="detail-timeline">
              {ticketDetail.activities.length > 0 ? (
                ticketDetail.activities.map(activity => (
                  <div className="detail-message" key={activity.id}>
                    <strong>{formatTicketActivityType(activity.activityType)} · {activity.summary}</strong>
                    <span>
                      {formatNullableText(activity.actorMemberName ?? activity.actorUserName, '系统')} · {formatDateTime(activity.createdAt)}
                    </span>
                    <span>{formatNullableText(activity.detail, '无补充内容')}</span>
                  </div>
                ))
              ) : (
                <span className="entity-placeholder">这张工单还没有处理记录。</span>
              )}
            </div>
          </div>
        ) : isTicketDetailLoading ? (
          <span className="entity-placeholder">正在加载工单详情...</span>
        ) : ticketDetailError ? (
          <span className="entity-placeholder">{ticketDetailError}</span>
        ) : (
          <span className="entity-placeholder">选择一张工单后，这里会展示更完整的工单摘要。</span>
        )}
      </div>
    </>
  );
}
