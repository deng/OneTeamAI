import { createContext, useContext, useEffect, useMemo, type Dispatch, type ReactNode, type SetStateAction } from 'react';
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
  HealthResponse,
  IntegrationConnectionHealthResponse,
  IntegrationConnectionResponse,
  IntegrationPreviewItemResponse,
  MemberResponse,
  ProjectResponse,
  RunTicketWorkflowRequest,
  TeamSummaryResponse,
  TicketResponse,
  UpdateConciergeAppRequest,
  UpdateCustomerRequest,
  UpdateProjectRequest,
  UpdateTicketRequest,
  UserResponse,
  CreateAiMemberRequest,
  CreateHumanMemberRequest,
  CreateIntegrationConnectionRequest,
  CreateInvitationRequest,
  FileKnowledgeItemResponse,
  InvitationResponse,
  MemberRole,
  UpdateTeamRequest,
} from '../generated/api';
import type {
  AiMemberTemplateEditorForm,
  AiMemberTemplateItem,
  AuditLogItem,
  ChatMessage,
  Feedback,
  TicketDetailItem,
  UserSessionItem,
  WorkflowTemplateItem,
} from './types';
import { useWorkspaceAuth } from './useWorkspaceAuth';
import { useWorkspaceChat } from './useWorkspaceChat';
import { useWorkspaceConversations } from './useWorkspaceConversations';
import { useWorkspaceCustomers } from './useWorkspaceCustomers';
import { useWorkspaceIntegrations } from './useWorkspaceIntegrations';
import { useWorkspaceResources } from './useWorkspaceResources';
import { useWorkspaceStatus } from './useWorkspaceStatus';
import { useWorkspaceTeam } from './useWorkspaceTeam';
import { useWorkspaceTeamSettings } from './useWorkspaceTeamSettings';
import { useWorkspaceTickets } from './useWorkspaceTickets';
import { useWorkspaceWorkflows } from './useWorkspaceWorkflows';

// ─── Contexts ───────────────────────────────────────────────────────

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

interface StatusContextValue {
  busyAction: string | null;
  feedback: Feedback | null;
  runAction: RunAction;
  setFeedback: Dispatch<SetStateAction<Feedback | null>>;
}

interface AuthContextValue {
  currentUser: UserResponse | null;
  health: HealthResponse | null;
  token: string | null;
  loginForm: { email: string; password: string };
  registerForm: { email: string; password: string; displayName: string; companyName: string };
  setLoginForm: Dispatch<SetStateAction<{ email: string; password: string }>>;
  setRegisterForm: Dispatch<SetStateAction<{ email: string; password: string; displayName: string; companyName: string }>>;
  handleLogin: () => Promise<void>;
  handleLogout: () => Promise<void>;
  handleRegister: () => Promise<void>;
}

interface TeamContextValue {
  currentTeam: TeamSummaryResponse | null;
  currentTeamId: string;
  refreshTeams: () => Promise<void>;
  teamDescription: string;
  teamName: string;
  teams: TeamSummaryResponse[];
  setCurrentTeamId: (teamId: string) => void;
  setTeamDescription: Dispatch<SetStateAction<string>>;
  setTeamName: Dispatch<SetStateAction<string>>;
  handleCreateTeam: () => Promise<void>;
}

interface ResourcesContextValue {
  conciergeApps: ConciergeAppResponse[];
  conciergeUpdateForm: UpdateConciergeAppRequest;
  createConciergeForm: CreateConciergeAppRequest;
  createProjectForm: CreateProjectRequest;
  isResourcesLoading: boolean;
  resourcesError: string | null;
  refreshWorkspaceData: (teamId: string) => Promise<void>;
  projectUpdateForm: UpdateProjectRequest;
  projects: ProjectResponse[];
  selectedConciergeApp: ConciergeAppResponse | null | undefined;
  selectedConciergeAppId: string;
  selectedProject: ProjectResponse | null;
  selectedProjectId: string;
  selectedProjectParticipants: MemberResponse[];
  teamMembers: MemberResponse[];
  setConciergeUpdateForm: Dispatch<SetStateAction<UpdateConciergeAppRequest>>;
  setCreateConciergeForm: Dispatch<SetStateAction<CreateConciergeAppRequest>>;
  setCreateProjectForm: Dispatch<SetStateAction<CreateProjectRequest>>;
  setProjectUpdateForm: Dispatch<SetStateAction<UpdateProjectRequest>>;
  setSelectedConciergeAppId: Dispatch<SetStateAction<string>>;
  setSelectedProjectId: Dispatch<SetStateAction<string>>;
  handleCreateConciergeApp: () => Promise<void>;
  handleCreateProject: () => Promise<void>;
  handleSaveConciergeApp: () => Promise<void>;
  handleSaveProject: () => Promise<void>;
}

interface CustomersContextValue {
  createCustomerForm: CreateCustomerRequest;
  customerUpdateForm: UpdateCustomerRequest;
  customers: CustomerResponse[];
  customersError: string | null;
  isCustomersLoading: boolean;
  refreshCustomers: (teamId: string) => Promise<void>;
  selectedCustomer: CustomerResponse | null;
  selectedCustomerId: string;
  setCreateCustomerForm: Dispatch<SetStateAction<CreateCustomerRequest>>;
  setCustomerUpdateForm: Dispatch<SetStateAction<UpdateCustomerRequest>>;
  setSelectedCustomerId: Dispatch<SetStateAction<string>>;
  handleCreateCustomer: () => Promise<void>;
  handleSaveCustomer: () => Promise<void>;
}

