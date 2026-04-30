import type { Dispatch, SetStateAction } from 'react';
import type {
  AgentWorkflowResponse,
  ConversationDetailResponse,
  ConversationSummaryResponse,
  CreateConversationRequest,
  CreateCustomerRequest,
  CreateTicketRequest,
  CustomerResponse,
  MemberResponse,
  ProjectResponse,
  RunTicketWorkflowRequest,
  TicketResponse,
  UpdateCustomerRequest,
  UpdateTicketRequest,
} from '../../generated/api';
import type { TicketDetailItem } from '../types';
import { ConversationSection } from './ConversationSection';
import { CustomerSection } from './CustomerSection';
import { TicketSection } from './TicketSection';
import { WorkflowPanel } from './WorkflowPanel';
import type { WorkflowTemplateItem } from '../types';

type CustomerOpsSectionProps = {
  autoRunConversationWorkflow: boolean;
  autoRunTicketWorkflow: boolean;
  busyAction: string | null;
  canManage: boolean;
  conversationDetail: ConversationDetailResponse | null;
  conversationDetailError: string | null;
  createConversationForm: CreateConversationRequest;
  createCustomerForm: CreateCustomerRequest;
  createTicketForm: CreateTicketRequest;
  customerUpdateForm: UpdateCustomerRequest;
  customers: CustomerResponse[];
  filteredConversations: ConversationSummaryResponse[];
  filteredTickets: TicketResponse[];
  isConversationDetailLoading: boolean;
  isTicketDetailLoading: boolean;
  projects: ProjectResponse[];
  relatedTickets: TicketResponse[];
  selectedConversation: ConversationSummaryResponse | null;
  selectedConversationId: string;
  selectedCustomer: CustomerResponse | null;
  selectedCustomerId: string;
  selectedTicket: TicketResponse | null;
  selectedTicketId: string;
  teamMembers: MemberResponse[];
  ticketCommentDraft: string;
  ticketDetail: TicketDetailItem | null;
  ticketDetailError: string | null;
  ticketUpdateDrafts: Record<string, UpdateTicketRequest>;
  workflowForm: RunTicketWorkflowRequest;
  workflowTemplates: WorkflowTemplateItem[];
  workflowRuns: AgentWorkflowResponse[];
  selectedWorkflow: AgentWorkflowResponse | null;
  selectedWorkflowId: string;
  workflowScope: 'ticket' | 'conversation' | null;
  workflowScopeLabel: string;
  onAddTicketComment: () => void;
  onAutoRunConversationWorkflowChange: (value: boolean) => void;
  onAutoRunTicketWorkflowChange: (value: boolean) => void;
  onCreateConversation: () => void;
  onCreateConversationFormChange: Dispatch<SetStateAction<CreateConversationRequest>>;
  onCreateCustomer: () => void;
  onCreateCustomerFormChange: Dispatch<SetStateAction<CreateCustomerRequest>>;
  onCreateTicket: () => void;
  onCreateTicketFormChange: Dispatch<SetStateAction<CreateTicketRequest>>;
  onCustomerUpdateFormChange: Dispatch<SetStateAction<UpdateCustomerRequest>>;
  onRunConversationWorkflow: () => void;
  onSaveCustomer: () => void;
  onSaveTicket: (ticket: TicketResponse) => void;
  onSelectConciergeAppId: (conciergeAppId: string) => void;
  onSelectConversationId: (conversationId: string) => void;
  onSelectCustomerId: (customerId: string) => void;
  onSelectProjectId: (projectId: string) => void;
  onSelectTicketId: (ticketId: string) => void;
  onSelectWorkflowId: (workflowId: string) => void;
  onTicketCommentDraftChange: (value: string) => void;
  onTicketDraftsChange: Dispatch<SetStateAction<Record<string, UpdateTicketRequest>>>;
  onWorkflowFormChange: Dispatch<SetStateAction<RunTicketWorkflowRequest>>;
};

