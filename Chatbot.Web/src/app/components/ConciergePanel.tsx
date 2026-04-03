import type { Dispatch, SetStateAction } from 'react';
import type {
  ConciergeAppResponse,
  ConversationSummaryResponse,
  CustomerResponse,
  MemberResponse,
  ProjectResponse,
  TicketResponse,
  UpdateConciergeAppRequest,
} from '../../generated/api';
import {
  formatConciergeStatus,
  formatNullableText,
} from '../formatters';

type ConciergePanelProps = {
  conciergeApps: ConciergeAppResponse[];
  selectedConciergeAppId: string;
  onSelectConciergeAppId: (conciergeAppId: string) => void;
  conversations: ConversationSummaryResponse[];
  customers: CustomerResponse[];
  onSelectRelatedProjectId: (projectId: string) => void;
  onSelectRelatedConversationId: (conversationId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
  projects: ProjectResponse[];
  teamMembers: MemberResponse[];
  tickets: TicketResponse[];
  selectedConciergeApp: ConciergeAppResponse | null | undefined;
  conciergeUpdateForm: UpdateConciergeAppRequest;
  onConciergeUpdateFormChange: Dispatch<SetStateAction<UpdateConciergeAppRequest>>;
  busyAction: string | null;
  canManageConciergeApps: boolean;
  onSaveConciergeApp: () => void;
};

export function ConciergePanel({
  conciergeApps,
  selectedConciergeAppId,
  onSelectConciergeAppId,
  conversations,
  customers,
  onSelectRelatedProjectId,
  onSelectRelatedConversationId,
  onSelectRelatedCustomerId,
  onSelectRelatedTicketId,
  projects,
  teamMembers,
  tickets,
  selectedConciergeApp,
  conciergeUpdateForm,
  onConciergeUpdateFormChange,
  busyAction,
  canManageConciergeApps,
  onSaveConciergeApp,
}: ConciergePanelProps) {
  const relatedConversations = conversations.filter(
    conversation => conversation.conciergeAppId === selectedConciergeApp?.id,
  );
  const relatedCustomerIds = new Set(
    relatedConversations
      .map(conversation => conversation.customerId)
      .filter((customerId): customerId is string => Boolean(customerId)),
  );
  const relatedCustomers = customers.filter(customer => relatedCustomerIds.has(customer.id ?? ''));
  const relatedTickets = tickets.filter(ticket => ticket.conciergeAppId === selectedConciergeApp?.id);

  return (
    <>
      <div className="entity-card">
        <div className="entity-card-title">坐台程序</div>
        {conciergeApps.length > 0 ? (
          <div className="entity-chip-list">
            {conciergeApps.map(appEntity => (
              (() => {
                const appConversations = conversations.filter(conversation => conversation.conciergeAppId === appEntity.id);
                const appCustomerIds = new Set(
                  appConversations
                    .map(conversation => conversation.customerId)
                    .filter((customerId): customerId is string => Boolean(customerId)),
                );
                const appTickets = tickets.filter(ticket => ticket.conciergeAppId === appEntity.id);

                return (
                  <button
                    className={
                      appEntity.id === selectedConciergeAppId
                        ? 'entity-chip entity-chip-button entity-chip-selected'
                        : 'entity-chip entity-chip-button'
                    }
                    key={appEntity.id}
                    type="button"
                    onClick={() => onSelectConciergeAppId(appEntity.id ?? '')}
                  >
                    <strong>{appEntity.name}</strong>
                    <span>{formatConciergeStatus(appEntity.status)}</span>
                    <span>
                      项目：
                      {projects.find(project => project.id === appEntity.projectId)?.name ?? '未关联'}
                    </span>
                    <span>
                      主 AI：
                      {teamMembers.find(member => member.id === appEntity.primaryAiMemberId)?.displayName ?? '未绑定'}
                    </span>
                    <span>
                      客户：{appCustomerIds.size} · 会话：{appConversations.length} · 工单：{appTickets.length}
                    </span>
                    <span>渠道：{formatNullableText(appEntity.channelLabel, '未设置渠道')}</span>
                    <span>服务时间：{formatNullableText(appEntity.businessHours, '未设置服务时间')}</span>
                    <span>服务范围：{formatNullableText(appEntity.serviceScope)}</span>
                    <span>欢迎语：{formatNullableText(appEntity.welcomeMessage)}</span>
                    <span>FAQ 范围：{formatNullableText(appEntity.faqScope)}</span>
                    <span>自动建单：{formatNullableText(appEntity.ticketCreationPolicy)}</span>
                    <span>转人工：{formatNullableText(appEntity.humanHandoffPolicy)}</span>
                  </button>
                );
              })()
            ))}
          </div>
        ) : (
          <span className="entity-placeholder">暂无坐台程序</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">坐台程序详情</div>
        {selectedConciergeApp ? (
          <div className="entity-card-body">
            <label className="field">
              <span>名称</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={conciergeUpdateForm.name ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({ ...current, name: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>状态</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={conciergeUpdateForm.status ?? 0}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    status: Number(event.currentTarget.value) as NonNullable<UpdateConciergeAppRequest['status']>,
                  }))
                }
              >
                <option value={0}>草稿</option>
                <option value={1}>已启用</option>
                <option value={2}>已停用</option>
                <option value={3}>已归档</option>
              </select>
            </label>
            <label className="field">
              <span>主 AI 员工</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={conciergeUpdateForm.primaryAiMemberId ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    primaryAiMemberId: event.currentTarget.value,
                  }))
                }
              >
                <option value="">暂不绑定</option>
                {teamMembers
                  .filter(member => member.aiProfile)
                  .map(member => (
                    <option key={member.id} value={member.id}>
                      {member.displayName}
                    </option>
                  ))}
              </select>
            </label>
            <label className="field">
              <span>渠道标识</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={conciergeUpdateForm.channelLabel ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    channelLabel: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>服务时间</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={conciergeUpdateForm.businessHours ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    businessHours: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>欢迎语</span>
              <textarea
                className="text-area"
                rows={3}
                disabled={busyAction !== null}
                value={conciergeUpdateForm.welcomeMessage ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    welcomeMessage: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>FAQ 范围</span>
              <textarea
                className="text-area"
                rows={2}
                disabled={busyAction !== null}
                value={conciergeUpdateForm.faqScope ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    faqScope: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>服务范围</span>
              <textarea
                className="text-area"
                rows={2}
                disabled={busyAction !== null}
                value={conciergeUpdateForm.serviceScope ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    serviceScope: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>自动建单策略</span>
              <textarea
                className="text-area"
                rows={2}
                disabled={busyAction !== null}
                value={conciergeUpdateForm.ticketCreationPolicy ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    ticketCreationPolicy: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>转人工策略</span>
              <textarea
                className="text-area"
                rows={2}
                disabled={busyAction !== null}
                value={conciergeUpdateForm.humanHandoffPolicy ?? ''}
                onChange={event =>
                  onConciergeUpdateFormChange(current => ({
                    ...current,
                    humanHandoffPolicy: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManageConciergeApps || !selectedConciergeApp.id}
              type="button"
              onClick={onSaveConciergeApp}
            >
              {busyAction === 'update-concierge' ? '保存中...' : '保存坐台程序'}
            </button>
            {selectedConciergeApp.projectId ? (
              <button
                className="secondary-button"
                disabled={busyAction !== null}
                type="button"
                onClick={() => onSelectRelatedProjectId(selectedConciergeApp.projectId ?? '')}
              >
                跳转到绑定项目：
                {projects.find(project => project.id === selectedConciergeApp.projectId)?.name ?? '未命名项目'}
              </button>
            ) : null}
            {relatedCustomers.length > 0 ? (
              <div className="entity-chip-list">
                {relatedCustomers.slice(0, 6).map(customer => (
                  <button
                    className="entity-chip entity-chip-button"
                    key={customer.id}
                    type="button"
                    onClick={() => onSelectRelatedCustomerId(customer.id ?? '')}
                  >
                    <strong>{formatNullableText(customer.displayName, '未命名客户')}</strong>
                    <span>{formatNullableText(customer.companyName, '未设置公司/品牌')}</span>
                  </button>
                ))}
              </div>
            ) : null}
            {relatedConversations.length > 0 ? (
              <div className="entity-chip-list">
                {relatedConversations.slice(0, 6).map(conversation => (
                  <button
                    className="entity-chip entity-chip-button"
                    key={conversation.id}
                    type="button"
                    onClick={() => onSelectRelatedConversationId(conversation.id ?? '')}
                  >
                    <strong>{formatNullableText(conversation.customerName, '未命名会话')}</strong>
                    <span>{formatNullableText(conversation.latestMessage, '暂无最新消息')}</span>
                  </button>
                ))}
              </div>
            ) : null}
            {relatedTickets.length > 0 ? (
              <div className="entity-chip-list">
                {relatedTickets.slice(0, 6).map(ticket => (
                  <button
                    className="entity-chip entity-chip-button"
                    key={ticket.id}
                    type="button"
                    onClick={() => onSelectRelatedTicketId(ticket.id ?? '')}
                  >
                    <strong>{formatNullableText(ticket.title, '未命名工单')}</strong>
                    <span>{formatNullableText(ticket.customerName, '未关联客户')}</span>
                  </button>
                ))}
              </div>
            ) : null}
            {selectedConciergeApp.id ? (
              <a
                className="secondary-button"
                href={`${window.location.origin}${window.location.pathname}?view=concierge&appId=${selectedConciergeApp.id}`}
                target="_blank"
                rel="noreferrer"
              >
                打开客户入口
              </a>
            ) : null}
          </div>
        ) : (
          <span className="entity-placeholder">请先选择一个坐台程序</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">客户入口预览</div>
        {selectedConciergeApp ? (
          <div className="concierge-preview">
            <div className="concierge-preview-hero">
              <span className="status-pill">{formatConciergeStatus(selectedConciergeApp.status)}</span>
              <strong>{formatNullableText(selectedConciergeApp.name, '未命名坐台程序')}</strong>
              <span>
                {formatNullableText(selectedConciergeApp.channelLabel, '未设置渠道')} ·{' '}
                {formatNullableText(selectedConciergeApp.businessHours, '未设置服务时间')}
              </span>
            </div>
            <div className="concierge-preview-bubble">
              {formatNullableText(
                selectedConciergeApp.welcomeMessage,
                '你好，欢迎来到我们的接待台，请先告诉我你的需求。',
              )}
            </div>
            <div className="concierge-preview-grid">
              <div className="entity-chip">
                <strong>服务范围</strong>
                <span>{formatNullableText(selectedConciergeApp.serviceScope)}</span>
              </div>
              <div className="entity-chip">
                <strong>FAQ 范围</strong>
                <span>{formatNullableText(selectedConciergeApp.faqScope)}</span>
              </div>
              <div className="entity-chip">
                <strong>自动建单规则</strong>
                <span>{formatNullableText(selectedConciergeApp.ticketCreationPolicy)}</span>
              </div>
              <div className="entity-chip">
                <strong>转人工规则</strong>
                <span>{formatNullableText(selectedConciergeApp.humanHandoffPolicy)}</span>
              </div>
            </div>
            <div className="mini-meta">
              <span>
                对外项目：{projects.find(project => project.id === selectedConciergeApp.projectId)?.name ?? '未关联'}
              </span>
              <span>
                接待 AI：
                {teamMembers.find(member => member.id === selectedConciergeApp.primaryAiMemberId)?.displayName ??
                  '未绑定'}
              </span>
              <span>
                客户：{relatedCustomers.length} · 会话：{relatedConversations.length} · 工单：{relatedTickets.length}
              </span>
            </div>
            {selectedConciergeApp.projectId ? (
              <button
                className="secondary-button"
                type="button"
                onClick={() => onSelectRelatedProjectId(selectedConciergeApp.projectId ?? '')}
              >
                查看关联项目
              </button>
            ) : null}
            {relatedCustomers.length > 0 ? (
              <button
                className="secondary-button"
                type="button"
                onClick={() => onSelectRelatedCustomerId(relatedCustomers[0]?.id ?? '')}
              >
                查看首个相关客户
              </button>
            ) : null}
            {relatedConversations.length > 0 ? (
              <button
                className="secondary-button"
                type="button"
                onClick={() => onSelectRelatedConversationId(relatedConversations[0]?.id ?? '')}
              >
                查看首个相关会话
              </button>
            ) : null}
            {relatedTickets.length > 0 ? (
              <button
                className="secondary-button"
                type="button"
                onClick={() => onSelectRelatedTicketId(relatedTickets[0]?.id ?? '')}
              >
                查看首个相关工单
              </button>
            ) : null}
          </div>
        ) : (
          <span className="entity-placeholder">请先选择一个坐台程序</span>
        )}
      </div>
    </>
  );
}