interface ConversationsContextValue {
  autoRunConversationWorkflow: boolean;
  conversations: ConversationSummaryResponse[];
  conversationDetail: ConversationDetailResponse | null;
  conversationDetailError: string | null;
  conversationsError: string | null;
  createConversationForm: CreateConversationRequest;
  filteredConversations: ConversationSummaryResponse[];
  isConversationDetailLoading: boolean;
  isConversationsLoading: boolean;
  selectedConversation: ConversationSummaryResponse | null;
  selectedConversationId: string;
  setAutoRunConversationWorkflow: Dispatch<SetStateAction<boolean>>;
  setCreateConversationForm: Dispatch<SetStateAction<CreateConversationRequest>>;
  setSelectedConversationId: Dispatch<SetStateAction<string>>;
  handleCreateConversation: () => Promise<void>;
  refreshConversations: () => Promise<void>;
}

interface TicketsContextValue {
  autoRunTicketWorkflow: boolean;
  createTicketForm: CreateTicketRequest;
  filteredTickets: TicketResponse[];
  isTicketDetailLoading: boolean;
  isTicketsLoading: boolean;
  ticketsError: string | null;
  relatedTickets: TicketResponse[];
  refreshTickets: () => Promise<void>;
  tickets: TicketResponse[];
  selectedTicket: TicketResponse | null;
  selectedTicketId: string;
  setAutoRunTicketWorkflow: Dispatch<SetStateAction<boolean>>;
  ticketCommentDraft: string;
  ticketDetail: TicketDetailItem | null;
  ticketDetailError: string | null;
  ticketUpdateDrafts: Record<string, UpdateTicketRequest>;
  setCreateTicketForm: Dispatch<SetStateAction<CreateTicketRequest>>;
  setSelectedTicketId: Dispatch<SetStateAction<string>>;
  setTicketCommentDraft: Dispatch<SetStateAction<string>>;
  setTicketUpdateDrafts: Dispatch<SetStateAction<Record<string, UpdateTicketRequest>>>;
  handleAddComment: () => Promise<void>;
  handleCreateTicket: () => Promise<void>;
  handleSaveTicket: (ticket: TicketResponse) => Promise<void>;
}

interface TeamSettingsContextValue {
  aiMemberForm: CreateAiMemberRequest;
  aiTemplateEditorForm: AiMemberTemplateEditorForm;
  aiTemplateLibrary: ReadonlyArray<AiMemberTemplateItem>;
  aiTemplateOptions: ReadonlyArray<AiMemberTemplateItem>;
  humanMemberForm: CreateHumanMemberRequest;
  invitationForm: CreateInvitationRequest;
  myAuditLogs: AuditLogItem[];
  myInvitations: InvitationResponse[];
  teamAuditLogs: AuditLogItem[];
  teamInvitations: InvitationResponse[];
  teamSettingsForm: UpdateTeamRequest;
  userSessions: UserSessionItem[];
  setAiMemberForm: Dispatch<SetStateAction<CreateAiMemberRequest>>;
  setAiTemplateEditorForm: Dispatch<SetStateAction<AiMemberTemplateEditorForm>>;
  setHumanMemberForm: Dispatch<SetStateAction<CreateHumanMemberRequest>>;
  setInvitationForm: Dispatch<SetStateAction<CreateInvitationRequest>>;
  setTeamSettingsForm: Dispatch<SetStateAction<UpdateTeamRequest>>;
  handleApplyAiTemplate: (templateKey: string) => void;
  handleAcceptInvitation: (invitation: InvitationResponse) => void;
  handleCreateAiMember: () => void;
  handleCreateAiTemplateTemplate: () => void;
  handleCreateHumanMember: () => void;
  handleCreateInvitation: () => void;
  handleDuplicateAiTemplate: (templateId: string) => void;
  handleEditAiTemplate: (templateId: string) => void;
  handleLogoutAll: () => void;
  handleRemoveMember: (member: MemberResponse) => void;
  handleRevokeInvitation: (invitation: InvitationResponse) => void;
  handleRevokeSession: (sessionId: string, isCurrent: boolean) => void;
  handleSaveTeamSettings: () => void;
  handleStartNewAiTemplate: () => void;
  handleToggleAiTemplate: (template: AiMemberTemplateItem) => void;
  handleUpdateMemberRole: (member: MemberResponse, role: MemberRole) => void;
  handleUpdateAiTemplateTemplate: () => void;
  selectedAiTemplateId: string;
}

