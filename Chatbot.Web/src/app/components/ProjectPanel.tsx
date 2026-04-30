import { useState, type Dispatch, type SetStateAction } from 'react';
import type {
  ConciergeAppResponse,
  ConversationSummaryResponse,
  CustomerResponse,
  MemberResponse,
  ProjectResponse,
  TicketResponse,
  UpdateProjectRequest,
} from '../../generated/api';
import {
  formatMemberRole,
  formatMemberType,
  formatNullableText,
  formatProjectStatus,
} from '../formatters';
import { LoadingSpinner } from './LoadingSpinner';

type ProjectPanelProps = {
  projects: ProjectResponse[];
  selectedProjectId: string;
  onSelectProjectId: (projectId: string) => void;
  projectsLoading?: boolean;
  projectsError?: string | null;
  onRetryProjects?: () => void;
  conciergeApps: ConciergeAppResponse[];
  onSelectRelatedConciergeAppId: (conciergeAppId: string) => void;
  conversations: ConversationSummaryResponse[];
  onSelectRelatedConversationId: (conversationId: string) => void;
  customers: CustomerResponse[];
  onSelectRelatedCustomerId: (customerId: string) => void;
  teamMembers: MemberResponse[];
  tickets: TicketResponse[];
  onSelectRelatedTicketId: (ticketId: string) => void;
  busyAction: string | null;
  projectUpdateForm: UpdateProjectRequest;
  onProjectUpdateFormChange: Dispatch<SetStateAction<UpdateProjectRequest>>;
  selectedProject: ProjectResponse | null;
  selectedProjectParticipants: MemberResponse[];
  canManageProjects: boolean;
  onRunProjectWorkflow: () => void;
  onSaveProject: () => void;
};