export function CustomerOpsSection({
  autoRunConversationWorkflow,
  autoRunTicketWorkflow,
  busyAction,
  canManage,
  conversationDetail,
  conversationDetailError,
  createConversationForm,
  createCustomerForm,
  createTicketForm,
  customerUpdateForm,
  customers,
  filteredConversations,
  filteredTickets,
  isConversationDetailLoading,
  isTicketDetailLoading,
  projects,
  relatedTickets,
  selectedConversation,
  selectedConversationId,
  selectedCustomer,
  selectedCustomerId,
  selectedTicket,
  selectedTicketId,
  teamMembers,
  ticketCommentDraft,
  ticketDetail,
  ticketDetailError,
  ticketUpdateDrafts,
  workflowForm,
  workflowTemplates,
  workflowRuns,
  selectedWorkflow,
  selectedWorkflowId,
  workflowScope,
  workflowScopeLabel,
  onAddTicketComment,
  onAutoRunConversationWorkflowChange,
  onAutoRunTicketWorkflowChange,
  onCreateConversation,
  onCreateConversationFormChange,
  onCreateCustomer,
  onCreateCustomerFormChange,
  onCreateTicket,
  onCreateTicketFormChange,
  onCustomerUpdateFormChange,
  onRunConversationWorkflow,
  onSaveCustomer,
  onSaveTicket,
  onSelectConciergeAppId,
  onSelectConversationId,
  onSelectCustomerId,
  onSelectProjectId,
  onSelectTicketId,
  onSelectWorkflowId,
  onTicketCommentDraftChange,
  onTicketDraftsChange,
  onWorkflowFormChange,
}: CustomerOpsSectionProps) {
  return (
    <>
      <CustomerSection
        busyAction={busyAction}
        canManageCustomers={canManage}
        createCustomerForm={createCustomerForm}
        customerUpdateForm={customerUpdateForm}
        customers={customers}
        filteredConversations={filteredConversations}
        filteredTickets={filteredTickets}
        projects={projects}
        selectedCustomer={selectedCustomer}
        selectedCustomerId={selectedCustomerId}
        onCreateCustomer={onCreateCustomer}
        onCreateCustomerFormChange={onCreateCustomerFormChange}
        onCustomerUpdateFormChange={onCustomerUpdateFormChange}
        onSaveCustomer={onSaveCustomer}
        onSelectCustomerId={onSelectCustomerId}
        onSelectRelatedConversationId={onSelectConversationId}
        onSelectRelatedProjectId={onSelectProjectId}
        onSelectRelatedTicketId={onSelectTicketId}
      />

      <ConversationSection
        autoRunConversationWorkflow={autoRunConversationWorkflow}
        busyAction={busyAction}
        canManageConversations={canManage}
        conversationDetail={conversationDetail}
        conversationDetailError={conversationDetailError}
        createConversationForm={createConversationForm}
        filteredConversations={filteredConversations}
        isConversationDetailLoading={isConversationDetailLoading}
        relatedTickets={relatedTickets}
        selectedConversation={selectedConversation}
        selectedConversationId={selectedConversationId}
        selectedCustomer={selectedCustomer}
        onCreateConversation={onCreateConversation}
        onCreateConversationFormChange={onCreateConversationFormChange}
        onAutoRunConversationWorkflowChange={onAutoRunConversationWorkflowChange}
        onRunConversationWorkflow={onRunConversationWorkflow}
        onSelectConversationId={onSelectConversationId}
        onSelectRelatedConciergeAppId={onSelectConciergeAppId}
        onSelectRelatedCustomerId={onSelectCustomerId}
        onSelectRelatedTicketId={onSelectTicketId}
      />

      <TicketSection
        autoRunTicketWorkflow={autoRunTicketWorkflow}
        busyAction={busyAction}
        canManageTickets={canManage}
        createTicketForm={createTicketForm}
        filteredTickets={filteredTickets}
        isTicketDetailLoading={isTicketDetailLoading}
        relatedTickets={relatedTickets}
        selectedConversation={selectedConversation}
        selectedTicket={selectedTicket}
        selectedTicketId={selectedTicketId}
        teamMembers={teamMembers}
        ticketCommentDraft={ticketCommentDraft}
        ticketDetail={ticketDetail}
        ticketDetailError={ticketDetailError}
        ticketUpdateDrafts={ticketUpdateDrafts}
        onAddComment={onAddTicketComment}
        onAutoRunTicketWorkflowChange={onAutoRunTicketWorkflowChange}
        onCreateTicket={onCreateTicket}
        onCreateTicketFormChange={onCreateTicketFormChange}
        onSaveTicket={onSaveTicket}
        onSelectRelatedConciergeAppId={onSelectConciergeAppId}
        onSelectRelatedConversationId={onSelectConversationId}
        onSelectRelatedCustomerId={onSelectCustomerId}
        onSelectRelatedProjectId={onSelectProjectId}
        onSelectTicketId={onSelectTicketId}
        onTicketCommentDraftChange={onTicketCommentDraftChange}
        onTicketDraftsChange={onTicketDraftsChange}
      />

      <WorkflowPanel
        currentWorkflowScope={workflowScope}
        currentWorkflowScopeLabel={workflowScopeLabel}
        currentScopeWorkflowTemplates={workflowTemplates}
        workflowForm={workflowForm}
        onWorkflowFormChange={onWorkflowFormChange}
        aiMembers={teamMembers.filter(member => member.aiProfile)}
        autonomousAiMembers={teamMembers.filter(member => member.aiProfile?.isAutonomous)}
        busyAction={busyAction}
        canRunWorkflow={canManage && Boolean(workflowScope)}
        onRunWorkflow={onRunConversationWorkflow}
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
