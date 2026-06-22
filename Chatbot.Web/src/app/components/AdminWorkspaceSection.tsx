import { useAuthContext, useStatusContext, useTeamContext, useResourcesContext, useTeamSettingsContext, useIntegrationsContext, useDerivedContext, useNavigationContext } from '../workspaceContexts';
import { IntegrationPanel } from './IntegrationPanel';
import { TeamManagementPanel } from './TeamManagementPanel';
import { TeamSettingsPanel } from './TeamSettingsPanel';

export function AdminWorkspaceSection() {
  const { busyAction } = useStatusContext();
  const { currentUser } = useAuthContext();
  const { currentTeam, currentTeamId, teams } = useTeamContext();
  const { selectedProjectId: currentProjectId, selectedProject: currentProject, teamMembers } = useResourcesContext();
  const {
    aiMemberForm,
    aiTemplateEditorForm,
    aiTemplateLibrary,
    aiTemplateOptions,
    humanMemberForm,
    invitationForm,
    myAuditLogs,
    myInvitations,
    teamAuditLogs,
    teamInvitations,
    teamSettingsForm,
    userSessions,
    setAiMemberForm,
    setAiTemplateEditorForm,
    setHumanMemberForm,
    setInvitationForm,
    setTeamSettingsForm,
    handleApplyAiTemplate,
    handleAcceptInvitation,
    handleCreateAiMember,
    handleCreateAiTemplateTemplate,
    handleCreateHumanMember,
    handleCreateInvitation,
    handleDuplicateAiTemplate,
    handleEditAiTemplate,
    handleLogoutAll,
    handleRemoveMember,
    handleRevokeInvitation,
    handleRevokeSession,
    handleSaveTeamSettings,
    handleStartNewAiTemplate,
    handleToggleAiTemplate,
    handleUpdateMemberRole,
    handleUpdateAiTemplateTemplate,
    selectedAiTemplateId,
  } = useTeamSettingsContext();
  const {
    integrationConnections,
    integrationFiles,
    integrationFolderPath,
    integrationForm,
    integrationPreviewCount,
    integrationPreviewCustomers,
    integrationPreviewProjects,
    integrationPreviewTasks,
    integrationPreviewTickets,
    selectedIntegration,
    selectedIntegrationHealth,
    selectedIntegrationId,
    setIntegrationFolderPath,
    setIntegrationForm,
    handleCreateIntegration,
    handleImportPreviewCustomer,
    handleImportPreviewProject,
    handleImportPreviewTicket,
    handleRefreshIntegrationPreview,
    handleSelectIntegrationId,
    handleValidateIntegration,
  } = useIntegrationsContext();
  const {
    projectsLeadCountByMemberId,
    conciergeCountByMemberId,
    importedCustomerExternalIds,
    importedProjectExternalIds,
    importedTicketExternalIds,
    integrationAuditLogs,
    mappedCustomers,
    mappedProjects,
    mappedTickets,
  } = useDerivedContext();
  const { navigateToCustomer, navigateToProject, navigateToTicket } = useNavigationContext();

  return (
    <>
      <TeamManagementPanel
        aiMemberForm={aiMemberForm}
        aiTemplateEditorForm={aiTemplateEditorForm}
        aiTemplateLibrary={aiTemplateLibrary}
        aiTemplateOptions={aiTemplateOptions}
        busyAction={busyAction}
        canManage={Boolean(currentTeamId)}
        humanMemberForm={humanMemberForm}
        invitationForm={invitationForm}
        onAiMemberFormChange={setAiMemberForm}
        onAiTemplateEditorFormChange={setAiTemplateEditorForm}
        onApplyAiTemplate={handleApplyAiTemplate}
        onCreateAiMember={handleCreateAiMember}
        onCreateAiTemplateTemplate={handleCreateAiTemplateTemplate}
        onCreateHumanMember={handleCreateHumanMember}
        onCreateInvitation={handleCreateInvitation}
        onDuplicateAiTemplate={handleDuplicateAiTemplate}
        onEditAiTemplate={handleEditAiTemplate}
        onHumanMemberFormChange={setHumanMemberForm}
        onInvitationFormChange={setInvitationForm}
        onStartNewAiTemplate={handleStartNewAiTemplate}
        onToggleAiTemplate={handleToggleAiTemplate}
        onUpdateAiTemplateTemplate={handleUpdateAiTemplateTemplate}
        selectedAiTemplateId={selectedAiTemplateId}
      />

      <TeamSettingsPanel
        busyAction={busyAction}
        createdTeam={currentTeam}
        currentUser={currentUser}
        myAuditLogs={myAuditLogs}
        myInvitations={myInvitations}
        myTeamsCount={teams.length}
        onAcceptInvitation={handleAcceptInvitation}
        onLogoutAll={handleLogoutAll}
        onRemoveMember={handleRemoveMember}
        onRevokeInvitation={handleRevokeInvitation}
        onRevokeSession={handleRevokeSession}
        onSaveTeamSettings={handleSaveTeamSettings}
        onTeamSettingsFormChange={setTeamSettingsForm}
        onUpdateMemberRole={handleUpdateMemberRole}
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
        canManageIntegrations={Boolean(currentTeamId)}
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
        onNavigateToCustomer={navigateToCustomer}
        onNavigateToProject={navigateToProject}
        onNavigateToTicket={navigateToTicket}
        onCreateIntegration={handleCreateIntegration}
        onImportPreviewCustomer={handleImportPreviewCustomer}
        onImportPreviewProject={handleImportPreviewProject}
        onImportPreviewTicket={handleImportPreviewTicket}
        onIntegrationFolderPathChange={setIntegrationFolderPath}
        onIntegrationFormChange={setIntegrationForm}
        onRefreshIntegrationPreview={handleRefreshIntegrationPreview}
        onRetryLatestIntegrationIssue={() => {
          if (
            selectedIntegrationHealth
            && (!selectedIntegrationHealth.isAuthenticated || !selectedIntegrationHealth.isReachable)
          ) {
            void handleValidateIntegration();
            return;
          }
          void handleRefreshIntegrationPreview();
        }}
        onSelectIntegrationId={handleSelectIntegrationId}
        onValidateIntegration={handleValidateIntegration}
        selectedIntegration={selectedIntegration}
        selectedIntegrationHealth={selectedIntegrationHealth}
        selectedIntegrationId={selectedIntegrationId}
      />
    </>
  );
}
