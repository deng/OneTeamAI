import type { Dispatch, SetStateAction } from 'react';
import type {
  AgentWorkflowResponse,
  ConciergeAppResponse,
  ConversationDetailResponse,
  ConversationSummaryResponse,
  CreateConciergeAppRequest,
  CreateConversationRequest,
  CreateCustomerRequest,
  CreateProjectRequest,
  CreateTicketRequest,
  CustomerResponse,
  MemberResponse,
  ProjectResponse,
  RunTicketWorkflowRequest,
  TicketResponse,
  UpdateConciergeAppRequest,
  UpdateCustomerRequest,
  UpdateProjectRequest,
  UpdateTicketRequest,
} from '../../generated/api';
import type { TicketDetailItem, WorkflowTemplateItem } from '../types';
import { CustomerOpsSection } from './CustomerOpsSection';
import { ProjectOpsSection } from './ProjectOpsSection';

type BusinessWorkspaceSectionProps = {
  autoRunConversationWorkflow: boolean;
  autoRunTicketWorkflow: boolean;
  busyAction: string | null;
  canManage: boolean;
  conciergeApps: ConciergeAppResponse[];
  conciergeUpdateForm: UpdateConciergeAppRequest;
  conversationDetail: ConversationDetailResponse | null;
  conversationDetailError: string | null;
  conversations: ConversationSummaryResponse[];
  createConciergeForm: CreateConciergeAppRequest;
  createConversationForm: CreateConversationRequest;
  createCustomerForm: CreateCustomerRequest;
  createProjectForm: CreateProjectRequest;
  createTicketForm: CreateTicketRequest;
  allTickets: TicketResponse[];
  customerUpdateForm: UpdateCustomerRequest;
  customers: CustomerResponse[];
  customersLoading?: boolean;
  customersError?: string | null;
  onRetryCustomers?: () => void;
  filteredConversations: ConversationSummaryResponse[];
  conversationsLoading?: boolean;
  conversationsError?: string | null;
  onRetryConversations?: () => void;
  filteredTickets: TicketResponse[];
  ticketsLoading?: boolean;
  ticketsError?: string | null;
  onRetryTickets?: () => void;
  isConversationDetailLoading: boolean;
  isTicketDetailLoading: boolean;
  projectUpdateForm: UpdateProjectRequest;
  projects: ProjectResponse[];
  projectsLoading?: boolean;
  projectsError?: string | null;
  onRetryProjects?: () => void;
  relatedTickets: TicketResponse[];
  selectedConciergeApp: ConciergeAppResponse | null | undefined;
  selectedConciergeAppId: string;
  selectedConversation: ConversationSummaryResponse | null;
  selectedConversationId: string;
  selectedCustomer: CustomerResponse | null;
  selectedCustomerId: string;
  selectedProject: ProjectResponse | null;
  selectedProjectId: string;
  selectedProjectParticipants: MemberResponse[];
  selectedTicket: TicketResponse | null;
  selectedTicketId: string;
  teamMembers: MemberResponse[];
  ticketCommentDraft: string;
  ticketDetail: TicketDetailItem | null;
  ticketDetailError: string | null;
  ticketUpdateDrafts: Record<string, UpdateTicketRequest>;
  projectWorkflowForm: RunTicketWorkflowRequest;
  projectWorkflowTemplates: WorkflowTemplateItem[];
  projectWorkflowRuns: AgentWorkflowResponse[];
  projectSelectedWorkflow: AgentWorkflowResponse | null;
  projectSelectedWorkflowId: string;
  customerWorkflowForm: RunTicketWorkflowRequest;
  customerWorkflowTemplates: WorkflowTemplateItem[];
  customerWorkflowRuns: AgentWorkflowResponse[];
  customerSelectedWorkflow: AgentWorkflowResponse | null;
  customerSelectedWorkflowId: string;
  customerWorkflowScope: 'ticket' | 'conversation' | null;
  customerWorkflowScopeLabel: string;
  onConciergeUpdateFormChange: Dispatch<SetStateAction<UpdateConciergeAppRequest>>;
  onCreateConciergeApp: () => void;
  onCreateConciergeFormChange: Dispatch<SetStateAction<CreateConciergeAppRequest>>;
  onCreateConversation: () => void;
  onCreateConversationFormChange: Dispatch<SetStateAction<CreateConversationRequest>>;
  onCreateCustomer: () => void;
  onCreateCustomerFormChange: Dispatch<SetStateAction<CreateCustomerRequest>>;
  onCreateProject: () => void;
  onCreateProjectFormChange: Dispatch<SetStateAction<CreateProjectRequest>>;
  onCreateTicket: () => void;
  onCreateTicketFormChange: Dispatch<SetStateAction<CreateTicketRequest>>;
  onCustomerUpdateFormChange: Dispatch<SetStateAction<UpdateCustomerRequest>>;
  onProjectUpdateFormChange: Dispatch<SetStateAction<UpdateProjectRequest>>;
  onRunConversationWorkflow: () => void;
  onRunProjectWorkflow: () => void;
  onSaveConciergeApp: () => void;
  onSaveCustomer: () => void;
  onSaveProject: () => void;
  onSaveTicket: (ticket: TicketResponse) => void;
  onSelectConciergeAppId: (conciergeAppId: string) => void;
  onSelectConversationId: (conversationId: string) => void;
  onSelectCustomerId: (customerId: string) => void;
  onSelectProjectId: (projectId: string) => void;
  onSelectTicketId: (ticketId: string) => void;
  onProjectSelectWorkflowId: (workflowId: string) => void;
  onCustomerSelectWorkflowId: (workflowId: string) => void;
  onTicketCommentDraftChange: (value: string) => void;
  onTicketDraftsChange: Dispatch<SetStateAction<Record<string, UpdateTicketRequest>>>;
  onAddTicketComment: () => void;
  onAutoRunConversationWorkflowChange: (value: boolean) => void;
  onAutoRunTicketWorkflowChange: (value: boolean) => void;
  onProjectWorkflowFormChange: Dispatch<SetStateAction<RunTicketWorkflowRequest>>;
  onCustomerWorkflowFormChange: Dispatch<SetStateAction<RunTicketWorkflowRequest>>;
};

