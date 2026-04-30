import { useState, type Dispatch, type SetStateAction } from 'react';
import {
  CustomerFollowUpStatus,
  type ConversationSummaryResponse,
  type CustomerResponse,
  type ProjectResponse,
  type TicketResponse,
  type UpdateCustomerRequest,
} from '../../generated/api';
import {
  formatCustomerFollowUpStatus,
  formatCustomerStatus,
  formatDateTime,
  formatExternalSystemType,
  formatNullableText,
  formatRecordSourceType,
} from '../formatters';
import { LoadingSpinner } from './LoadingSpinner';

type CustomerPanelProps = {
  customers: CustomerResponse[];
  selectedCustomerId: string;
  onSelectCustomerId: (customerId: string) => void;
  customersLoading?: boolean;
  customersError?: string | null;
  onRetryCustomers?: () => void;
  filteredConversations: ConversationSummaryResponse[];
  filteredTickets: TicketResponse[];
  onSelectRelatedConversationId: (conversationId: string) => void;
  onSelectRelatedProjectId: (projectId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
  projects: ProjectResponse[];
  selectedCustomer: CustomerResponse | null;
  customerUpdateForm: UpdateCustomerRequest;
  onCustomerUpdateFormChange: Dispatch<SetStateAction<UpdateCustomerRequest>>;
  busyAction: string | null;
  canManageCustomers: boolean;
  onSaveCustomer: () => void;
};

export function CustomerPanel({
  customers,
  selectedCustomerId,
  onSelectCustomerId,
  customersLoading = false,
  customersError = null,
  onRetryCustomers,
  filteredConversations,
  filteredTickets,
  onSelectRelatedConversationId,
  onSelectRelatedProjectId,
  onSelectRelatedTicketId,
  projects,
  selectedCustomer,
  customerUpdateForm,
  onCustomerUpdateFormChange,
  busyAction,
  canManageCustomers,
  onSaveCustomer,
}: CustomerPanelProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const filteredCustomers = searchQuery
    ? customers.filter(
        c =>
          (c.displayName ?? '').toLowerCase().includes(searchQuery.toLowerCase()) ||
          (c.companyName ?? '').toLowerCase().includes(searchQuery.toLowerCase()) ||
          (c.email ?? '').toLowerCase().includes(searchQuery.toLowerCase()),
      )
    : customers;
  return (
    <>
      <div className="entity-card">
        <div className="entity-card-title">客户</div>
        {customersLoading ? (
          <LoadingSpinner text="正在加载客户..." />
        ) : customersError ? (
          <div className="entity-error">
            <span>{customersError}</span>
            {onRetryCustomers && (
              <button className="retry-button" type="button" onClick={onRetryCustomers}>重试</button>
            )}
          </div>
        ) : filteredCustomers.length > 0 ? (
          <>
            <input
              className="search-input"
              placeholder="搜索客户..."
              value={searchQuery}
              onChange={e => setSearchQuery(e.currentTarget.value)}
            />
            <div className="entity-chip-list">
            {filteredCustomers.map(customer => (
              <button
                className={
                  customer.id === selectedCustomerId
                    ? 'entity-chip entity-chip-button entity-chip-selected'
                    : 'entity-chip entity-chip-button'
                }
                key={customer.id}
                type="button"
                onClick={() => onSelectCustomerId(customer.id ?? '')}
              >
                <strong>{customer.displayName}</strong>
                <span>{formatNullableText(customer.companyName, '未设置公司/品牌')}</span>
                <span>{customer.email ?? customer.phoneNumber ?? formatCustomerStatus(customer.status)}</span>
                <span>来源：{formatNullableText(customer.sourceLabel)}</span>
                <span>标签：{formatNullableText(customer.tags, '未设置标签')}</span>
                <span>跟进：{formatCustomerFollowUpStatus(customer.followUpStatus)}</span>
                <span>状态：{formatCustomerStatus(customer.status)}</span>
                <span>
                  数据来源：{formatRecordSourceType(customer.sourceType)}
                  {customer.externalSystemType !== undefined
                    ? ` / ${formatExternalSystemType(customer.externalSystemType)}`
                    : ''}
                </span>
                {customer.externalId ? <span>外部 ID：{customer.externalId}</span> : null}
                <span>
                  项目：{formatNullableText(projects.find(project => project.id === customer.projectId)?.name, '未关联项目')}
                </span>
                <span>最近联系：{formatDateTime(customer.lastContactedAt)}</span>
                <span>备注：{formatNullableText(customer.notes, '暂无备注')}</span>
              </button>
            ))}
          </div>
          </>
        ) : searchQuery && filteredCustomers.length === 0 ? (
          <span className="entity-placeholder">没有搜索到匹配的客户。</span>
        ) : (
          <span className="entity-placeholder">暂无客户，请先在左侧表单中创建。</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">客户详情</div>
        {selectedCustomer ? (
          <div className="entity-card-body">
            <label className="field">
              <span>客户名</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.displayName ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({ ...current, displayName: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>邮箱</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.email ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({ ...current, email: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>手机号</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.phoneNumber ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({
                    ...current,
                    phoneNumber: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>公司/品牌</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.companyName ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({
                    ...current,
                    companyName: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>来源</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.sourceLabel ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({
                    ...current,
                    sourceLabel: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>客户标签</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.tags ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({ ...current, tags: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>跟进状态</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.followUpStatus ?? CustomerFollowUpStatus.NUMBER_0}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({
                    ...current,
                    followUpStatus: Number(event.currentTarget.value) as typeof current.followUpStatus,
                  }))
                }
              >
                <option value={CustomerFollowUpStatus.NUMBER_0}>新客户</option>
                <option value={CustomerFollowUpStatus.NUMBER_1}>跟进中</option>
                <option value={CustomerFollowUpStatus.NUMBER_2}>已确认</option>
                <option value={CustomerFollowUpStatus.NUMBER_3}>持续培育</option>
                <option value={CustomerFollowUpStatus.NUMBER_4}>已流失</option>
              </select>
            </label>
            <div className="mini-meta">
              <span>客户状态：{formatCustomerStatus(selectedCustomer.status)}</span>
              <span>
                数据来源：{formatRecordSourceType(selectedCustomer.sourceType)}
                {selectedCustomer.externalSystemType !== undefined
                  ? ` / ${formatExternalSystemType(selectedCustomer.externalSystemType)}`
                  : ''}
              </span>
              <span>外部 ID：{formatNullableText(selectedCustomer.externalId, '未映射')}</span>
            </div>
            {selectedCustomer.projectId ? (
              <button
                className="secondary-button"
                disabled={busyAction !== null}
                type="button"
                onClick={() => onSelectRelatedProjectId(selectedCustomer.projectId ?? '')}
              >
                跳转到关联项目：
                {projects.find(project => project.id === selectedCustomer.projectId)?.name ?? '未命名项目'}
              </button>
            ) : null}
            {filteredConversations.length > 0 ? (
              <div className="entity-chip-list">
                {filteredConversations.slice(0, 6).map(conversation => (
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
            {filteredTickets.length > 0 ? (
              <div className="entity-chip-list">
                {filteredTickets.slice(0, 6).map(ticket => (
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
            <label className="field">
              <span>关联项目</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={customerUpdateForm.projectId ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({
                    ...current,
                    projectId: event.currentTarget.value || undefined,
                  }))
                }
              >
                <option value="">暂不关联</option>
                {projects.map(project => (
                  <option key={project.id} value={project.id}>
                    {project.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="field">
              <span>最近跟进时间</span>
              <input
                className="text-input"
                type="datetime-local"
                disabled={busyAction !== null}
                value={
                  customerUpdateForm.lastContactedAt
                    ? new Date(customerUpdateForm.lastContactedAt).toISOString().slice(0, 16)
                    : ''
                }
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({
                    ...current,
                    lastContactedAt: event.currentTarget.value
                      ? new Date(event.currentTarget.value)
                      : undefined,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>备注</span>
              <textarea
                className="text-area"
                rows={3}
                disabled={busyAction !== null}
                value={customerUpdateForm.notes ?? ''}
                onChange={event =>
                  onCustomerUpdateFormChange(current => ({ ...current, notes: event.currentTarget.value }))
                }
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManageCustomers || !selectedCustomer.id}
              type="button"
              onClick={onSaveCustomer}
            >
              {busyAction === 'update-customer' ? '保存中...' : '保存客户'}
            </button>
          </div>
        ) : (
          <span className="entity-placeholder">请先选择一个客户</span>
        )}
      </div>
    </>
  );
}