export function ProjectPanel({
  projects,
  selectedProjectId,
  onSelectProjectId,
  projectsLoading = false,
  projectsError = null,
  onRetryProjects,
  conciergeApps,
  onSelectRelatedConciergeAppId,
  conversations,
  onSelectRelatedConversationId,
  customers,
  onSelectRelatedCustomerId,
  teamMembers,
  tickets,
  onSelectRelatedTicketId,
  busyAction,
  projectUpdateForm,
  onProjectUpdateFormChange,
  selectedProject,
  selectedProjectParticipants,
  canManageProjects,
  onRunProjectWorkflow,
  onSaveProject,
}: ProjectPanelProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const filteredProjects = searchQuery
    ? projects.filter(p => (p.name ?? '').toLowerCase().includes(searchQuery.toLowerCase()))
    : projects;
  const relatedConciergeApps = conciergeApps.filter(app => app.projectId === selectedProject?.id);
  const relatedCustomers = customers.filter(customer => customer.projectId === selectedProject?.id);
  const relatedConversationIds = new Set(
    tickets
      .filter(ticket => ticket.projectId === selectedProject?.id && ticket.conversationId)
      .map(ticket => ticket.conversationId as string),
  );
  const relatedConversations = conversations.filter(conversation => relatedConversationIds.has(conversation.id ?? ''));
  const relatedProjectTickets = tickets.filter(ticket => ticket.projectId === selectedProject?.id);

  return (
    <>
      <div className="entity-card">
        <div className="entity-card-title">项目</div>
        {projectsLoading ? (
          <LoadingSpinner text="正在加载项目..." />
        ) : projectsError ? (
          <div className="entity-error">
            <span>{projectsError}</span>
            {onRetryProjects && (
              <button className="retry-button" type="button" onClick={onRetryProjects}>重试</button>
            )}
          </div>
        ) : filteredProjects.length > 0 ? (
          <>
            <input
              className="search-input"
              placeholder="搜索项目..."
              value={searchQuery}
              onChange={e => setSearchQuery(e.currentTarget.value)}
            />
            <div className="entity-chip-list">
            {filteredProjects.map(project => (
              <button
                className={
                  project.id === selectedProjectId
                    ? 'entity-chip entity-chip-button entity-chip-selected'
                    : 'entity-chip entity-chip-button'
                }
                key={project.id}
                type="button"
                onClick={() => onSelectProjectId(project.id ?? '')}
              >
                <strong>{project.name}</strong>
                <span>{formatProjectStatus(project.status)}</span>
                <span>阶段：{formatNullableText(project.stageLabel, '未设置阶段')}</span>
                <span>
                  负责人：
                  {formatNullableText(
                    teamMembers.find(member => member.id === project.leadMemberId)?.displayName,
                    '未设置',
                  )}
                </span>
                <span>
                  参与成员：{project.participantCount ?? 0} · 客户：{project.customerCount ?? 0} · 工单：
                  {project.ticketCount ?? 0}
                </span>
                <span>摘要：{formatNullableText(project.summary, '暂无项目，请先在左侧表单中创建。摘要')}</span>
              </button>
            ))}
          </div>
          </>
        ) : searchQuery && filteredProjects.length === 0 ? (
          <span className="entity-placeholder">没有搜索到匹配的项目。</span>
        ) : (
          <span className="entity-placeholder">暂无项目，请先在左侧表单中创建。</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">项目详情</div>
        {selectedProject ? (
          <div className="entity-card-body">
            <label className="field">
              <span>名称</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={projectUpdateForm.name ?? ''}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({ ...current, name: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>阶段</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={projectUpdateForm.stageLabel ?? ''}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({
                    ...current,
                    stageLabel: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>负责人</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={projectUpdateForm.leadMemberId ?? ''}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({
                    ...current,
                    leadMemberId: event.currentTarget.value || undefined,
                  }))
                }
              >
                <option value="">暂不设置</option>
                {teamMembers.map(member => (
                  <option key={member.id} value={member.id}>
                    {member.displayName} · {formatMemberRole(member.role)}
                  </option>
                ))}
              </select>
            </label>
            <label className="field">
              <span>参与成员</span>
              <select
                multiple
                className="text-input"
                disabled={busyAction !== null || teamMembers.length === 0}
                value={projectUpdateForm.participantMemberIds ?? []}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({
                    ...current,
                    participantMemberIds: Array.from(event.currentTarget.selectedOptions).map(
                      option => option.value,
                    ),
                  }))
                }
              >
                {teamMembers.map(member => (
                  <option key={member.id} value={member.id}>
                    {member.displayName} · {formatMemberType(member.memberType)}
                  </option>
                ))}
              </select>
            </label>
            <label className="field">
              <span>说明</span>
              <textarea
                className="text-area"
                rows={3}
                disabled={busyAction !== null}
                value={projectUpdateForm.description ?? ''}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({
                    ...current,
                    description: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>摘要</span>
              <textarea
                className="text-area"
                rows={3}
                disabled={busyAction !== null}
                value={projectUpdateForm.summary ?? ''}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({
                    ...current,
                    summary: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>风险</span>
              <textarea
                className="text-area"
                rows={2}
                disabled={busyAction !== null}
                value={projectUpdateForm.riskSummary ?? ''}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({
                    ...current,
                    riskSummary: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>下一步</span>
              <textarea
                className="text-area"
                rows={2}
                disabled={busyAction !== null}
                value={projectUpdateForm.nextSteps ?? ''}
                onChange={event =>
                  onProjectUpdateFormChange(current => ({
                    ...current,
                    nextSteps: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <div className="mini-meta">
              <span>客户数：{selectedProject.customerCount ?? 0}</span>
              <span>工单数：{selectedProject.ticketCount ?? 0}</span>
              <span>坐台程序：{relatedConciergeApps.length}</span>
              <span>
                成员：
                {selectedProjectParticipants.length > 0
                  ? selectedProjectParticipants.map(member => member.displayName).join(' / ')
                  : '未设置'}
              </span>
            </div>
            {relatedConciergeApps.length > 0 ? (
              <div className="entity-chip-list">
                {relatedConciergeApps.map(app => (
                  <button
                    className="entity-chip entity-chip-button"
                    key={app.id}
                    type="button"
                    onClick={() => onSelectRelatedConciergeAppId(app.id ?? '')}
                  >
                    <strong>{formatNullableText(app.name, '未命名坐台程序')}</strong>
                    <span>{formatNullableText(app.channelLabel, '未设置渠道')}</span>
                  </button>
                ))}
              </div>
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
            {relatedProjectTickets.length > 0 ? (
              <div className="entity-chip-list">
                {relatedProjectTickets.slice(0, 6).map(ticket => (
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
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManageProjects || !selectedProject.id}
              type="button"
              onClick={onRunProjectWorkflow}
            >
              {busyAction === 'run-project-workflow' ? '运行中...' : '运行项目协作'}
            </button>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManageProjects || !selectedProject.id}
              type="button"
              onClick={onSaveProject}
            >
              {busyAction === 'update-project' ? '保存中...' : '保存项目设置'}
            </button>
          </div>
        ) : (
          <span className="entity-placeholder">选择一个项目后，这里会展示项目阶段、风险和协同成员。</span>
        )}
      </div>
    </>
  );
}