interface IntegrationsContextValue {
  integrationConnections: IntegrationConnectionResponse[];
  integrationFiles: FileKnowledgeItemResponse[];
  integrationFolderPath: string;
  integrationForm: CreateIntegrationConnectionRequest;
  integrationPreviewCount: number;
  integrationPreviewCustomers: IntegrationPreviewItemResponse[];
  integrationPreviewProjects: IntegrationPreviewItemResponse[];
  integrationPreviewTasks: IntegrationPreviewItemResponse[];
  integrationPreviewTickets: IntegrationPreviewItemResponse[];
  selectedIntegration: IntegrationConnectionResponse | null;
  selectedIntegrationHealth: IntegrationConnectionHealthResponse | null;
  selectedIntegrationId: string;
  setIntegrationFolderPath: Dispatch<SetStateAction<string>>;
  setIntegrationForm: Dispatch<SetStateAction<CreateIntegrationConnectionRequest>>;
  handleCreateIntegration: () => Promise<void>;
  handleImportPreviewCustomer: (externalRecordId: string, forceUpdate?: boolean) => Promise<void>;
  handleImportPreviewProject: (externalRecordId: string, forceUpdate?: boolean) => Promise<void>;
  handleImportPreviewTicket: (externalRecordId: string, forceUpdate?: boolean) => Promise<void>;
  handleRefreshIntegrationPreview: () => Promise<void>;
  handleSelectIntegrationId: (integrationId: string) => void;
  handleValidateIntegration: () => Promise<void>;
}

interface WorkflowsContextValue {
  project: {
    workflowTemplates: WorkflowTemplateItem[];
    workflowRuns: AgentWorkflowResponse[];
    selectedWorkflow: AgentWorkflowResponse | null;
    selectedWorkflowId: string;
    workflowForm: RunTicketWorkflowRequest;
    setSelectedWorkflowId: Dispatch<SetStateAction<string>>;
    setWorkflowForm: Dispatch<SetStateAction<RunTicketWorkflowRequest>>;
    handleRunWorkflow: () => Promise<void>;
  };
  customer: {
    workflowTemplates: WorkflowTemplateItem[];
    workflowRuns: AgentWorkflowResponse[];
    selectedWorkflow: AgentWorkflowResponse | null;
    selectedWorkflowId: string;
    workflowForm: RunTicketWorkflowRequest;
    setSelectedWorkflowId: Dispatch<SetStateAction<string>>;
    setWorkflowForm: Dispatch<SetStateAction<RunTicketWorkflowRequest>>;
    handleRunWorkflow: () => Promise<void>;
    workflowScope: 'ticket' | 'conversation' | null;
    workflowScopeLabel: string;
  };
}

interface ChatContextValue {
  chatError: string | null;
  chatInput: string;
  displayedTexts: Record<string, string>;
  isStreaming: boolean;
  messages: ChatMessage[];
  scrollRef: { current: HTMLDivElement | null };
  setChatInput: Dispatch<SetStateAction<string>>;
  handleSendMessage: () => Promise<void>;
  handleStop: () => void;
}

interface NavigationContextValue {
  navigateToProject: (projectId: string) => void;
  navigateToConciergeApp: (conciergeAppId: string) => void;
  navigateToCustomer: (customerId: string) => void;
  navigateToConversation: (conversationId: string) => void;
  navigateToTicket: (ticketId: string) => void;
  selectProjectWorkflow: (workflowId: string) => void;
  selectCustomerWorkflow: (workflowId: string) => void;
}

interface DerivedContextValue {
  projectsLeadCountByMemberId: Record<string, number>;
  conciergeCountByMemberId: Record<string, number>;
  importedCustomerExternalIds: string[];
  importedProjectExternalIds: string[];
  importedTicketExternalIds: string[];
  integrationAuditLogs: AuditLogItem[];
  mappedCustomers: CustomerResponse[];
  mappedProjects: ProjectResponse[];
  mappedTickets: TicketResponse[];
}

// ─── Context objects ────────────────────────────────────────────────

const StatusCtx = createContext<StatusContextValue | null>(null);
const AuthCtx = createContext<AuthContextValue | null>(null);
const TeamCtx = createContext<TeamContextValue | null>(null);
const ResourcesCtx = createContext<ResourcesContextValue | null>(null);
const CustomersCtx = createContext<CustomersContextValue | null>(null);
const ConversationsCtx = createContext<ConversationsContextValue | null>(null);
const TicketsCtx = createContext<TicketsContextValue | null>(null);
const TeamSettingsCtx = createContext<TeamSettingsContextValue | null>(null);
const IntegrationsCtx = createContext<IntegrationsContextValue | null>(null);
const WorkflowsCtx = createContext<WorkflowsContextValue | null>(null);
const ChatCtx = createContext<ChatContextValue | null>(null);
const NavigationCtx = createContext<NavigationContextValue | null>(null);
const DerivedCtx = createContext<DerivedContextValue | null>(null);

// ─── Provider ───────────────────────────────────────────────────────

