import { useState, type Dispatch, SetStateAction } from 'react';
import type {
  ConciergeAppResponse,
  ConversationSummaryResponse,
  CreateProjectRequest,
  CustomerResponse,
  MemberResponse,
  ProjectResponse,
  TicketResponse,
  UpdateProjectRequest,
} from '../../generated/api';
import { Modal } from './Modal';
import { ProjectPanel } from './ProjectPanel';

type ProjectSectionProps = {
  busyAction: string | null;
  canManageProjects: boolean;
  conciergeApps: ConciergeAppResponse[];
  conversations: ConversationSummaryResponse[];
  createProjectForm: CreateProjectRequest;
  customers: CustomerResponse[];
  projectUpdateForm: UpdateProjectRequest;
  projects: ProjectResponse[];
  projectsLoading?: boolean;
  projectsError?: string | null;
  onRetryProjects?: () => void;
  selectedProject: ProjectResponse | null;
  selectedProjectId: string;
  selectedProjectParticipants: MemberResponse[];
  teamMembers: MemberResponse[];
  tickets: TicketResponse[];
  onCreateProject: () => void;
  onCreateProjectFormChange: Dispatch<SetStateAction<CreateProjectRequest>>;
  onProjectUpdateFormChange: Dispatch<SetStateAction<UpdateProjectRequest>>;
  onRunProjectWorkflow: () => void;
  onSaveProject: () => void;
  onSelectRelatedConciergeAppId: (conciergeAppId: string) => void;
  onSelectRelatedConversationId: (conversationId: string) => void;
  onSelectRelatedCustomerId: (customerId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
  onSelectProjectId: (projectId: string) => void;
};

export function ProjectSection({
  busyAction,
  canManageProjects,
  conciergeApps,
  conversations,
  createProjectForm,
  customers,
  projectUpdateForm,
  projects,
  projectsLoading = false,
  projectsError = null,
  onRetryProjects,
  selectedProject,
  selectedProjectId,
  selectedProjectParticipants,
  teamMembers,
  tickets,
  onCreateProject,
  onCreateProjectFormChange,
  onProjectUpdateFormChange,
  onRunProjectWorkflow,
  onSaveProject,
  onSelectRelatedConciergeAppId,
  onSelectRelatedConversationId,
  onSelectRelatedCustomerId,
  onSelectRelatedTicketId,
  onSelectProjectId,
}: ProjectSectionProps) {
  const [showForm, setShowForm] = useState(false);
  return (
    <>
      <div className="panel-title panel-title-gap">项目</div>
      {canManageProjects ? (
        <button className="secondary-button" type="button" onClick={() => setShowForm(true)}>
          + 创建项目
        </button>
      ) : null}

      <Modal open={showForm} onClose={() => setShowForm(false)} title="创建项目">
        <label className="field">
          <span>项目名称</span>
          <input
            className="text-input"
            value={createProjectForm.name ?? ''}
            onChange={event =>
              onCreateProjectFormChange(current => ({ ...current, name: event.currentTarget.value }))
            }
          />
        </label>
        <label className="field">
          <span>阶段</span>
          <input
            className="text-input"
            value={createProjectForm.stageLabel ?? ''}
            onChange={event =>
              onCreateProjectFormChange(current => ({
                ...current,
                stageLabel: event.currentTarget.value,
              }))
            }
          />
        </label>
        <label className="field">
          <span>说明</span>
          <textarea
            className="text-area"
            rows={2}
            value={createProjectForm.description ?? ''}
            onChange={event =>
              onCreateProjectFormChange(current => ({
                ...current,
                description: event.currentTarget.value,
              }))
            }
          />
        </label>
        <button
          className="secondary-button"
          disabled={busyAction !== null}
          type="button"
          onClick={() => { onCreateProject(); setShowForm(false); }}
        >
          {busyAction === 'create-project' ? '创建中...' : '创建项目'}
        </button>
      </Modal>

      <ProjectPanel
        projects={projects}
        selectedProjectId={selectedProjectId}
        onSelectProjectId={onSelectProjectId}
        projectsLoading={projectsLoading}
        projectsError={projectsError}
        onRetryProjects={onRetryProjects}
        conciergeApps={conciergeApps}
        onSelectRelatedConciergeAppId={onSelectRelatedConciergeAppId}
        conversations={conversations}
        onSelectRelatedConversationId={onSelectRelatedConversationId}
        customers={customers}
        onSelectRelatedCustomerId={onSelectRelatedCustomerId}
        teamMembers={teamMembers}
        tickets={tickets}
        onSelectRelatedTicketId={onSelectRelatedTicketId}
        busyAction={busyAction}
        projectUpdateForm={projectUpdateForm}
        onProjectUpdateFormChange={onProjectUpdateFormChange}
        selectedProject={selectedProject}
        selectedProjectParticipants={selectedProjectParticipants}
        canManageProjects={canManageProjects}
        onRunProjectWorkflow={onRunProjectWorkflow}
        onSaveProject={onSaveProject}
      />
    </>
  );
}
