import { useEffect, useMemo } from 'react';
import { ChatPanel } from './components/ChatPanel';
import { AdminWorkspaceSection } from './components/AdminWorkspaceSection';
import { AuthSection } from './components/AuthSection';
import { BusinessWorkspaceSection } from './components/BusinessWorkspaceSection';
import { CollapsibleSection } from './components/CollapsibleSection';
import { TeamSection } from './components/TeamSection';
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

export function AppWorkspace() {
  const { busyAction, feedback, runAction, setFeedback } = useWorkspaceStatus();
  const {
    currentUser,
    health,
    loginForm,
    registerForm,
    setLoginForm,
    setRegisterForm,
    token,
    handleLogin,
    handleLogout,
    handleRegister,
  } = useWorkspaceAuth({
    runAction,
    setFeedback,
  });
  const {
    currentTeam,
    currentTeamId,
    refreshTeams,
    teamDescription,
    teamName,
    teams,
    setCurrentTeamId,
    setTeamDescription,
    setTeamName,
    handleCreateTeam,
  } = useWorkspaceTeam({
    token,
    runAction,
    setFeedback,
  });
  const {
    conciergeApps,
    conciergeUpdateForm,
    createConciergeForm,
    createProjectForm,
    isResourcesLoading,
    resourcesError,
    refreshWorkspaceData,
    projectUpdateForm,
    projects,
    selectedConciergeApp,
    selectedConciergeAppId,
    selectedProject,
    selectedProjectId,
    selectedProjectParticipants,
    teamMembers,
    setConciergeUpdateForm,
    setCreateConciergeForm,
    setCreateProjectForm,
    setProjectUpdateForm,
    setSelectedConciergeAppId,
    setSelectedProjectId,
    handleCreateConciergeApp,
    handleCreateProject,
    handleSaveConciergeApp,
    handleSaveProject,
  } = useWorkspaceResources({
    currentTeamId,
    token,
    runAction,
    setFeedback,
  });
  const projectsLeadCountByMemberId = useMemo(
    () =>
      projects.reduce<Record<string, number>>((accumulator, project) => {
        if (project.leadMemberId) {
          accumulator[project.leadMemberId] = (accumulator[project.leadMemberId] ?? 0) + 1;
        }
        return accumulator;
      }, {}),
    [projects],
  );
  const conciergeCountByMemberId = useMemo(
    () =>
      conciergeApps.reduce<Record<string, number>>((accumulator, app) => {
        if (app.primaryAiMemberId) {
          accumulator[app.primaryAiMemberId] = (accumulator[app.primaryAiMemberId] ?? 0) + 1;
        }
        return accumulator;
      }, {}),
    [conciergeApps],
  );
  const {
    createCustomerForm,
    customerUpdateForm,
    customers,
    customersError,
    isCustomersLoading,
    refreshCustomers,
    selectedCustomer,
    selectedCustomerId,
    setCreateCustomerForm,
    setCustomerUpdateForm,
    setSelectedCustomerId,
    handleCreateCustomer,
    handleSaveCustomer,
  } = useWorkspaceCustomers({
    currentTeamId,
    projects,
    token,
    runAction,
    setFeedback,
  });
  const {
    autoRunConversationWorkflow,
    conversations,
    conversationDetail,
    conversationDetailError,
    conversationsError,
    createConversationForm,
    filteredConversations,
    isConversationDetailLoading,
    isConversationsLoading,
    selectedConversation,
    selectedConversationId,
    setAutoRunConversationWorkflow,
    setCreateConversationForm,
    setSelectedConversationId,
    handleCreateConversation,
    refreshConversations,
  } = useWorkspaceConversations({
    currentTeamId,
    selectedConciergeApp,
    selectedCustomer,
    selectedCustomerId,
    token,
    runAction,
    setFeedback,
  });
  const {
    autoRunTicketWorkflow,
    createTicketForm,
    filteredTickets,
    isTicketDetailLoading,
    isTicketsLoading,
    ticketsError,
    relatedTickets,
    refreshTickets,
    tickets,
    selectedTicket,
    selectedTicketId,
    setAutoRunTicketWorkflow,
    ticketCommentDraft,
    ticketDetail,
    ticketDetailError,
    ticketUpdateDrafts,
    setCreateTicketForm,
    setSelectedTicketId,
    setTicketCommentDraft,
    setTicketUpdateDrafts,
    handleAddComment,
    handleCreateTicket,
    handleSaveTicket,
  } = useWorkspaceTickets({
    currentTeamId,
    selectedConversation,
    selectedConversationId,
    selectedCustomerId,
    token,
    runAction,
    setFeedback,
  });
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
  } = useWorkspaceTeamSettings({
    currentTeam,
    currentTeamId,
    currentUser,
    token,
    teamMembers,
    projectsLeadCountByMemberId,
    conciergeCountByMemberId,
    refreshWorkspaceData,
    refreshTeams,
    runAction,
    setFeedback,
  });
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
  } = useWorkspaceIntegrations({
    currentTeamId,
    refreshWorkspaceData,
    refreshCustomers,
    refreshTickets,
    selectedProjectId,
    selectedCustomerId,
    token,
    runAction,
    setFeedback,
  });
  const importedCustomerExternalIds = useMemo(
    () =>
      customers
        .filter(
          customer =>
            customer.externalSystemType === selectedIntegration?.externalSystemType
            && Boolean(customer.externalId),
        )
        .map(customer => customer.externalId as string),
    [customers, selectedIntegration?.externalSystemType],
  );
  const importedProjectExternalIds = useMemo(
    () =>
      projects
        .filter(
          project =>
            project.externalSystemType === selectedIntegration?.externalSystemType
            && Boolean(project.externalId),
        )
        .map(project => project.externalId as string),
    [projects, selectedIntegration?.externalSystemType],
  );
  const integrationAuditLogs = useMemo(
    () => {
      const logs = teamAuditLogs.filter(log => log.actionType.startsWith('integration.'));
      if (!selectedIntegration) {
        return logs;
      }

      return logs.filter(log => {
        if (log.entityId === selectedIntegration.id) {
          return true;
        }

        return log.summary.includes(selectedIntegration.name ?? '');
      });
    },
    [selectedIntegration, teamAuditLogs],
  );
  const mappedCustomers = useMemo(
    () =>
      customers.filter(
        customer =>
          customer.externalSystemType === selectedIntegration?.externalSystemType
          && Boolean(customer.externalId),
      ),
    [customers, selectedIntegration?.externalSystemType],
  );
  const mappedProjects = useMemo(
    () =>
      projects.filter(
        project =>
          project.externalSystemType === selectedIntegration?.externalSystemType
          && Boolean(project.externalId),
      ),
    [projects, selectedIntegration?.externalSystemType],
  );
  const importedTicketExternalIds = useMemo(
    () =>
      tickets
        .filter(
          ticket =>
            ticket.externalSystemType === selectedIntegration?.externalSystemType
            && Boolean(ticket.externalId),
        )
        .map(ticket => ticket.externalId as string),
    [tickets, selectedIntegration?.externalSystemType],
  );
  const mappedTickets = useMemo(
    () =>
      tickets.filter(
        ticket =>
          ticket.externalSystemType === selectedIntegration?.externalSystemType
          && Boolean(ticket.externalId),
      ),
    [tickets, selectedIntegration?.externalSystemType],
  );
  const {
    currentScopeWorkflowTemplates: projectWorkflowTemplates,
    selectedWorkflow: projectSelectedWorkflow,
    selectedWorkflowId: projectSelectedWorkflowId,
    ticketWorkflows: projectWorkflowRuns,
    workflowForm: projectWorkflowForm,
    setSelectedWorkflowId: setProjectSelectedWorkflowId,
    setWorkflowForm: setProjectWorkflowForm,
    handleRunWorkflow: handleRunProjectWorkflow,
  } = useWorkspaceWorkflows({
    currentTeamId,
    scope: selectedProjectId ? 'project' : null,
    scopeId: selectedProjectId,
    scopeLabel: selectedProject?.name ?? '当前项目',
    teamMembers,
    token,
    runAction,
    setFeedback,
  });
  const customerWorkflowScope = selectedTicketId ? 'ticket' : selectedConversationId ? 'conversation' : null;
  const customerWorkflowScopeId = selectedTicketId || selectedConversationId;
  const customerWorkflowScopeLabel = selectedTicketId
    ? (selectedTicket?.title ?? '当前工单')
    : (selectedConversation?.customerName ?? '当前会话');
  const {
    currentScopeWorkflowTemplates: customerWorkflowTemplates,
    selectedWorkflow: customerSelectedWorkflow,
    selectedWorkflowId: customerSelectedWorkflowId,
    ticketWorkflows: customerWorkflowRuns,
    workflowForm: customerWorkflowForm,
    setSelectedWorkflowId: setCustomerSelectedWorkflowId,
    setWorkflowForm: setCustomerWorkflowForm,
    handleRunWorkflow: handleRunCustomerWorkflow,
  } = useWorkspaceWorkflows({
    currentTeamId,
    scope: customerWorkflowScope,
    scopeId: customerWorkflowScopeId,
    scopeLabel: customerWorkflowScopeLabel,
    teamMembers,
    token,
    runAction,
    setFeedback,
  });
  const {
    chatError,
    chatInput,
    displayedTexts,
    isStreaming,
    messages,
    scrollRef,
    setChatInput,
    handleSendMessage,
    handleStop,
  } = useWorkspaceChat();

  useEffect(() => {
    scrollRef.current?.scrollTo({
      top: scrollRef.current.scrollHeight,
      behavior: 'smooth',
    });
  }, [displayedTexts, messages, scrollRef]);

  function navigateToProject(projectId: string) {
    if (!projectId) {
      return;
    }

    setSelectedProjectId(projectId);

    const projectConciergeApp = conciergeApps.find(app => app.projectId === projectId);
    if (projectConciergeApp?.id) {
      setSelectedConciergeAppId(projectConciergeApp.id);
    } else {
      setSelectedConciergeAppId('');
    }

    const currentCustomer = customers.find(item => item.id === selectedCustomerId);
    if (currentCustomer?.projectId !== projectId) {
      setSelectedCustomerId('');
      setSelectedConversationId('');
      setSelectedTicketId('');
    }
  }

  function navigateToConciergeApp(conciergeAppId: string) {
    if (!conciergeAppId) {
      return;
    }

    setSelectedConciergeAppId(conciergeAppId);
    const app = conciergeApps.find(item => item.id === conciergeAppId);
    if (app?.projectId) {
      setSelectedProjectId(app.projectId);
    }

    const currentConversation = conversations.find(item => item.id === selectedConversationId);
    if (currentConversation?.conciergeAppId !== conciergeAppId) {
      setSelectedConversationId('');
      setSelectedTicketId('');
    }
  }

  function navigateToCustomer(customerId: string) {
    if (!customerId) {
      return;
    }

    setSelectedCustomerId(customerId);
    const customer = customers.find(item => item.id === customerId);
    if (customer?.projectId) {
      navigateToProject(customer.projectId);
    }

    const currentConversation = conversations.find(item => item.id === selectedConversationId);
    if (currentConversation?.customerId !== customerId) {
      setSelectedConversationId('');
      setSelectedTicketId('');
    }
  }

  function navigateToConversation(conversationId: string) {
    if (!conversationId) {
      return;
    }

    setSelectedConversationId(conversationId);
    const conversation = conversations.find(item => item.id === conversationId);
    if (conversation?.customerId) {
      setSelectedCustomerId(conversation.customerId);
    }
    if (conversation?.conciergeAppId) {
      navigateToConciergeApp(conversation.conciergeAppId);
    }

    const currentTicket = tickets.find(item => item.id === selectedTicketId);
    if (currentTicket?.conversationId !== conversationId) {
      setSelectedTicketId('');
    }
  }

  function navigateToTicket(ticketId: string) {
    if (!ticketId) {
      return;
    }

    setSelectedTicketId(ticketId);
    const ticket = tickets.find(item => item.id === ticketId);
    if (ticket?.projectId) {
      navigateToProject(ticket.projectId);
    }
    if (ticket?.customerId) {
      setSelectedCustomerId(ticket.customerId);
    }
    if (ticket?.conversationId) {
      setSelectedConversationId(ticket.conversationId);
    }
    if (ticket?.conciergeAppId) {
      setSelectedConciergeAppId(ticket.conciergeAppId);
    }
  }

  function selectProjectWorkflow(workflowId: string) {
    setProjectSelectedWorkflowId(workflowId);
    const workflow = projectWorkflowRuns.find(item => item.id === workflowId);
    if (!workflow) {
      return;
    }

    if (workflow.projectId) {
      navigateToProject(workflow.projectId);
    }
    if (workflow.conversationId) {
      navigateToConversation(workflow.conversationId);
    }
    if (workflow.ticketId) {
      navigateToTicket(workflow.ticketId);
    }
  }

  function selectCustomerWorkflow(workflowId: string) {
    setCustomerSelectedWorkflowId(workflowId);
    const workflow = customerWorkflowRuns.find(item => item.id === workflowId);
    if (!workflow) {
      return;
    }

    if (workflow.projectId) {
      navigateToProject(workflow.projectId);
    }
    if (workflow.conversationId) {
      navigateToConversation(workflow.conversationId);
    }
    if (workflow.ticketId) {
      navigateToTicket(workflow.ticketId);
    }
  }

  return (
    <div className="shell">
      <header className="hero">
        <p className="eyebrow">AI Virtual Team Workspace</p>
        <h1>重新接回工作台主链</h1>
        <p className="lede">
          当前版本已经恢复登录、团队、项目、坐台程序和聊天主链。后续会继续在现有组件和 hooks
          上，把客户、会话、工单和多智能体协作逐步接回去。
        </p>
      </header>

      <div className="workspace">
        <aside className="panel panel-side">
            <CollapsibleSection title="账号" storageKey="section-auth" defaultExpanded={!currentUser}>
              <AuthSection
            busyAction={busyAction}
            currentUser={currentUser}
            feedback={feedback}
            health={health}
            loginForm={loginForm}
            registerForm={registerForm}
            onLogin={() => {
              void handleLogin();
            }}
            onLogout={() => {
              void handleLogout();
            }}
            onRegister={() => {
              void handleRegister();
            }}
            onLoginFormChange={setLoginForm}
            onRegisterFormChange={setRegisterForm}
          />
            </CollapsibleSection>

            <CollapsibleSection title="团队" storageKey="section-team" defaultExpanded={!currentTeamId}>
          <TeamSection
            busyAction={busyAction}
            currentTeam={currentTeam}
            currentTeamId={currentTeamId}
            currentUser={currentUser}
            teamDescription={teamDescription}
            teamName={teamName}
            teams={teams}
            onCreateTeam={() => {
              void handleCreateTeam();
            }}
            onCurrentTeamIdChange={setCurrentTeamId}
            onTeamDescriptionChange={setTeamDescription}
            onTeamNameChange={setTeamName}
          />
            </CollapsibleSection>

          {currentUser && currentTeamId ? (
            <CollapsibleSection title="管理" storageKey="section-admin">
              <AdminWorkspaceSection
              busyAction={busyAction}
              canManageIntegrations={Boolean(currentTeamId)}
              conciergeCountByMemberId={conciergeCountByMemberId}
              createdTeam={currentTeam}
              currentUser={currentUser}
              aiMemberForm={aiMemberForm}
              aiTemplateEditorForm={aiTemplateEditorForm}
              aiTemplateLibrary={aiTemplateLibrary}
              aiTemplateOptions={aiTemplateOptions}
              humanMemberForm={humanMemberForm}
              integrationConnections={integrationConnections}
              integrationFiles={integrationFiles}
              integrationFolderPath={integrationFolderPath}
              integrationForm={integrationForm}
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
              currentProjectId={selectedProjectId}
              currentProject={selectedProject}
              myAuditLogs={myAuditLogs}
              myInvitations={myInvitations}
              myTeamsCount={teams.length}
              invitationForm={invitationForm}
              onAcceptInvitation={invitation => {
                void handleAcceptInvitation(invitation);
              }}
              onAiMemberFormChange={setAiMemberForm}
              onAiTemplateEditorFormChange={setAiTemplateEditorForm}
              onApplyAiTemplate={handleApplyAiTemplate}
              onCreateAiMember={() => {
                void handleCreateAiMember();
              }}
              onCreateAiTemplateTemplate={() => {
                void handleCreateAiTemplateTemplate();
              }}
              onCreateHumanMember={() => {
                void handleCreateHumanMember();
              }}
              onCreateInvitation={() => {
                void handleCreateInvitation();
              }}
              onDuplicateAiTemplate={handleDuplicateAiTemplate}
              onEditAiTemplate={handleEditAiTemplate}
              onCreateIntegration={() => {
                void handleCreateIntegration();
              }}
              onHumanMemberFormChange={setHumanMemberForm}
              onImportPreviewCustomer={(externalRecordId, forceUpdate) => {
                void handleImportPreviewCustomer(externalRecordId, forceUpdate);
              }}
              onImportPreviewProject={(externalRecordId, forceUpdate) => {
                void handleImportPreviewProject(externalRecordId, forceUpdate);
              }}
              onImportPreviewTicket={(externalRecordId, forceUpdate) => {
                void handleImportPreviewTicket(externalRecordId, forceUpdate);
              }}
              onIntegrationFolderPathChange={setIntegrationFolderPath}
              onIntegrationFormChange={setIntegrationForm}
              onNavigateToCustomer={navigateToCustomer}
              onNavigateToProject={navigateToProject}
              onNavigateToTicket={navigateToTicket}
              onRefreshIntegrationPreview={() => {
                void handleRefreshIntegrationPreview();
              }}
              onRetryLatestIntegrationIssue={() => {
                if (!selectedIntegration) {
                  return;
                }

                if (
                  selectedIntegrationHealth
                  && (!selectedIntegrationHealth.isAuthenticated || !selectedIntegrationHealth.isReachable)
                ) {
                  void handleValidateIntegration();
                  return;
                }

                void handleRefreshIntegrationPreview();
              }}
              onInvitationFormChange={setInvitationForm}
              onLogoutAll={() => {
                void handleLogoutAll();
              }}
              onRemoveMember={member => {
                void handleRemoveMember(member);
              }}
              onRevokeInvitation={invitation => {
                void handleRevokeInvitation(invitation);
              }}
              onRevokeSession={(sessionId, isCurrent) => {
                void handleRevokeSession(sessionId, isCurrent);
              }}
              onSaveTeamSettings={() => {
                void handleSaveTeamSettings();
              }}
              onSelectIntegrationId={integrationId => {
                void handleSelectIntegrationId(integrationId);
              }}
              onStartNewAiTemplate={handleStartNewAiTemplate}
              onTeamSettingsFormChange={setTeamSettingsForm}
              onToggleAiTemplate={template => {
                void handleToggleAiTemplate(template);
              }}
              onUpdateMemberRole={(member, role) => {
                void handleUpdateMemberRole(member, role);
              }}
              onUpdateAiTemplateTemplate={() => {
                void handleUpdateAiTemplateTemplate();
              }}
              onValidateIntegration={() => {
                void handleValidateIntegration();
              }}
              projectsLeadCountByMemberId={projectsLeadCountByMemberId}
              selectedAiTemplateId={selectedAiTemplateId}
              selectedIntegration={selectedIntegration}
              selectedIntegrationHealth={selectedIntegrationHealth}
              selectedIntegrationId={selectedIntegrationId}
              teamAuditLogs={teamAuditLogs}
              teamInvitations={teamInvitations}
              teamMembers={teamMembers}
              teamSettingsForm={teamSettingsForm}
              userSessions={userSessions}
            />
            </CollapsibleSection>
          ) : null}

          {currentUser && currentTeamId ? (
            <CollapsibleSection title="业务" storageKey="section-business">
              <BusinessWorkspaceSection
              autoRunConversationWorkflow={autoRunConversationWorkflow}
              autoRunTicketWorkflow={autoRunTicketWorkflow}
              busyAction={busyAction}
              canManage={Boolean(currentTeamId)}
              conciergeApps={conciergeApps}
              conciergeUpdateForm={conciergeUpdateForm}
              conversationDetail={conversationDetail}
              conversationDetailError={conversationDetailError}
              conversations={conversations}
              createConciergeForm={createConciergeForm}
              createConversationForm={createConversationForm}
              createCustomerForm={createCustomerForm}
              createProjectForm={createProjectForm}
              createTicketForm={createTicketForm}
              allTickets={tickets}
              customerUpdateForm={customerUpdateForm}
              customers={customers}
              customersLoading={isCustomersLoading}
              customersError={customersError}
              onRetryCustomers={() => currentTeamId && refreshCustomers(currentTeamId)}
              filteredConversations={filteredConversations}
              conversationsLoading={isConversationsLoading}
              conversationsError={conversationsError}
              onRetryConversations={() => refreshConversations()}
              filteredTickets={filteredTickets}
              ticketsLoading={isTicketsLoading}
              ticketsError={ticketsError}
              onRetryTickets={() => refreshTickets()}
              isConversationDetailLoading={isConversationDetailLoading}
              isTicketDetailLoading={isTicketDetailLoading}
              projectUpdateForm={projectUpdateForm}
              projects={projects}
              projectsLoading={isResourcesLoading}
              projectsError={resourcesError}
              onRetryProjects={() => currentTeamId && refreshWorkspaceData(currentTeamId)}
              relatedTickets={relatedTickets}
              selectedConciergeApp={selectedConciergeApp}
              selectedConciergeAppId={selectedConciergeAppId}
              selectedConversation={selectedConversation}
              selectedConversationId={selectedConversationId}
              selectedCustomer={selectedCustomer}
              selectedCustomerId={selectedCustomerId}
              selectedProject={selectedProject}
              selectedProjectId={selectedProjectId}
              selectedProjectParticipants={selectedProjectParticipants}
              selectedTicket={selectedTicket}
              selectedTicketId={selectedTicketId}
              teamMembers={teamMembers}
              ticketCommentDraft={ticketCommentDraft}
              ticketDetail={ticketDetail}
              ticketDetailError={ticketDetailError}
              ticketUpdateDrafts={ticketUpdateDrafts}
              projectWorkflowForm={projectWorkflowForm}
              projectWorkflowTemplates={projectWorkflowTemplates}
              projectWorkflowRuns={projectWorkflowRuns}
              projectSelectedWorkflow={projectSelectedWorkflow}
              projectSelectedWorkflowId={projectSelectedWorkflowId}
              customerWorkflowForm={customerWorkflowForm}
              customerWorkflowTemplates={customerWorkflowTemplates}
              customerWorkflowRuns={customerWorkflowRuns}
              customerSelectedWorkflow={customerSelectedWorkflow}
              customerSelectedWorkflowId={customerSelectedWorkflowId}
              customerWorkflowScope={customerWorkflowScope}
              customerWorkflowScopeLabel={customerWorkflowScopeLabel}
              onConciergeUpdateFormChange={setConciergeUpdateForm}
              onCreateConciergeApp={() => {
                void handleCreateConciergeApp();
              }}
              onCreateConciergeFormChange={setCreateConciergeForm}
              onCreateConversation={() => {
                void handleCreateConversation();
              }}
              onAutoRunConversationWorkflowChange={setAutoRunConversationWorkflow}
              onCreateConversationFormChange={setCreateConversationForm}
              onCreateCustomer={() => {
                void handleCreateCustomer();
              }}
              onCreateCustomerFormChange={setCreateCustomerForm}
              onCreateProject={() => {
                void handleCreateProject();
              }}
              onCreateProjectFormChange={setCreateProjectForm}
              onCreateTicket={() => {
                void handleCreateTicket();
              }}
              onAutoRunTicketWorkflowChange={setAutoRunTicketWorkflow}
              onCreateTicketFormChange={setCreateTicketForm}
              onCustomerUpdateFormChange={setCustomerUpdateForm}
              onProjectUpdateFormChange={setProjectUpdateForm}
              onRunConversationWorkflow={() => {
                void handleRunCustomerWorkflow();
              }}
              onRunProjectWorkflow={() => {
                void handleRunProjectWorkflow();
              }}
              onSaveConciergeApp={() => {
                void handleSaveConciergeApp();
              }}
              onSaveCustomer={() => {
                void handleSaveCustomer();
              }}
              onSaveProject={() => {
                void handleSaveProject();
              }}
              onSaveTicket={ticket => {
                void handleSaveTicket(ticket);
              }}
              onSelectConciergeAppId={navigateToConciergeApp}
              onSelectConversationId={navigateToConversation}
              onSelectCustomerId={navigateToCustomer}
              onSelectProjectId={navigateToProject}
              onSelectTicketId={navigateToTicket}
              onProjectSelectWorkflowId={selectProjectWorkflow}
              onCustomerSelectWorkflowId={selectCustomerWorkflow}
              onTicketCommentDraftChange={setTicketCommentDraft}
              onTicketDraftsChange={setTicketUpdateDrafts}
              onAddTicketComment={() => {
                void handleAddComment();
              }}
              onProjectWorkflowFormChange={setProjectWorkflowForm}
              onCustomerWorkflowFormChange={setCustomerWorkflowForm}
            />
            </CollapsibleSection>
          ) : null}

          <div className="panel-title panel-title-gap">快捷问题</div>
          <div className="prompt-list">
            {[
              '帮我总结今天的项目进展',
              '给出一个客服机器人欢迎语',
              '解释一下 Microsoft Agent Framework 的作用',
            ].map(prompt => (
              <button
                className="prompt-chip"
                key={prompt}
                type="button"
                onClick={() => setChatInput(prompt)}
              >
                {prompt}
              </button>
            ))}
          </div>
        </aside>

        <ChatPanel
          isStreaming={isStreaming}
          selectedProjectId={selectedProjectId}
          selectedProjectName={selectedProject?.name ?? '未选项目'}
          selectedCustomerId={selectedCustomerId}
          selectedCustomerName={selectedCustomer?.displayName ?? '未选客户'}
          selectedTicketId={selectedTicketId}
          selectedTicketTitle={selectedTicket?.title ?? '未选工单'}
          onSelectProjectContext={() => {
            if (selectedProjectId) {
              navigateToProject(selectedProjectId);
            }
          }}
          onSelectCustomerContext={() => {
            if (selectedCustomerId) {
              navigateToCustomer(selectedCustomerId);
            }
          }}
          onSelectTicketContext={() => {
            if (selectedTicketId) {
              navigateToTicket(selectedTicketId);
            }
          }}
          onStop={handleStop}
          scrollRef={scrollRef}
          messages={messages}
          displayedTexts={displayedTexts}
          input={chatInput}
          onInputChange={setChatInput}
          onSubmit={() => {
            void handleSendMessage();
          }}
          errorMessage={chatError}
          onSyncConversationDraft={() => {
            const draftMessage = chatInput.trim();
            if (!draftMessage) {
              setFeedback({
                kind: 'error',
                text: '先输入一段聊天内容，再同步到会话草稿。',
              });
              return;
            }

            setCreateConversationForm(current => ({
              ...current,
              customerId: current.customerId || (selectedCustomer?.id ?? ''),
              customerDisplayName: current.customerDisplayName || (selectedCustomer?.displayName ?? ''),
              customerEmail: current.customerEmail || (selectedCustomer?.email ?? ''),
              initialMessage: draftMessage,
            }));
            setFeedback({
              kind: 'success',
              text: '已把聊天内容同步到会话草稿。',
            });
          }}
          onSyncTicketDraft={() => {
            const draftMessage = chatInput.trim();
            if (!draftMessage) {
              setFeedback({
                kind: 'error',
                text: '先输入一段聊天内容，再同步到工单草稿。',
              });
              return;
            }

            setCreateTicketForm(current => ({
              ...current,
              title: current.title || `${selectedCustomer?.displayName ?? '客户'}需求`,
              summary: draftMessage,
            }));
            setFeedback({
              kind: 'success',
              text: '已把聊天内容同步到工单草稿。',
            });
          }}
        />
      </div>
    </div>
  );
}
