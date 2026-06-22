import { useEffect, useRef, useState, type Dispatch, SetStateAction } from 'react';
import type {
  ConciergeAppResponse,
  ConversationSummaryResponse,
  CreateConciergeAppRequest,
  CustomerResponse,
  MemberResponse,
  ProjectResponse,
  TicketResponse,
  UpdateConciergeAppRequest,
} from '../../generated/api';
import { Modal } from './Modal';
import { ConciergePanel } from './ConciergePanel';

type ConciergeSectionProps = {
  busyAction: string | null;
  canManageConciergeApps: boolean;
  conciergeApps: ConciergeAppResponse[];
  conciergeUpdateForm: UpdateConciergeAppRequest;
  conversations: ConversationSummaryResponse[];
  createConciergeForm: CreateConciergeAppRequest;
  customers: CustomerResponse[];
  projects: ProjectResponse[];
  selectedConciergeApp: ConciergeAppResponse | null | undefined;
  selectedConciergeAppId: string;
  teamMembers: MemberResponse[];
  tickets: TicketResponse[];
  onConciergeUpdateFormChange: Dispatch<SetStateAction<UpdateConciergeAppRequest>>;
  onCreateConciergeApp: () => void;
  onCreateConciergeFormChange: Dispatch<SetStateAction<CreateConciergeAppRequest>>;
  onSaveConciergeApp: () => void;
  onSelectConciergeAppId: (conciergeAppId: string) => void;
  onSelectRelatedConversationId: (conversationId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  onSelectRelatedProjectId: (projectId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
};

export function ConciergeSection({
  busyAction,
  canManageConciergeApps,
  conciergeApps,
  conciergeUpdateForm,
  conversations,
  createConciergeForm,
  customers,
  projects,
  selectedConciergeApp,
  selectedConciergeAppId,
  teamMembers,
  tickets,
  onConciergeUpdateFormChange,
  onCreateConciergeApp,
  onCreateConciergeFormChange,
  onSaveConciergeApp,
  onSelectConciergeAppId,
  onSelectRelatedConversationId,
  onSelectRelatedCustomerId,
  onSelectRelatedProjectId,
  onSelectRelatedTicketId,
}: ConciergeSectionProps) {
  const [showForm, setShowForm] = useState(false);
  const prevBusyRef = useRef(busyAction);
  useEffect(() => {
    if (showForm && prevBusyRef.current === 'create-concierge' && busyAction === null) {
      setShowForm(false);
    }
    prevBusyRef.current = busyAction;
  }, [busyAction, showForm]);
  return (
    <>
      <div className="panel-title panel-title-gap">坐台程序</div>
      {canManageConciergeApps ? (
        <button className="secondary-button" type="button" onClick={() => setShowForm(true)}>
          + 创建坐台程序
        </button>
      ) : null}

      <Modal open={showForm} onClose={() => setShowForm(false)} title="创建坐台程序">
        <label className="field">
          <span>名称</span>
          <input
            className="text-input"
            value={createConciergeForm.name ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({ ...current, name: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>绑定项目</span>
          <select
            className="text-input"
            value={createConciergeForm.projectId ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                projectId: event.currentTarget.value,
              }))
            }
          >
            <option value="">请选择项目</option>
            {projects.map(project => (
              <option key={project.id} value={project.id}>
                {project.name}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span>服务范围</span>
          <textarea
            className="text-area"
            rows={2}
            value={createConciergeForm.serviceScope ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                serviceScope: event.currentTarget.value,
              }))
            }
          />
        </label>
        <label className="field">
          <span>欢迎语</span>
          <textarea
            className="text-area"
            rows={2}
            value={createConciergeForm.welcomeMessage ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
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
            value={createConciergeForm.faqScope ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                faqScope: event.currentTarget.value,
              }))
            }
          />
        </label>
        <label className="field">
          <span>填写指引</span>
          <textarea
            className="text-area"
            rows={2}
            value={createConciergeForm.intakeGuidance ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                intakeGuidance: event.currentTarget.value,
              }))
            }
          />
        </label>
        <label className="field">
          <span>建议提问</span>
          <textarea
            className="text-area"
            rows={2}
            value={createConciergeForm.suggestedPrompts ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                suggestedPrompts: event.currentTarget.value,
              }))
            }
          />
        </label>
        <label className="field">
          <span>渠道标识</span>
          <input
            className="text-input"
            value={createConciergeForm.channelLabel ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
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
            value={createConciergeForm.businessHours ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                businessHours: event.currentTarget.value,
              }))
            }
          />
        </label>
        <label className="checkbox-field">
          <input
            checked={createConciergeForm.requireEmail ?? false}
            type="checkbox"
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                requireEmail: event.currentTarget.checked,
              }))
            }
          />
          要求客户填写邮箱
        </label>
        <label className="checkbox-field">
          <input
            checked={createConciergeForm.requirePhoneNumber ?? false}
            type="checkbox"
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                requirePhoneNumber: event.currentTarget.checked,
              }))
            }
          />
          要求客户填写手机号
        </label>
        <label className="field">
          <span>主 AI 员工</span>
          <select
            className="text-input"
            value={createConciergeForm.primaryAiMemberId ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
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
          <span>自动建单策略</span>
          <textarea
            className="text-area"
            rows={2}
            value={createConciergeForm.ticketCreationPolicy ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
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
            value={createConciergeForm.humanHandoffPolicy ?? ''}
            onChange={event =>
              onCreateConciergeFormChange(current => ({
                ...current,
                humanHandoffPolicy: event.currentTarget.value,
              }))
            }
          />
        </label>
        <button
          className="secondary-button"
          disabled={busyAction !== null}
          type="button"
          onClick={onCreateConciergeApp}
        >
          {busyAction === 'create-concierge' ? '创建中...' : '创建坐台程序'}
        </button>
      </Modal>

      <ConciergePanel
        conciergeApps={conciergeApps}
        selectedConciergeAppId={selectedConciergeAppId}
        onSelectConciergeAppId={onSelectConciergeAppId}
        conversations={conversations}
        customers={customers}
        tickets={tickets}
        projects={projects}
        teamMembers={teamMembers}
        selectedConciergeApp={selectedConciergeApp}
        conciergeUpdateForm={conciergeUpdateForm}
        onConciergeUpdateFormChange={onConciergeUpdateFormChange}
        busyAction={busyAction}
        canManageConciergeApps={canManageConciergeApps}
        onSaveConciergeApp={onSaveConciergeApp}
        onSelectRelatedConversationId={onSelectRelatedConversationId}
        onSelectRelatedCustomerId={onSelectRelatedCustomerId}
        onSelectRelatedProjectId={onSelectRelatedProjectId}
        onSelectRelatedTicketId={onSelectRelatedTicketId}
      />
    </>
  );
}
