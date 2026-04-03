import { useEffect, useMemo } from 'react';
import { ChatPanel } from './components/ChatPanel';
import { AdminWorkspaceSection } from './components/AdminWorkspaceSection';
import { AuthSection } from './components/AuthSection';
import { BusinessWorkspaceSection } from './components/BusinessWorkspaceSection';
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
    conversations,
    conversationDetail,
    conversationDetailError,
    createConversationForm,
    filteredConversations,
    isConversationDetailLoading,
    selectedConversation,
    selectedConversationId,
    setCreateConversationForm,
    setSelectedConversationId,
    handleCreateConversation,
  } = useWorkspaceConversations({
    selectedConciergeApp,
    selectedCustomer,
    selectedCustomerId,
    token,
    runAction,
    setFeedback,
  });
  const {
    createTicketForm,
    filteredTickets,
    isTicketDetailLoading,
    relatedTickets,
    tickets,
    selectedTicket,
    selectedTicketId,
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
    setHumanMemberForm,
    setInvitationForm,
    setTeamSettingsForm,
    handleApplyAiTemplate,
    handleAcceptInvitation,
    handleCreateAiMember,
    handleCreateHumanMember,
    handleCreateInvitation,
    handleLogoutAll,
    handleRemoveMember,
    handleRevokeInvitation,
    handleRevokeSession,
    handleSaveTeamSettings,
    handleUpdateMemberRole,
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
    integrationForm,
    integrationPreviewCount,
    integrationPreviewCustomers,
    integrationPreviewProjects,
    integrationPreviewTasks,
    integrationPreviewTickets,
    selectedIntegration,
    selectedIntegrationHealth,
    selectedIntegrationId,
    setIntegrationForm,
    handleCreateIntegration,
    handleSelectIntegrationId,
    handleValidateIntegration,
  } = useWorkspaceIntegrations({
    currentTeamId,
    token,
    runAction,
    setFeedback,
  });
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

          {currentUser && currentTeamId ? (
            <AdminWorkspaceSection
              busyAction={busyAction}
              canManageIntegrations={Boolean(currentTeamId)}
              conciergeCountByMemberId={conciergeCountByMemberId}
              createdTeam={currentTeam}
              currentUser={currentUser}
              aiMemberForm={aiMemberForm}
              aiTemplateOptions={aiTemplateOptions}
              humanMemberForm={humanMemberForm}
              integrationConnections={integrationConnections}
              integrationFiles={integrationFiles}
              integrationForm={integrationForm}
              integrationPreviewCount={integrationPreviewCount}
              integrationPreviewCustomers={integrationPreviewCustomers}
              integrationPreviewProjects={integrationPreviewProjects}
              integrationPreviewTasks={integrationPreviewTasks}
              integrationPreviewTickets={integrationPreviewTickets}
              myAuditLogs={myAuditLogs}
              myInvitations={myInvitations}
              myTeamsCount={teams.length}
              invitationForm={invitationForm}
              onAcceptInvitation={invitation => {
                void handleAcceptInvitation(invitation);
              }}
              onAiMemberFormChange={setAiMemberForm}
              onApplyAiTemplate={handleApplyAiTemplate}
              onCreateAiMember={() => {
                void handleCreateAiMember();
              }}
              onCreateHumanMember={() => {
                void handleCreateHumanMember();
              }}
              onCreateInvitation={() => {
                void handleCreateInvitation();
              }}
              onCreateIntegration={() => {
                void handleCreateIntegration();
              }}
              onHumanMemberFormChange={setHumanMemberForm}
              onIntegrationFormChange={setIntegrationForm}
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
              onTeamSettingsFormChange={setTeamSettingsForm}
              onUpdateMemberRole={(member, role) => {
                void handleUpdateMemberRole(member, role);
              }}
              onValidateIntegration={() => {
                void handleValidateIntegration();
              }}
              projectsLeadCountByMemberId={projectsLeadCountByMemberId}
              selectedIntegration={selectedIntegration}
              selectedIntegrationHealth={selectedIntegrationHealth}
              selectedIntegrationId={selectedIntegrationId}
              teamAuditLogs={teamAuditLogs}
              teamInvitations={teamInvitations}
              teamMembers={teamMembers}
              teamSettingsForm={teamSettingsForm}
              userSessions={userSessions}
            />
          ) : null}

          {currentUser && currentTeamId ? (
            <BusinessWorkspaceSection
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
              filteredConversations={filteredConversations}
              filteredTickets={filteredTickets}
              isConversationDetailLoading={isConversationDetailLoading}
              isTicketDetailLoading={isTicketDetailLoading}
              projectUpdateForm={projectUpdateForm}
              projects={projects}
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
