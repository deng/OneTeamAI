import { useAuthContext, useStatusContext, useTeamContext, useResourcesContext, useCustomersContext, useConversationsContext, useTicketsContext, useWorkflowsContext, useNavigationContext } from '../workspaceContexts';
import { CustomerOpsSection } from './CustomerOpsSection';
import { ProjectOpsSection } from './ProjectOpsSection';

export function BusinessWorkspaceSection() {
  const { busyAction } = useStatusContext();
  const { currentUser } = useAuthContext();
  const { currentTeamId } = useTeamContext();
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
    handleCreateConciergeApp,
    handleCreateProject,
    handleSaveConciergeApp,
    handleSaveProject,
  } = useResourcesContext();
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
    handleCreateCustomer,
    handleSaveCustomer,
  } = useCustomersContext();
  const {
    autoRunConversationWorkflow,
    conversationDetail,
    conversationDetailError,
    conversations,
    conversationsError,
    createConversationForm,
    filteredConversations,
    isConversationDetailLoading,
    isConversationsLoading,
    selectedConversation,
    selectedConversationId,
    setAutoRunConversationWorkflow,
    setCreateConversationForm,
    handleCreateConversation,
    refreshConversations,
  } = useConversationsContext();
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
    setTicketCommentDraft,
    setTicketUpdateDrafts,
    handleAddComment,
    handleCreateTicket,
    handleSaveTicket,
  } = useTicketsContext();
  const workflows = useWorkflowsContext();
  const {
    navigateToProject,
    navigateToConciergeApp,
    navigateToCustomer,
    navigateToConversation,
    navigateToTicket,
    selectProjectWorkflow,
    selectCustomerWorkflow,
  } = useNavigationContext();

  return (
    <>
      <ProjectOpsSection
        busyAction={busyAction}
        canManage={Boolean(currentTeamId)}
        conciergeApps={conciergeApps}
        conciergeUpdateForm={conciergeUpdateForm}
        conversations={conversations}
        createConciergeForm={createConciergeForm}
        createProjectForm={createProjectForm}
        customers={customers}
        projectUpdateForm={projectUpdateForm}
        projects={projects}
        projectsLoading={isResourcesLoading}
        projectsError={resourcesError}
        onRetryProjects={() => currentTeamId && refreshWorkspaceData(currentTeamId)}
        selectedConciergeApp={selectedConciergeApp}
        selectedConciergeAppId={selectedConciergeAppId}
        selectedProject={selectedProject}
        selectedProjectId={selectedProjectId}
        selectedProjectParticipants={selectedProjectParticipants}
        teamMembers={teamMembers}
        tickets={tickets}
        workflowForm={workflows.project.workflowForm}
        workflowTemplates={workflows.project.workflowTemplates}
        workflowRuns={workflows.project.workflowRuns}
        selectedWorkflow={workflows.project.selectedWorkflow}
        selectedWorkflowId={workflows.project.selectedWorkflowId}
        onConciergeUpdateFormChange={setConciergeUpdateForm}
        onCreateConciergeApp={handleCreateConciergeApp}
        onCreateConciergeFormChange={setCreateConciergeForm}
        onCreateProject={handleCreateProject}
        onCreateProjectFormChange={setCreateProjectForm}
        onProjectUpdateFormChange={setProjectUpdateForm}
        onRunProjectWorkflow={workflows.project.handleRunWorkflow}
        onSaveConciergeApp={handleSaveConciergeApp}
        onSaveProject={handleSaveProject}
        onSelectConciergeAppId={navigateToConciergeApp}
        onSelectConversationId={navigateToConversation}
        onSelectCustomerId={navigateToCustomer}
        onSelectProjectId={navigateToProject}
        onSelectTicketId={navigateToTicket}
        onSelectWorkflowId={selectProjectWorkflow}
        onWorkflowFormChange={workflows.project.setWorkflowForm}
      />

      <CustomerOpsSection
        autoRunConversationWorkflow={autoRunConversationWorkflow}
        autoRunTicketWorkflow={autoRunTicketWorkflow}
        busyAction={busyAction}
        canManage={Boolean(currentTeamId)}
        conversationDetail={conversationDetail}
        conversationDetailError={conversationDetailError}
        createConversationForm={createConversationForm}
        createCustomerForm={createCustomerForm}
        createTicketForm={createTicketForm}
        customerUpdateForm={customerUpdateForm}
        customers={customers}
        customersLoading={isCustomersLoading}
        customersError={customersError}
        onRetryCustomers={() => currentTeamId && refreshCustomers(currentTeamId)}
        filteredConversations={filteredConversations}
        conversationsLoading={isConversationsLoading}
        conversationsError={conversationsError}
        onRetryConversations={refreshConversations}
        filteredTickets={filteredTickets}
        ticketsLoading={isTicketsLoading}
        ticketsError={ticketsError}
        onRetryTickets={refreshTickets}
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
        workflowForm={workflows.customer.workflowForm}
        workflowTemplates={workflows.customer.workflowTemplates}
        workflowRuns={workflows.customer.workflowRuns}
        selectedWorkflow={workflows.customer.selectedWorkflow}
        selectedWorkflowId={workflows.customer.selectedWorkflowId}
        workflowScope={workflows.customer.workflowScope}
        workflowScopeLabel={workflows.customer.workflowScopeLabel}
        onAddTicketComment={handleAddComment}
        onAutoRunConversationWorkflowChange={setAutoRunConversationWorkflow}
        onAutoRunTicketWorkflowChange={setAutoRunTicketWorkflow}
        onCreateConversation={handleCreateConversation}
        onCreateConversationFormChange={setCreateConversationForm}
        onCreateCustomer={handleCreateCustomer}
        onCreateCustomerFormChange={setCreateCustomerForm}
        onCreateTicket={handleCreateTicket}
        onCreateTicketFormChange={setCreateTicketForm}
        onCustomerUpdateFormChange={setCustomerUpdateForm}
        onRunConversationWorkflow={workflows.customer.handleRunWorkflow}
        onSaveCustomer={handleSaveCustomer}
        onSaveTicket={handleSaveTicket}
        onSelectConciergeAppId={navigateToConciergeApp}
        onSelectConversationId={navigateToConversation}
        onSelectCustomerId={navigateToCustomer}
        onSelectProjectId={navigateToProject}
        onSelectTicketId={navigateToTicket}
        onSelectWorkflowId={selectCustomerWorkflow}
        onTicketCommentDraftChange={setTicketCommentDraft}
        onTicketDraftsChange={setTicketUpdateDrafts}
        onWorkflowFormChange={workflows.customer.setWorkflowForm}
      />
    </>
  );
}