export function WorkspaceProvider({ children }: { children: ReactNode }) {
  // 1. Status (no dependencies)
  const { busyAction, feedback, runAction, setFeedback } = useWorkspaceStatus();

  // 2. Auth
  const auth = useWorkspaceAuth({ runAction, setFeedback });

  // 3. Team
  const team = useWorkspaceTeam({ token: auth.token, runAction, setFeedback });

  // 4. Resources
  const resources = useWorkspaceResources({
    currentTeamId: team.currentTeamId,
    token: auth.token,
    runAction,
    setFeedback,
  });

  // 5. Customers
  const customers = useWorkspaceCustomers({
    currentTeamId: team.currentTeamId,
    projects: resources.projects,
    token: auth.token,
    runAction,
    setFeedback,
  });

  // 6. Conversations
  const conversations = useWorkspaceConversations({
    currentTeamId: team.currentTeamId,
    selectedConciergeApp: resources.selectedConciergeApp,
    selectedCustomer: customers.selectedCustomer,
    selectedCustomerId: customers.selectedCustomerId,
    token: auth.token,
    runAction,
    setFeedback,
  });

  // 7. Tickets
  const tickets = useWorkspaceTickets({
    currentTeamId: team.currentTeamId,
    selectedConversation: conversations.selectedConversation,
    selectedConversationId: conversations.selectedConversationId,
    selectedCustomerId: customers.selectedCustomerId,
    token: auth.token,
    runAction,
    setFeedback,
  });

  // 8. Computed values for team settings
  const projectsLeadCountByMemberId = useMemo(
    () =>
      resources.projects.reduce<Record<string, number>>((acc, project) => {
        if (project.leadMemberId) {
          acc[project.leadMemberId] = (acc[project.leadMemberId] ?? 0) + 1;
        }
        return acc;
      }, {}),
    [resources.projects],
  );
  const conciergeCountByMemberId = useMemo(
    () =>
      resources.conciergeApps.reduce<Record<string, number>>((acc, app) => {
        if (app.primaryAiMemberId) {
          acc[app.primaryAiMemberId] = (acc[app.primaryAiMemberId] ?? 0) + 1;
        }
        return acc;
      }, {}),
    [resources.conciergeApps],
  );

  // 9. TeamSettings
  const teamSettings = useWorkspaceTeamSettings({
    currentTeam: team.currentTeam,
    currentTeamId: team.currentTeamId,
    currentUser: auth.currentUser,
    token: auth.token,
    teamMembers: resources.teamMembers,
    projectsLeadCountByMemberId,
    conciergeCountByMemberId,
    refreshWorkspaceData: resources.refreshWorkspaceData,
    refreshTeams: team.refreshTeams,
    runAction,
    setFeedback,
  });

  // 10. Integrations
  const integrations = useWorkspaceIntegrations({
    currentTeamId: team.currentTeamId,
    refreshWorkspaceData: resources.refreshWorkspaceData,
    refreshCustomers: customers.refreshCustomers,
    refreshTickets: tickets.refreshTickets,
    selectedProjectId: resources.selectedProjectId,
    selectedCustomerId: customers.selectedCustomerId,
    token: auth.token,
    runAction,
    setFeedback,
  });

  // 11. Computed values for integrations
  const importedCustomerExternalIds = useMemo(
    () =>
      customers.customers
        .filter(
          c =>
            c.externalSystemType === integrations.selectedIntegration?.externalSystemType
            && Boolean(c.externalId),
        )
        .map(c => c.externalId as string),
    [customers.customers, integrations.selectedIntegration?.externalSystemType],
  );
  const importedProjectExternalIds = useMemo(
    () =>
      resources.projects
        .filter(
          p =>
            p.externalSystemType === integrations.selectedIntegration?.externalSystemType
            && Boolean(p.externalId),
        )
        .map(p => p.externalId as string),
    [resources.projects, integrations.selectedIntegration?.externalSystemType],
  );
  const importedTicketExternalIds = useMemo(
    () =>
      tickets.tickets
        .filter(
          t =>
            t.externalSystemType === integrations.selectedIntegration?.externalSystemType
            && Boolean(t.externalId),
        )
        .map(t => t.externalId as string),
    [tickets.tickets, integrations.selectedIntegration?.externalSystemType],
  );
  const integrationAuditLogs = useMemo(() => {
    const logs = teamSettings.teamAuditLogs.filter(log => log.actionType.startsWith('integration.'));
    const selected = integrations.selectedIntegration;
    if (!selected) {
      return logs;
    }
    return logs.filter(log => {
      if (log.entityId === selected.id) return true;
      return log.summary.includes(selected.name ?? '');
    });
  }, [integrations.selectedIntegration, teamSettings.teamAuditLogs]);
  const mappedCustomers = useMemo(
    () =>
      customers.customers.filter(
        c =>
          c.externalSystemType === integrations.selectedIntegration?.externalSystemType
          && Boolean(c.externalId),
      ),
    [customers.customers, integrations.selectedIntegration?.externalSystemType],
  );
  const mappedProjects = useMemo(
    () =>
      resources.projects.filter(
        p =>
          p.externalSystemType === integrations.selectedIntegration?.externalSystemType
          && Boolean(p.externalId),
      ),
    [resources.projects, integrations.selectedIntegration?.externalSystemType],
  );
  const mappedTickets = useMemo(
    () =>
      tickets.tickets.filter(
        t =>
          t.externalSystemType === integrations.selectedIntegration?.externalSystemType
          && Boolean(t.externalId),
      ),
    [tickets.tickets, integrations.selectedIntegration?.externalSystemType],
  );

  // 12. Workflows (called twice: project scope and customer scope)
  const projectWorkflows = useWorkspaceWorkflows({
    currentTeamId: team.currentTeamId,
    scope: resources.selectedProjectId ? 'project' : null,
    scopeId: resources.selectedProjectId,
    scopeLabel: resources.selectedProject?.name ?? '当前项目',
    teamMembers: resources.teamMembers,
    token: auth.token,
    runAction,
    setFeedback,
  });
  const customerWorkflowScope: 'ticket' | 'conversation' | null = tickets.selectedTicketId
    ? 'ticket'
    : conversations.selectedConversationId
      ? 'conversation'
      : null;
  const customerWorkflowScopeId = tickets.selectedTicketId || conversations.selectedConversationId;
  const customerWorkflowScopeLabel = tickets.selectedTicketId
    ? (tickets.selectedTicket?.title ?? '当前工单')
    : (conversations.selectedConversation?.customerName ?? '当前会话');
  const customerWorkflows = useWorkspaceWorkflows({
    currentTeamId: team.currentTeamId,
    scope: customerWorkflowScope,
    scopeId: customerWorkflowScopeId,
    scopeLabel: customerWorkflowScopeLabel,
    teamMembers: resources.teamMembers,
    token: auth.token,
    runAction,
    setFeedback,
  });

  // 13. Chat (no dependencies)
  const chat = useWorkspaceChat();

  // 14. Navigation functions (coordinate multiple domains)
  const navigateToProject = (projectId: string) => {
    if (!projectId) return;
    resources.setSelectedProjectId(projectId);
    const projectConciergeApp = resources.conciergeApps.find(app => app.projectId === projectId);
    if (projectConciergeApp?.id) {
      resources.setSelectedConciergeAppId(projectConciergeApp.id);
    } else {
      resources.setSelectedConciergeAppId('');
    }
    const currentCustomer = customers.customers.find(item => item.id === customers.selectedCustomerId);
    if (currentCustomer?.projectId !== projectId) {
      customers.setSelectedCustomerId('');
      conversations.setSelectedConversationId('');
      tickets.setSelectedTicketId('');
    }
  };

  const navigateToConciergeApp = (conciergeAppId: string) => {
    if (!conciergeAppId) return;
    resources.setSelectedConciergeAppId(conciergeAppId);
    const app = resources.conciergeApps.find(item => item.id === conciergeAppId);
    if (app?.projectId) {
      resources.setSelectedProjectId(app.projectId);
    }
    const currentConversation = conversations.conversations.find(item => item.id === conversations.selectedConversationId);
    if (currentConversation?.conciergeAppId !== conciergeAppId) {
      conversations.setSelectedConversationId('');
      tickets.setSelectedTicketId('');
    }
  };

  const navigateToCustomer = (customerId: string) => {
    if (!customerId) return;
    customers.setSelectedCustomerId(customerId);
    const customer = customers.customers.find(item => item.id === customerId);
    if (customer?.projectId) {
      navigateToProject(customer.projectId);
    }
    const currentConversation = conversations.conversations.find(item => item.id === conversations.selectedConversationId);
    if (currentConversation?.customerId !== customerId) {
      conversations.setSelectedConversationId('');
      tickets.setSelectedTicketId('');
    }
  };

  const navigateToConversation = (conversationId: string) => {
    if (!conversationId) return;
    conversations.setSelectedConversationId(conversationId);
    const conversation = conversations.conversations.find(item => item.id === conversationId);
    if (conversation?.customerId) {
      customers.setSelectedCustomerId(conversation.customerId);
    }
    if (conversation?.conciergeAppId) {
      navigateToConciergeApp(conversation.conciergeAppId);
    }
    const currentTicket = tickets.tickets.find(item => item.id === tickets.selectedTicketId);
    if (currentTicket?.conversationId !== conversationId) {
      tickets.setSelectedTicketId('');
    }
  };

  const navigateToTicket = (ticketId: string) => {
    if (!ticketId) return;
    tickets.setSelectedTicketId(ticketId);
    const ticket = tickets.tickets.find(item => item.id === ticketId);
    if (ticket?.projectId) navigateToProject(ticket.projectId);
    if (ticket?.customerId) customers.setSelectedCustomerId(ticket.customerId);
    if (ticket?.conversationId) conversations.setSelectedConversationId(ticket.conversationId);
    if (ticket?.conciergeAppId) navigateToConciergeApp(ticket.conciergeAppId);
  };

  const selectProjectWorkflow = (workflowId: string) => {
    projectWorkflows.setSelectedWorkflowId(workflowId);
    const workflow = projectWorkflows.ticketWorkflows.find(item => item.id === workflowId);
    if (!workflow) return;
    if (workflow.projectId) navigateToProject(workflow.projectId);
    if (workflow.conversationId) navigateToConversation(workflow.conversationId);
    if (workflow.ticketId) navigateToTicket(workflow.ticketId);
  };

  const selectCustomerWorkflow = (workflowId: string) => {
    customerWorkflows.setSelectedWorkflowId(workflowId);
    const workflow = customerWorkflows.ticketWorkflows.find(item => item.id === workflowId);
    if (!workflow) return;
    if (workflow.projectId) navigateToProject(workflow.projectId);
    if (workflow.conversationId) navigateToConversation(workflow.conversationId);
    if (workflow.ticketId) navigateToTicket(workflow.ticketId);
  };

  // 15. Auto-scroll effect
  useEffect(() => {
    chat.scrollRef.current?.scrollTo({
      top: chat.scrollRef.current.scrollHeight,
      behavior: 'smooth',
    });
  }, [chat.displayedTexts, chat.messages, chat.scrollRef]);

  // ─── Context values ───────────────────────────────────────────────

  const statusValue: StatusContextValue = { busyAction, feedback, runAction, setFeedback };

  const authValue: AuthContextValue = {
    currentUser: auth.currentUser,
    health: auth.health,
    token: auth.token,
    loginForm: auth.loginForm,
    registerForm: auth.registerForm,
    setLoginForm: auth.setLoginForm,
    setRegisterForm: auth.setRegisterForm,
    handleLogin: auth.handleLogin,
    handleLogout: auth.handleLogout,
    handleRegister: auth.handleRegister,
  };

  const teamValue: TeamContextValue = {
    currentTeam: team.currentTeam,
    currentTeamId: team.currentTeamId,
    refreshTeams: team.refreshTeams,
    teamDescription: team.teamDescription,
    teamName: team.teamName,
    teams: team.teams,
    setCurrentTeamId: team.setCurrentTeamId,
    setTeamDescription: team.setTeamDescription,
    setTeamName: team.setTeamName,
    handleCreateTeam: team.handleCreateTeam,
  };

  const resourcesValue: ResourcesContextValue = {
    conciergeApps: resources.conciergeApps,
    conciergeUpdateForm: resources.conciergeUpdateForm,
    createConciergeForm: resources.createConciergeForm,
    createProjectForm: resources.createProjectForm,
    isResourcesLoading: resources.isResourcesLoading,
    resourcesError: resources.resourcesError,
    refreshWorkspaceData: resources.refreshWorkspaceData,
    projectUpdateForm: resources.projectUpdateForm,
    projects: resources.projects,
    selectedConciergeApp: resources.selectedConciergeApp,
    selectedConciergeAppId: resources.selectedConciergeAppId,
    selectedProject: resources.selectedProject,
    selectedProjectId: resources.selectedProjectId,
    selectedProjectParticipants: resources.selectedProjectParticipants,
    teamMembers: resources.teamMembers,
    setConciergeUpdateForm: resources.setConciergeUpdateForm,
    setCreateConciergeForm: resources.setCreateConciergeForm,
    setCreateProjectForm: resources.setCreateProjectForm,
    setProjectUpdateForm: resources.setProjectUpdateForm,
    setSelectedConciergeAppId: resources.setSelectedConciergeAppId,
    setSelectedProjectId: resources.setSelectedProjectId,
    handleCreateConciergeApp: resources.handleCreateConciergeApp,
    handleCreateProject: resources.handleCreateProject,
    handleSaveConciergeApp: resources.handleSaveConciergeApp,
    handleSaveProject: resources.handleSaveProject,
  };

  const customersValue: CustomersContextValue = {
    createCustomerForm: customers.createCustomerForm,
    customerUpdateForm: customers.customerUpdateForm,
    customers: customers.customers,
    customersError: customers.customersError,
    isCustomersLoading: customers.isCustomersLoading,
    refreshCustomers: customers.refreshCustomers,
    selectedCustomer: customers.selectedCustomer,
    selectedCustomerId: customers.selectedCustomerId,
    setCreateCustomerForm: customers.setCreateCustomerForm,
    setCustomerUpdateForm: customers.setCustomerUpdateForm,
    setSelectedCustomerId: customers.setSelectedCustomerId,
    handleCreateCustomer: customers.handleCreateCustomer,
    handleSaveCustomer: customers.handleSaveCustomer,
  };

  const conversationsValue: ConversationsContextValue = {
    autoRunConversationWorkflow: conversations.autoRunConversationWorkflow,
    conversations: conversations.conversations,
    conversationDetail: conversations.conversationDetail,
    conversationDetailError: conversations.conversationDetailError,
    conversationsError: conversations.conversationsError,
    createConversationForm: conversations.createConversationForm,
    filteredConversations: conversations.filteredConversations,
    isConversationDetailLoading: conversations.isConversationDetailLoading,
    isConversationsLoading: conversations.isConversationsLoading,
    selectedConversation: conversations.selectedConversation,
    selectedConversationId: conversations.selectedConversationId,
    setAutoRunConversationWorkflow: conversations.setAutoRunConversationWorkflow,
    setCreateConversationForm: conversations.setCreateConversationForm,
    setSelectedConversationId: conversations.setSelectedConversationId,
    handleCreateConversation: conversations.handleCreateConversation,
    refreshConversations: conversations.refreshConversations,
  };

  const ticketsValue: TicketsContextValue = {
    autoRunTicketWorkflow: tickets.autoRunTicketWorkflow,
    createTicketForm: tickets.createTicketForm,
    filteredTickets: tickets.filteredTickets,
    isTicketDetailLoading: tickets.isTicketDetailLoading,
    isTicketsLoading: tickets.isTicketsLoading,
    ticketsError: tickets.ticketsError,
    relatedTickets: tickets.relatedTickets,
    refreshTickets: tickets.refreshTickets,
    tickets: tickets.tickets,
    selectedTicket: tickets.selectedTicket,
    selectedTicketId: tickets.selectedTicketId,
    setAutoRunTicketWorkflow: tickets.setAutoRunTicketWorkflow,
    ticketCommentDraft: tickets.ticketCommentDraft,
    ticketDetail: tickets.ticketDetail,
    ticketDetailError: tickets.ticketDetailError,
    ticketUpdateDrafts: tickets.ticketUpdateDrafts,
    setCreateTicketForm: tickets.setCreateTicketForm,
    setSelectedTicketId: tickets.setSelectedTicketId,
    setTicketCommentDraft: tickets.setTicketCommentDraft,
    setTicketUpdateDrafts: tickets.setTicketUpdateDrafts,
    handleAddComment: tickets.handleAddComment,
    handleCreateTicket: tickets.handleCreateTicket,
    handleSaveTicket: tickets.handleSaveTicket,
  };

  const teamSettingsValue: TeamSettingsContextValue = {
    aiMemberForm: teamSettings.aiMemberForm,
    aiTemplateEditorForm: teamSettings.aiTemplateEditorForm,
    aiTemplateLibrary: teamSettings.aiTemplateLibrary,
    aiTemplateOptions: teamSettings.aiTemplateOptions,
    humanMemberForm: teamSettings.humanMemberForm,
    invitationForm: teamSettings.invitationForm,
    myAuditLogs: teamSettings.myAuditLogs,
    myInvitations: teamSettings.myInvitations,
    teamAuditLogs: teamSettings.teamAuditLogs,
    teamInvitations: teamSettings.teamInvitations,
    teamSettingsForm: teamSettings.teamSettingsForm,
    userSessions: teamSettings.userSessions,
    setAiMemberForm: teamSettings.setAiMemberForm,
    setAiTemplateEditorForm: teamSettings.setAiTemplateEditorForm,
    setHumanMemberForm: teamSettings.setHumanMemberForm,
    setInvitationForm: teamSettings.setInvitationForm,
    setTeamSettingsForm: teamSettings.setTeamSettingsForm,
    handleApplyAiTemplate: teamSettings.handleApplyAiTemplate,
    handleAcceptInvitation: teamSettings.handleAcceptInvitation,
    handleCreateAiMember: teamSettings.handleCreateAiMember,
    handleCreateAiTemplateTemplate: teamSettings.handleCreateAiTemplateTemplate,
    handleCreateHumanMember: teamSettings.handleCreateHumanMember,
    handleCreateInvitation: teamSettings.handleCreateInvitation,
    handleDuplicateAiTemplate: teamSettings.handleDuplicateAiTemplate,
    handleEditAiTemplate: teamSettings.handleEditAiTemplate,
    handleLogoutAll: teamSettings.handleLogoutAll,
    handleRemoveMember: teamSettings.handleRemoveMember,
    handleRevokeInvitation: teamSettings.handleRevokeInvitation,
    handleRevokeSession: teamSettings.handleRevokeSession,
    handleSaveTeamSettings: teamSettings.handleSaveTeamSettings,
    handleStartNewAiTemplate: teamSettings.handleStartNewAiTemplate,
    handleToggleAiTemplate: teamSettings.handleToggleAiTemplate,
    handleUpdateMemberRole: teamSettings.handleUpdateMemberRole,
    handleUpdateAiTemplateTemplate: teamSettings.handleUpdateAiTemplateTemplate,
    selectedAiTemplateId: teamSettings.selectedAiTemplateId,
  };

  const integrationsValue: IntegrationsContextValue = {
    integrationConnections: integrations.integrationConnections,
    integrationFiles: integrations.integrationFiles,
    integrationFolderPath: integrations.integrationFolderPath,
    integrationForm: integrations.integrationForm,
    integrationPreviewCount: integrations.integrationPreviewCount,
    integrationPreviewCustomers: integrations.integrationPreviewCustomers,
    integrationPreviewProjects: integrations.integrationPreviewProjects,
    integrationPreviewTasks: integrations.integrationPreviewTasks,
    integrationPreviewTickets: integrations.integrationPreviewTickets,
    selectedIntegration: integrations.selectedIntegration,
    selectedIntegrationHealth: integrations.selectedIntegrationHealth,
    selectedIntegrationId: integrations.selectedIntegrationId,
    setIntegrationFolderPath: integrations.setIntegrationFolderPath,
    setIntegrationForm: integrations.setIntegrationForm,
    handleCreateIntegration: integrations.handleCreateIntegration,
    handleImportPreviewCustomer: integrations.handleImportPreviewCustomer,
    handleImportPreviewProject: integrations.handleImportPreviewProject,
    handleImportPreviewTicket: integrations.handleImportPreviewTicket,
    handleRefreshIntegrationPreview: integrations.handleRefreshIntegrationPreview,
    handleSelectIntegrationId: integrations.handleSelectIntegrationId,
    handleValidateIntegration: integrations.handleValidateIntegration,
  };

  const workflowsValue: WorkflowsContextValue = {
    project: {
      workflowTemplates: projectWorkflows.currentScopeWorkflowTemplates,
      workflowRuns: projectWorkflows.ticketWorkflows,
      selectedWorkflow: projectWorkflows.selectedWorkflow,
      selectedWorkflowId: projectWorkflows.selectedWorkflowId,
      workflowForm: projectWorkflows.workflowForm,
      setSelectedWorkflowId: projectWorkflows.setSelectedWorkflowId,
      setWorkflowForm: projectWorkflows.setWorkflowForm,
      handleRunWorkflow: projectWorkflows.handleRunWorkflow,
    },
    customer: {
      workflowTemplates: customerWorkflows.currentScopeWorkflowTemplates,
      workflowRuns: customerWorkflows.ticketWorkflows,
      selectedWorkflow: customerWorkflows.selectedWorkflow,
      selectedWorkflowId: customerWorkflows.selectedWorkflowId,
      workflowForm: customerWorkflows.workflowForm,
      setSelectedWorkflowId: customerWorkflows.setSelectedWorkflowId,
      setWorkflowForm: customerWorkflows.setWorkflowForm,
      handleRunWorkflow: customerWorkflows.handleRunWorkflow,
      workflowScope: customerWorkflowScope,
      workflowScopeLabel: customerWorkflowScopeLabel,
    },
  };

  const chatValue: ChatContextValue = {
    chatError: chat.chatError,
    chatInput: chat.chatInput,
    displayedTexts: chat.displayedTexts,
    isStreaming: chat.isStreaming,
    messages: chat.messages,
    scrollRef: chat.scrollRef,
    setChatInput: chat.setChatInput,
    handleSendMessage: chat.handleSendMessage,
    handleStop: chat.handleStop,
  };

  const navigationValue: NavigationContextValue = {
    navigateToProject,
    navigateToConciergeApp,
    navigateToCustomer,
    navigateToConversation,
    navigateToTicket,
    selectProjectWorkflow,
    selectCustomerWorkflow,
  };

  const derivedValue: DerivedContextValue = {
    projectsLeadCountByMemberId,
    conciergeCountByMemberId,
    importedCustomerExternalIds,
    importedProjectExternalIds,
    importedTicketExternalIds,
    integrationAuditLogs,
    mappedCustomers,
    mappedProjects,
    mappedTickets,
  };

  return (
    <StatusCtx.Provider value={statusValue}>
      <AuthCtx.Provider value={authValue}>
        <TeamCtx.Provider value={teamValue}>
          <ResourcesCtx.Provider value={resourcesValue}>
            <CustomersCtx.Provider value={customersValue}>
              <ConversationsCtx.Provider value={conversationsValue}>
                <TicketsCtx.Provider value={ticketsValue}>
                  <TeamSettingsCtx.Provider value={teamSettingsValue}>
                    <IntegrationsCtx.Provider value={integrationsValue}>
                      <WorkflowsCtx.Provider value={workflowsValue}>
                        <ChatCtx.Provider value={chatValue}>
                          <NavigationCtx.Provider value={navigationValue}>
                            <DerivedCtx.Provider value={derivedValue}>
                              {children}
                            </DerivedCtx.Provider>
                          </NavigationCtx.Provider>
                        </ChatCtx.Provider>
                      </WorkflowsCtx.Provider>
                    </IntegrationsCtx.Provider>
                  </TeamSettingsCtx.Provider>
                </TicketsCtx.Provider>
              </ConversationsCtx.Provider>
            </CustomersCtx.Provider>
          </ResourcesCtx.Provider>
        </TeamCtx.Provider>
      </AuthCtx.Provider>
    </StatusCtx.Provider>
  );
}

