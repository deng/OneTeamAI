import type { Dispatch, SetStateAction } from 'react';
import type {
  CreateAiMemberRequest,
  CreateHumanMemberRequest,
  CreateIntegrationConnectionRequest,
  CreateInvitationRequest,
  FileKnowledgeItemResponse,
  IntegrationConnectionHealthResponse,
  IntegrationConnectionResponse,
  IntegrationPreviewItemResponse,
  InvitationResponse,
  MemberRole,
  MemberResponse,
  ProjectResponse,
  CustomerResponse,
  TeamSummaryResponse,
  TicketResponse,
  UpdateTeamRequest,
  UserResponse,
} from '../../generated/api';
import type {
  AiMemberTemplateEditorForm,
  AiMemberTemplateItem,
  AuditLogItem,
  UserSessionItem,
} from '../types';
import { IntegrationPanel } from './IntegrationPanel';
import { TeamManagementPanel } from './TeamManagementPanel';
import { TeamSettingsPanel } from './TeamSettingsPanel';

type AdminWorkspaceSectionProps = {
  busyAction: string | null;
  createdTeam: TeamSummaryResponse | null;
  currentUser: UserResponse | null;
  myTeamsCount: number;
  teamSettingsForm: UpdateTeamRequest;
  userSessions: UserSessionItem[];
  teamMembers: MemberResponse[];
  projectsLeadCountByMemberId: Record<string, number>;
  conciergeCountByMemberId: Record<string, number>;
  teamInvitations: InvitationResponse[];
  myInvitations: InvitationResponse[];
  teamAuditLogs: AuditLogItem[];
  myAuditLogs: AuditLogItem[];
  integrationForm: CreateIntegrationConnectionRequest;
  integrationFolderPath: string;
  canManageIntegrations: boolean;
  integrationConnections: IntegrationConnectionResponse[];
  selectedIntegrationId: string;
  selectedIntegration: IntegrationConnectionResponse | null;
  integrationPreviewCount: number;
  selectedIntegrationHealth: IntegrationConnectionHealthResponse | null;
  integrationFiles: FileKnowledgeItemResponse[];
  integrationPreviewCustomers: IntegrationPreviewItemResponse[];
  integrationPreviewProjects: IntegrationPreviewItemResponse[];
  integrationPreviewTickets: IntegrationPreviewItemResponse[];
  integrationPreviewTasks: IntegrationPreviewItemResponse[];
  integrationAuditLogs: AuditLogItem[];
  importedCustomerExternalIds: string[];
  importedProjectExternalIds: string[];
  importedTicketExternalIds: string[];
  mappedCustomers: CustomerResponse[];
  mappedProjects: ProjectResponse[];
  mappedTickets: TicketResponse[];
  currentProjectId: string;
  currentProject: ProjectResponse | null;
  humanMemberForm: CreateHumanMemberRequest;
  aiMemberForm: CreateAiMemberRequest;
  aiTemplateEditorForm: AiMemberTemplateEditorForm;
  aiTemplateLibrary: ReadonlyArray<AiMemberTemplateItem>;
  invitationForm: CreateInvitationRequest;
  aiTemplateOptions: ReadonlyArray<AiMemberTemplateItem>;
  selectedAiTemplateId: string;
  onTeamSettingsFormChange: Dispatch<SetStateAction<UpdateTeamRequest>>;
  onHumanMemberFormChange: Dispatch<SetStateAction<CreateHumanMemberRequest>>;
  onAiMemberFormChange: Dispatch<SetStateAction<CreateAiMemberRequest>>;
  onAiTemplateEditorFormChange: Dispatch<SetStateAction<AiMemberTemplateEditorForm>>;
  onInvitationFormChange: Dispatch<SetStateAction<CreateInvitationRequest>>;
  onIntegrationFolderPathChange: (value: string) => void;
  onNavigateToCustomer: (customerId: string) => void;
  onNavigateToProject: (projectId: string) => void;
  onNavigateToTicket: (ticketId: string) => void;
  onSaveTeamSettings: () => void;
  onRevokeSession: (sessionId: string, isCurrent: boolean) => void;
  onLogoutAll: () => void;
  onUpdateMemberRole: (member: MemberResponse, role: MemberRole) => void;
  onRemoveMember: (member: MemberResponse) => void;
  onRevokeInvitation: (invitation: InvitationResponse) => void;
  onAcceptInvitation: (invitation: InvitationResponse) => void;
  onApplyAiTemplate: (templateKey: string) => void;
  onStartNewAiTemplate: () => void;
  onDuplicateAiTemplate: (templateId: string) => void;
  onEditAiTemplate: (templateId: string) => void;
  onCreateHumanMember: () => void;
  onCreateAiMember: () => void;
  onCreateAiTemplateTemplate: () => void;
  onUpdateAiTemplateTemplate: () => void;
  onToggleAiTemplate: (template: AiMemberTemplateItem) => void;
  onCreateInvitation: () => void;
  onIntegrationFormChange: Dispatch<SetStateAction<CreateIntegrationConnectionRequest>>;
  onCreateIntegration: () => void;
  onImportPreviewCustomer: (externalRecordId: string, forceUpdate?: boolean) => void;
  onImportPreviewProject: (externalRecordId: string, forceUpdate?: boolean) => void;
  onImportPreviewTicket: (externalRecordId: string, forceUpdate?: boolean) => void;
  onRefreshIntegrationPreview: () => void;
  onRetryLatestIntegrationIssue: () => void;
  onSelectIntegrationId: (integrationId: string) => void;
  onValidateIntegration: () => void;
};

