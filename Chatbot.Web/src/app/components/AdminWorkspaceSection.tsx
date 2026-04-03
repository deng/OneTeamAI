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
  TeamSummaryResponse,
  UpdateTeamRequest,
  UserResponse,
} from '../../generated/api';
import type { AuditLogItem, UserSessionItem } from '../types';
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
  humanMemberForm: CreateHumanMemberRequest;
  aiMemberForm: CreateAiMemberRequest;
  invitationForm: CreateInvitationRequest;
  aiTemplateOptions: ReadonlyArray<({ key: string; label: string } & CreateAiMemberRequest)>;
  onTeamSettingsFormChange: Dispatch<SetStateAction<UpdateTeamRequest>>;
  onHumanMemberFormChange: Dispatch<SetStateAction<CreateHumanMemberRequest>>;
  onAiMemberFormChange: Dispatch<SetStateAction<CreateAiMemberRequest>>;
  onInvitationFormChange: Dispatch<SetStateAction<CreateInvitationRequest>>;
  onSaveTeamSettings: () => void;
  onRevokeSession: (sessionId: string, isCurrent: boolean) => void;
  onLogoutAll: () => void;
  onUpdateMemberRole: (member: MemberResponse, role: MemberRole) => void;
  onRemoveMember: (member: MemberResponse) => void;
  onRevokeInvitation: (invitation: InvitationResponse) => void;
  onAcceptInvitation: (invitation: InvitationResponse) => void;
  onApplyAiTemplate: (templateKey: string) => void;
  onCreateHumanMember: () => void;
  onCreateAiMember: () => void;
  onCreateInvitation: () => void;
  onIntegrationFormChange: Dispatch<SetStateAction<CreateIntegrationConnectionRequest>>;
  onCreateIntegration: () => void;
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
  humanMemberForm,
  aiMemberForm,
  invitationForm,
  aiTemplateOptions,
  onTeamSettingsFormChange,
  onHumanMemberFormChange,
  onAiMemberFormChange,
  onInvitationFormChange,
  onSaveTeamSettings,
  onRevokeSession,
  onLogoutAll,
  onUpdateMemberRole,
  onRemoveMember,
  onRevokeInvitation,
  onAcceptInvitation,
  onApplyAiTemplate,
  onCreateHumanMember,
  onCreateAiMember,
  onCreateInvitation,
  onIntegrationFormChange,
  onCreateIntegration,
  onSelectIntegrationId,
  onValidateIntegration,
}: AdminWorkspaceSectionProps) {
  return (
    <>
      <TeamManagementPanel
        aiMemberForm={aiMemberForm}
        aiTemplateOptions={aiTemplateOptions}
        busyAction={busyAction}
        canManage={canManageIntegrations}
        humanMemberForm={humanMemberForm}
        invitationForm={invitationForm}
        onAiMemberFormChange={onAiMemberFormChange}
        onApplyAiTemplate={onApplyAiTemplate}
        onCreateAiMember={onCreateAiMember}
        onCreateHumanMember={onCreateHumanMember}
        onCreateInvitation={onCreateInvitation}
        onHumanMemberFormChange={onHumanMemberFormChange}
        onInvitationFormChange={onInvitationFormChange}
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
        integrationPreviewCount={integrationPreviewCount}
        integrationPreviewCustomers={integrationPreviewCustomers}
        integrationPreviewProjects={integrationPreviewProjects}
        integrationPreviewTasks={integrationPreviewTasks}
        integrationPreviewTickets={integrationPreviewTickets}
        onCreateIntegration={onCreateIntegration}
        onIntegrationFormChange={onIntegrationFormChange}
        onSelectIntegrationId={onSelectIntegrationId}
        onValidateIntegration={onValidateIntegration}
        selectedIntegration={selectedIntegration}
        selectedIntegrationHealth={selectedIntegrationHealth}
        selectedIntegrationId={selectedIntegrationId}
      />
    </>
  );
}