// ─── Consumer hooks ─────────────────────────────────────────────────

export function useStatusContext(): StatusContextValue {
  const ctx = useContext(StatusCtx);
  if (!ctx) throw new Error('useStatusContext must be used within WorkspaceProvider');
  return ctx;
}

export function useAuthContext(): AuthContextValue {
  const ctx = useContext(AuthCtx);
  if (!ctx) throw new Error('useAuthContext must be used within WorkspaceProvider');
  return ctx;
}

export function useTeamContext(): TeamContextValue {
  const ctx = useContext(TeamCtx);
  if (!ctx) throw new Error('useTeamContext must be used within WorkspaceProvider');
  return ctx;
}

export function useResourcesContext(): ResourcesContextValue {
  const ctx = useContext(ResourcesCtx);
  if (!ctx) throw new Error('useResourcesContext must be used within WorkspaceProvider');
  return ctx;
}

export function useCustomersContext(): CustomersContextValue {
  const ctx = useContext(CustomersCtx);
  if (!ctx) throw new Error('useCustomersContext must be used within WorkspaceProvider');
  return ctx;
}

export function useConversationsContext(): ConversationsContextValue {
  const ctx = useContext(ConversationsCtx);
  if (!ctx) throw new Error('useConversationsContext must be used within WorkspaceProvider');
  return ctx;
}