export function AdminWorkspaceSection({
  busyAction,
  createdTeam,
  currentUser,
  myTeamsCount,
  teamSettingsForm,
  userSessions,
  teamMembers,
  projectsLeadCountByMemberId,
  conciergeCountByMemberId,
  teamInvitations,
  myInvitations,
  teamAuditLogs,
  myAuditLogs,
  integrationForm,
  integrationFolderPath,
  canManageIntegrations,
  integrationConnections,
  selectedIntegrationId,
  selectedIntegration,
  integrationPreviewCount,
  selectedIntegrationHealth,
  integrationFiles,
  integrationPreviewCustomers,
  integrationPreviewProjects,
  integrationPreviewTickets,
  integrationPreviewTasks,
  integrationAuditLogs,
  importedCustomerExternalIds,
  importedProjectExternalIds,
  importedTicketExternalIds,
  mappedCustomers,
  mappedProjects,
  mappedTickets,
  currentProjectId,
  currentProject,
  humanMemberForm,
  aiMemberForm,
  aiTemplateEditorForm,
  aiTemplateLibrary,
  invitationForm,
  aiTemplateOptions,
  selectedAiTemplateId,
  onTeamSettingsFormChange,
  onHumanMemberFormChange,
  onAiMemberFormChange,
  onAiTemplateEditorFormChange,
  onInvitationFormChange,
  onIntegrationFolderPathChange,
  onNavigateToCustomer,
  onNavigateToProject,
  onNavigateToTicket,
  onSaveTeamSettings,
  onRevokeSession,
  onLogoutAll,
  onUpdateMemberRole,
  onRemoveMember,
  onRevokeInvitation,
  onAcceptInvitation,
  onApplyAiTemplate,
  onStartNewAiTemplate,
  onDuplicateAiTemplate,
  onEditAiTemplate,
  onCreateHumanMember,
  onCreateAiMember,
  onCreateAiTemplateTemplate,
  onUpdateAiTemplateTemplate,
  onToggleAiTemplate,
  onCreateInvitation,
  onIntegrationFormChange,
  onCreateIntegration,
  onImportPreviewCustomer,
  onImportPreviewProject,
  onImportPreviewTicket,
  onRefreshIntegrationPreview,
  onRetryLatestIntegrationIssue,
  onSelectIntegrationId,
  onValidateIntegration,
}: AdminWorkspaceSectionProps) {
  return (
    <>
      <TeamManagementPanel
        aiMemberForm={aiMemberForm}
        aiTemplateEditorForm={aiTemplateEditorForm}
        aiTemplateLibrary={aiTemplateLibrary}
        aiTemplateOptions={aiTemplateOptions}
        busyAction={busyAction}
        canManage={canManageIntegrations}
        humanMemberForm={humanMemberForm}
        invitationForm={invitationForm}
        onAiMemberFormChange={onAiMemberFormChange}
        onAiTemplateEditorFormChange={onAiTemplateEditorFormChange}
        onApplyAiTemplate={onApplyAiTemplate}
        onCreateAiMember={onCreateAiMember}
        onCreateAiTemplateTemplate={onCreateAiTemplateTemplate}
        onCreateHumanMember={onCreateHumanMember}
        onCreateInvitation={onCreateInvitation}
        onDuplicateAiTemplate={onDuplicateAiTemplate}
        onEditAiTemplate={onEditAiTemplate}
        onHumanMemberFormChange={onHumanMemberFormChange}
        onInvitationFormChange={onInvitationFormChange}
        onStartNewAiTemplate={onStartNewAiTemplate}
        onToggleAiTemplate={onToggleAiTemplate}
        onUpdateAiTemplateTemplate={onUpdateAiTemplateTemplate}
        selectedAiTemplateId={selectedAiTemplateId}
      />

      <TeamSettingsPanel
        busyAction={busyAction}
        createdTeam={createdTeam}
        currentUser={currentUser}
        myAuditLogs={myAuditLogs}
        myInvitations={myInvitations}
        myTeamsCount={myTeamsCount}
        onAcceptInvitation={onAcceptInvitation}
        onLogoutAll={onLogoutAll}
        onRemoveMember={onRemoveMember}
        onRevokeInvitation={onRevokeInvitation}
        onRevokeSession={onRevokeSession}
        onSaveTeamSettings={onSaveTeamSettings}
        onTeamSettingsFormChange={onTeamSettingsFormChange}
        onUpdateMemberRole={onUpdateMemberRole}
        projectsLeadCountByMemberId={projectsLeadCountByMemberId}
        conciergeCountByMemberId={conciergeCountByMemberId}
        teamAuditLogs={teamAuditLogs}
        teamInvitations={teamInvitations}
        teamMembers={teamMembers}
        teamSettingsForm={teamSettingsForm}
        userSessions={userSessions}
      />

      <IntegrationPanel
        busyAction={busyAction}
        canManageIntegrations={canManageIntegrations}
        integrationConnections={integrationConnections}
        integrationFiles={integrationFiles}
        integrationForm={integrationForm}
        integrationFolderPath={integrationFolderPath}
        integrationPreviewCount={integrationPreviewCount}
        integrationPreviewCustomers={integrationPreviewCustomers}
        integrationPreviewProjects={integrationPreviewProjects}
        integrationPreviewTasks={integrationPreviewTasks}
        integrationPreviewTickets={integrationPreviewTickets}
        integrationAuditLogs={integrationAuditLogs}
        importedCustomerExternalIds={importedCustomerExternalIds}
        importedProjectExternalIds={importedProjectExternalIds}
        importedTicketExternalIds={importedTicketExternalIds}
        mappedCustomers={mappedCustomers}
        mappedProjects={mappedProjects}
        mappedTickets={mappedTickets}
        currentProjectId={currentProjectId}
        currentProject={currentProject}
        onNavigateToCustomer={onNavigateToCustomer}
        onNavigateToProject={onNavigateToProject}
        onNavigateToTicket={onNavigateToTicket}
        onCreateIntegration={onCreateIntegration}
        onImportPreviewCustomer={onImportPreviewCustomer}
        onImportPreviewProject={onImportPreviewProject}
        onImportPreviewTicket={onImportPreviewTicket}
        onIntegrationFolderPathChange={onIntegrationFolderPathChange}
        onIntegrationFormChange={onIntegrationFormChange}
        onRefreshIntegrationPreview={onRefreshIntegrationPreview}
        onRetryLatestIntegrationIssue={onRetryLatestIntegrationIssue}
        onSelectIntegrationId={onSelectIntegrationId}
        onValidateIntegration={onValidateIntegration}
        selectedIntegration={selectedIntegration}
        selectedIntegrationHealth={selectedIntegrationHealth}
        selectedIntegrationId={selectedIntegrationId}
      />
    </>
  );
}
