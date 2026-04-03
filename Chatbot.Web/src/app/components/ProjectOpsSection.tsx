import type { Dispatch, SetStateAction } from 'react';
import type {
  AgentWorkflowResponse,
  ConciergeAppResponse,
  ConversationSummaryResponse,
  CreateConciergeAppRequest,
  CreateProjectRequest,
  CustomerResponse,
  MemberResponse,
  ProjectResponse,
  RunTicketWorkflowRequest,
  TicketResponse,
  UpdateConciergeAppRequest,
  UpdateProjectRequest,
} from '../../generated/api';
import type { WorkflowTemplateItem } from '../types';
import { ConciergeSection } from './ConciergeSection';
import { ProjectSection } from './ProjectSection';
import { WorkflowPanel } from './WorkflowPanel';

type ProjectOpsSectionProps = {
  busyAction: string | null;
  canManage: boolean;
  conciergeApps: ConciergeAppResponse[];
  conciergeUpdateForm: UpdateConciergeAppRequest;
  conversations: ConversationSummaryResponse[];
  createConciergeForm: CreateConciergeAppRequest;
  createProjectForm: CreateProjectRequest;
  customers: CustomerResponse[];
  projectUpdateForm: UpdateProjectRequest;
  projects: ProjectResponse[];
  selectedConciergeApp: ConciergeAppResponse | null | undefined;
  selectedConciergeAppId: string;
  selectedProject: ProjectResponse | null;
  selectedProjectId: string;
  selectedProjectParticipants: MemberResponse[];
  teamMembers: MemberResponse[];
  tickets: TicketResponse[];
  workflowForm: RunTicketWorkflowRequest;
  workflowTemplates: WorkflowTemplateItem[];
  workflowRuns: AgentWorkflowResponse[];
  selectedWorkflow: AgentWorkflowResponse | null;
  selectedWorkflowId: string;
  onConciergeUpdateFormChange: Dispatch<SetStateAction<UpdateConciergeAppRequest>>;
  onCreateConciergeApp: () => void;
  onCreateConciergeFormChange: Dispatch<SetStateAction<CreateConciergeAppRequest>>;
  onCreateProject: () => void;
  onCreateProjectFormChange: Dispatch<SetStateAction<CreateProjectRequest>>;
  onProjectUpdateFormChange: Dispatch<SetStateAction<UpdateProjectRequest>>;
  onRunProjectWorkflow: () => void;
  onSaveConciergeApp: () => void;
  onSaveProject: () => void;
  onSelectConciergeAppId: (conciergeAppId: string) => void;
  onSelectConversationId: (conversationId: string) => void;
  onSelectCustomerId: (customerId: string) => void;
  onSelectProjectId: (projectId: string) => void;
  onSelectTicketId: (ticketId: string) => void;
  onSelectWorkflowId: (workflowId: string) => void;
  onWorkflowFormChange: Dispatch<SetStateAction<RunTicketWorkflowRequest>>;
};

export function ProjectOpsSection({
  busyAction,
  canManage,
  conciergeApps,
  conciergeUpdateForm,
  conversations,
  createConciergeForm,
  createProjectForm,
  customers,
  projectUpdateForm,
  projects,
  selectedConciergeApp,
  selectedConciergeAppId,
  selectedProject,
  selectedProjectId,
  selectedProjectParticipants,
  teamMembers,
  tickets,
  workflowForm,
  workflowTemplates,
  workflowRuns,
  selectedWorkflow,
  selectedWorkflowId,
  onConciergeUpdateFormChange,
  onCreateConciergeApp,
  onCreateConciergeFormChange,
  onCreateProject,
  onCreateProjectFormChange,
  onProjectUpdateFormChange,
  onRunProjectWorkflow,
  onSaveConciergeApp,
  onSaveProject,
  onSelectConciergeAppId,
  onSelectConversationId,
  onSelectCustomerId,
  onSelectProjectId,
  onSelectTicketId,
  onSelectWorkflowId,
  onWorkflowFormChange,
}: ProjectOpsSectionProps) {
  return (
    <>
      <ProjectSection
        busyAction={busyAction}
        canManageProjects={canManage}
        conciergeApps={conciergeApps}
        conversations={conversations}
        createProjectForm={createProjectForm}
        customers={customers}
        projectUpdateForm={projectUpdateForm}
        projects={projects}
        selectedProject={selectedProject}
        selectedProjectId={selectedProjectId}
        selectedProjectParticipants={selectedProjectParticipants}
        teamMembers={teamMembers}
        tickets={tickets}
        onCreateProject={onCreateProject}
        onCreateProjectFormChange={onCreateProjectFormChange}
        onProjectUpdateFormChange={onProjectUpdateFormChange}
        onSaveProject={onSaveProject}
        onSelectRelatedConciergeAppId={onSelectConciergeAppId}
        onSelectRelatedConversationId={onSelectConversationId}
        onSelectRelatedCustomerId={onSelectCustomerId}
        onSelectRelatedTicketId={onSelectTicketId}
        onSelectProjectId={onSelectProjectId}
        onRunProjectWorkflow={onRunProjectWorkflow}
      />

      <ConciergeSection
        busyAction={busyAction}
        canManageConciergeApps={canManage}
        conciergeApps={conciergeApps}
        conciergeUpdateForm={conciergeUpdateForm}
        conversations={conversations}
        createConciergeForm={createConciergeForm}
        customers={customers}
        projects={projects}
        selectedConciergeApp={selectedConciergeApp}
        selectedConciergeAppId={selectedConciergeAppId}
        teamMembers={teamMembers}
        tickets={tickets}
        onConciergeUpdateFormChange={onConciergeUpdateFormChange}
        onCreateConciergeApp={onCreateConciergeApp}
        onCreateConciergeFormChange={onCreateConciergeFormChange}
        onSaveConciergeApp={onSaveConciergeApp}
        onSelectConciergeAppId={onSelectConciergeAppId}
        onSelectRelatedConversationId={onSelectConversationId}
        onSelectRelatedCustomerId={onSelectCustomerId}
        onSelectRelatedProjectId={onSelectProjectId}
        onSelectRelatedTicketId={onSelectTicketId}
      />

      <WorkflowPanel
        currentWorkflowScope="project"
        currentWorkflowScopeLabel={selectedProject?.name ?? '当前项目'}
        currentScopeWorkflowTemplates={workflowTemplates}
        workflowForm={workflowForm}
        onWorkflowFormChange={onWorkflowFormChange}
        aiMembers={teamMembers.filter(member => member.aiProfile)}
        autonomousAiMembers={teamMembers.filter(member => member.aiProfile?.isAutonomous)}
        busyAction={busyAction}
        canRunWorkflow={canManage && Boolean(selectedProject?.id)}
        onRunWorkflow={onRunProjectWorkflow}
        ticketWorkflows={workflowRuns}
        selectedWorkflowId={selectedWorkflowId}
        onSelectWorkflow={onSelectWorkflowId}
        selectedWorkflow={selectedWorkflow}
        onSelectRelatedProjectId={onSelectProjectId}
        onSelectRelatedConversationId={onSelectConversationId}
        onSelectRelatedTicketId={onSelectTicketId}
      />
    </>
  );
}