export function useTicketsContext(): TicketsContextValue {
  const ctx = useContext(TicketsCtx);
  if (!ctx) throw new Error('useTicketsContext must be used within WorkspaceProvider');
  return ctx;
}

export function useTeamSettingsContext(): TeamSettingsContextValue {
  const ctx = useContext(TeamSettingsCtx);
  if (!ctx) throw new Error('useTeamSettingsContext must be used within WorkspaceProvider');
  return ctx;
}

export function useIntegrationsContext(): IntegrationsContextValue {
  const ctx = useContext(IntegrationsCtx);
  if (!ctx) throw new Error('useIntegrationsContext must be used within WorkspaceProvider');
  return ctx;
}

export function useWorkflowsContext(): WorkflowsContextValue {
  const ctx = useContext(WorkflowsCtx);
  if (!ctx) throw new Error('useWorkflowsContext must be used within WorkspaceProvider');
  return ctx;
}

export function useChatContext(): ChatContextValue {
  const ctx = useContext(ChatCtx);
  if (!ctx) throw new Error('useChatContext must be used within WorkspaceProvider');
  return ctx;
}

export function useNavigationContext(): NavigationContextValue {
  const ctx = useContext(NavigationCtx);
  if (!ctx) throw new Error('useNavigationContext must be used within WorkspaceProvider');
  return ctx;
}

export function useDerivedContext(): DerivedContextValue {
  const ctx = useContext(DerivedCtx);
  if (!ctx) throw new Error('useDerivedContext must be used within WorkspaceProvider');
  return ctx;
}
