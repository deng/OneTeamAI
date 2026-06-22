import { useState, type Dispatch, SetStateAction } from 'react';
import {
  CustomerFollowUpStatus,
  type ConversationSummaryResponse,
  type CreateCustomerRequest,
  type CustomerResponse,
  type ProjectResponse,
  type TicketResponse,
  type UpdateCustomerRequest,
} from '../../generated/api';
import { Modal } from './Modal';
import { CustomerPanel } from './CustomerPanel';

type CustomerSectionProps = {
  busyAction: string | null;
  canManageCustomers: boolean;
  createCustomerForm: CreateCustomerRequest;
  customerUpdateForm: UpdateCustomerRequest;
  customers: CustomerResponse[];
  customersLoading?: boolean;
  customersError?: string | null;
  onRetryCustomers?: () => void;
  filteredConversations: ConversationSummaryResponse[];
  filteredTickets: TicketResponse[];
  projects: ProjectResponse[];
  selectedCustomer: CustomerResponse | null;
  selectedCustomerId: string;
  onCreateCustomer: () => void;
  onCreateCustomerFormChange: Dispatch<SetStateAction<CreateCustomerRequest>>;
  onCustomerUpdateFormChange: Dispatch<SetStateAction<UpdateCustomerRequest>>;
  onSaveCustomer: () => void;
  onSelectCustomerId: (customerId: string) => void;
  onSelectRelatedConversationId: (conversationId: string) => void;
  onSelectRelatedProjectId: (projectId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
};

export function CustomerSection({
  busyAction,
  canManageCustomers,
  createCustomerForm,
  customerUpdateForm,
  customers,
  customersLoading = false,
  customersError = null,
  onRetryCustomers,
  filteredConversations,
  filteredTickets,
  projects,
  selectedCustomer,
  selectedCustomerId,
  onCreateCustomer,
  onCreateCustomerFormChange,
  onCustomerUpdateFormChange,
  onSaveCustomer,
  onSelectCustomerId,
  onSelectRelatedConversationId,
  onSelectRelatedProjectId,
  onSelectRelatedTicketId,
}: CustomerSectionProps) {
  const [showForm, setShowForm] = useState(false);
  return (
    <>
      <div className="panel-title panel-title-gap">客户</div>
      <button className="secondary-button" type="button" onClick={() => setShowForm(true)}>
        + 创建客户
      </button>

      <Modal open={showForm} onClose={() => setShowForm(false)} title="创建客户">
        <label className="field">
          <span>客户名</span>
          <input
            className="text-input"
            value={createCustomerForm.displayName ?? ''}
            onChange={event =>
              onCreateCustomerFormChange(current => ({ ...current, displayName: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>邮箱</span>
          <input
            className="text-input"
            value={createCustomerForm.email ?? ''}
            onChange={event =>
              onCreateCustomerFormChange(current => ({ ...current, email: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>公司/品牌</span>
          <input
            className="text-input"
            value={createCustomerForm.companyName ?? ''}
            onChange={event =>
              onCreateCustomerFormChange(current => ({ ...current, companyName: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>来源</span>
          <input
            className="text-input"
            value={createCustomerForm.sourceLabel ?? ''}
            onChange={event =>
              onCreateCustomerFormChange(current => ({ ...current, sourceLabel: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>标签</span>
          <input
            className="text-input"
            value={createCustomerForm.tags ?? ''}
            onChange={event =>
              onCreateCustomerFormChange(current => ({ ...current, tags: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>跟进状态</span>
          <select
            className="text-input"
            value={createCustomerForm.followUpStatus ?? CustomerFollowUpStatus.NUMBER_0}
            onChange={event =>
              onCreateCustomerFormChange(current => ({
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
        <label className="field">
          <span>关联项目</span>
          <select
            className="text-input"
            value={createCustomerForm.projectId ?? ''}
            onChange={event =>
              onCreateCustomerFormChange(current => ({ ...current, projectId: event.currentTarget.value }))
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
          <span>备注</span>
          <textarea
            className="text-area"
            rows={2}
            value={createCustomerForm.notes ?? ''}
            onChange={event =>
              onCreateCustomerFormChange(current => ({ ...current, notes: event.currentTarget.value }))
            }
          />
        </label>
        <button
          className="secondary-button"
          disabled={busyAction !== null}
          type="button"
          onClick={() => { onCreateCustomer(); setShowForm(false); }}
        >
          {busyAction === 'create-customer' ? '创建中...' : '创建客户'}
        </button>
      </Modal>

      <CustomerPanel
        customers={customers}
        selectedCustomerId={selectedCustomerId}
        onSelectCustomerId={onSelectCustomerId}
        customersLoading={customersLoading}
        customersError={customersError}
        onRetryCustomers={onRetryCustomers}
        filteredConversations={filteredConversations}
        filteredTickets={filteredTickets}
        onSelectRelatedConversationId={onSelectRelatedConversationId}
        onSelectRelatedProjectId={onSelectRelatedProjectId}
        onSelectRelatedTicketId={onSelectRelatedTicketId}
        projects={projects}
        selectedCustomer={selectedCustomer}
        customerUpdateForm={customerUpdateForm}
        onCustomerUpdateFormChange={onCustomerUpdateFormChange}
        busyAction={busyAction}
        canManageCustomers={canManageCustomers}
        onSaveCustomer={onSaveCustomer}
      />
    </>
  );
}