export function BusinessWorkspaceSection({
  autoRunConversationWorkflow,
  autoRunTicketWorkflow,
  busyAction,
  canManage,
  conciergeApps,
  conciergeUpdateForm,
  conversationDetail,
  conversationDetailError,
  conversations,
  createConciergeForm,
  createConversationForm,
  createCustomerForm,
  createProjectForm,
  createTicketForm,
  allTickets,
  customerUpdateForm,
  customers,
  customersLoading = false,
  customersError = null,
  onRetryCustomers,
  filteredConversations,
  conversationsLoading = false,
  conversationsError = null,
  onRetryConversations,
  filteredTickets,
  ticketsLoading = false,
  ticketsError = null,
  onRetryTickets,
  isConversationDetailLoading,
  isTicketDetailLoading,
  projectUpdateForm,
  projects,
  projectsLoading = false,
  projectsError = null,
  onRetryProjects,
  relatedTickets,
  selectedConciergeApp,
  selectedConciergeAppId,
  selectedConversation,
  selectedConversationId,
  selectedCustomer,
  selectedCustomerId,
  selectedProject,
  selectedProjectId,
  selectedProjectParticipants,
  selectedTicket,
  selectedTicketId,
  teamMembers,
  ticketCommentDraft,
  ticketDetail,
  ticketDetailError,
  ticketUpdateDrafts,
  projectWorkflowForm,
  projectWorkflowTemplates,
  projectWorkflowRuns,
  projectSelectedWorkflow,
  projectSelectedWorkflowId,
  customerWorkflowForm,
  customerWorkflowTemplates,
  customerWorkflowRuns,
  customerSelectedWorkflow,
  customerSelectedWorkflowId,
  customerWorkflowScope,
  customerWorkflowScopeLabel,
  onConciergeUpdateFormChange,
  onCreateConciergeApp,
  onCreateConciergeFormChange,
  onCreateConversation,
  onCreateConversationFormChange,
  onCreateCustomer,
  onCreateCustomerFormChange,
  onCreateProject,
  onCreateProjectFormChange,
  onCreateTicket,
  onCreateTicketFormChange,
  onCustomerUpdateFormChange,
  onProjectUpdateFormChange,
  onRunConversationWorkflow,
  onRunProjectWorkflow,
  onSaveConciergeApp,
  onSaveCustomer,
  onSaveProject,
  onSaveTicket,
  onSelectConciergeAppId,
  onSelectConversationId,
  onSelectCustomerId,
  onSelectProjectId,
  onSelectTicketId,
  onProjectSelectWorkflowId,
  onCustomerSelectWorkflowId,
  onTicketCommentDraftChange,
  onTicketDraftsChange,
  onAddTicketComment,
  onAutoRunConversationWorkflowChange,
  onAutoRunTicketWorkflowChange,
  onProjectWorkflowFormChange,
  onCustomerWorkflowFormChange,
}: BusinessWorkspaceSectionProps) {
  return (
    <>
      <ProjectOpsSection
        busyAction={busyAction}
        canManage={canManage}
        conciergeApps={conciergeApps}
        conciergeUpdateForm={conciergeUpdateForm}
        conversations={conversations}
        createConciergeForm={createConciergeForm}
        createProjectForm={createProjectForm}
        customers={customers}
        projectUpdateForm={projectUpdateForm}
        projects={projects}
        projectsLoading={projectsLoading}
        projectsError={projectsError}
        onRetryProjects={onRetryProjects}
        selectedConciergeApp={selectedConciergeApp}
        selectedConciergeAppId={selectedConciergeAppId}
        selectedProject={selectedProject}
        selectedProjectId={selectedProjectId}
        selectedProjectParticipants={selectedProjectParticipants}
        teamMembers={teamMembers}
        tickets={allTickets}
        workflowForm={projectWorkflowForm}
        workflowTemplates={projectWorkflowTemplates}
        workflowRuns={projectWorkflowRuns}
        selectedWorkflow={projectSelectedWorkflow}
        selectedWorkflowId={projectSelectedWorkflowId}
        onConciergeUpdateFormChange={onConciergeUpdateFormChange}
        onCreateConciergeApp={onCreateConciergeApp}
        onCreateConciergeFormChange={onCreateConciergeFormChange}
        onCreateProject={onCreateProject}
        onCreateProjectFormChange={onCreateProjectFormChange}
        onProjectUpdateFormChange={onProjectUpdateFormChange}
        onRunProjectWorkflow={onRunProjectWorkflow}
        onSaveConciergeApp={onSaveConciergeApp}
        onSaveProject={onSaveProject}
        onSelectConciergeAppId={onSelectConciergeAppId}
        onSelectConversationId={onSelectConversationId}
        onSelectCustomerId={onSelectCustomerId}
        onSelectProjectId={onSelectProjectId}
        onSelectTicketId={onSelectTicketId}
        onSelectWorkflowId={onProjectSelectWorkflowId}
        onWorkflowFormChange={onProjectWorkflowFormChange}
      />

      <CustomerOpsSection
        autoRunConversationWorkflow={autoRunConversationWorkflow}
        autoRunTicketWorkflow={autoRunTicketWorkflow}
        busyAction={busyAction}
        canManage={canManage}
        conversationDetail={conversationDetail}
        conversationDetailError={conversationDetailError}
        createConversationForm={createConversationForm}
        createCustomerForm={createCustomerForm}
        createTicketForm={createTicketForm}
        customerUpdateForm={customerUpdateForm}
        customers={customers}
        customersLoading={customersLoading}
        customersError={customersError}
        onRetryCustomers={onRetryCustomers}
        filteredConversations={filteredConversations}
        conversationsLoading={conversationsLoading}
        conversationsError={conversationsError}
        onRetryConversations={onRetryConversations}
        filteredTickets={filteredTickets}
        ticketsLoading={ticketsLoading}
        ticketsError={ticketsError}
        onRetryTickets={onRetryTickets}
        isConversationDetailLoading={isConversationDetailLoading}
        isTicketDetailLoading={isTicketDetailLoading}
        projects={projects}
        relatedTickets={relatedTickets}
        selectedConversation={selectedConversation}
        selectedConversationId={selectedConversationId}
        selectedCustomer={selectedCustomer}
        selectedCustomerId={selectedCustomerId}
        selectedTicket={selectedTicket}
        selectedTicketId={selectedTicketId}
        teamMembers={teamMembers}
        ticketCommentDraft={ticketCommentDraft}
        ticketDetail={ticketDetail}
        ticketDetailError={ticketDetailError}
        ticketUpdateDrafts={ticketUpdateDrafts}
        workflowForm={customerWorkflowForm}
        workflowTemplates={customerWorkflowTemplates}
        workflowRuns={customerWorkflowRuns}
        selectedWorkflow={customerSelectedWorkflow}
        selectedWorkflowId={customerSelectedWorkflowId}
        workflowScope={customerWorkflowScope}
        workflowScopeLabel={customerWorkflowScopeLabel}
        onAddTicketComment={onAddTicketComment}
        onAutoRunConversationWorkflowChange={onAutoRunConversationWorkflowChange}
        onAutoRunTicketWorkflowChange={onAutoRunTicketWorkflowChange}
        onCreateConversation={onCreateConversation}
        onCreateConversationFormChange={onCreateConversationFormChange}
        onCreateCustomer={onCreateCustomer}
        onCreateCustomerFormChange={onCreateCustomerFormChange}
        onCreateTicket={onCreateTicket}
        onCreateTicketFormChange={onCreateTicketFormChange}
        onCustomerUpdateFormChange={onCustomerUpdateFormChange}
        onRunConversationWorkflow={onRunConversationWorkflow}
        onSaveCustomer={onSaveCustomer}
        onSaveTicket={onSaveTicket}
        onSelectConciergeAppId={onSelectConciergeAppId}
        onSelectConversationId={onSelectConversationId}
        onSelectCustomerId={onSelectCustomerId}
        onSelectProjectId={onSelectProjectId}
        onSelectTicketId={onSelectTicketId}
        onSelectWorkflowId={onCustomerSelectWorkflowId}
        onTicketCommentDraftChange={onTicketCommentDraftChange}
        onTicketDraftsChange={onTicketDraftsChange}
        onWorkflowFormChange={onCustomerWorkflowFormChange}
      />
    </>
  );
}
