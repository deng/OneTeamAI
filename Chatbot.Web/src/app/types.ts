import type { AiMemberTemplateResponse, CreateAiMemberRequest } from '../generated/api';

export type PublicConciergeApp = {
  id: string;
  name?: string | null;
  description?: string | null;
  serviceScope?: string | null;
  welcomeMessage?: string | null;
  faqScope?: string | null;
  intakeGuidance?: string | null;
  suggestedPrompts?: string | null;
  businessHours?: string | null;
  channelLabel?: string | null;
  requireEmail?: boolean;
  requirePhoneNumber?: boolean;
  status?: number;
  teamBrandName?: string | null;
  projectName?: string | null;
};

export type AuditLogItem = {
  id: string;
  teamId?: string | null;
  userId?: string | null;
  userDisplayName?: string | null;
  actionType: string;
  entityType: string;
  entityId?: string | null;
  summary: string;
  result: string;
  ipAddress?: string | null;
  createdAt: Date;
};

export type UserSessionItem = {
  id: string;
  createdAt: Date;
  lastSeenAt: Date;
  expiresAt: Date;
  revokedAt?: Date | null;
  revokedReason?: string | null;
  userAgent?: string | null;
  ipAddress?: string | null;
  isCurrent: boolean;
};

export type TicketActivityItem = {
  id: string;
  activityType: number;
  summary: string;
  detail?: string | null;
  actorMemberId?: string | null;
  actorMemberName?: string | null;
  actorUserId?: string | null;
  actorUserName?: string | null;
  createdAt: Date;
};

export type TicketDetailItem = {
  id: string;
  teamId: string;
  projectId: string;
  conciergeAppId?: string | null;
  customerId?: string | null;
  customerName?: string | null;
  conversationId?: string | null;
  title: string;
  summary: string;
  category?: string | null;
  status: number;
  priority: number;
  dueAt?: Date | null;
  resolutionSummary?: string | null;
  resolvedAt?: Date | null;
  lastActivityAt?: Date | null;
  assignedMemberId?: string | null;
  assignedMemberName?: string | null;
  activities: TicketActivityItem[];
};

export type WorkflowTemplateItem = {
  key: string;
  scope: 'ticket' | 'conversation' | 'project';
  label: string;
  goal: string;
  summary: string;
};

export type AiMemberTemplateItem = AiMemberTemplateResponse;

export type AiMemberTemplateEditorForm = {
  key: string;
  label: string;
  displayName: string;
  jobTitle: string;
  responsibilitySummary: string;
  title: string;
  permissionBoundary: string;
  systemPrompt: string;
  allowedTools: string;
  executableActions: string;
  knowledgeScope: string;
  isAutonomous: boolean;
  sortOrder: string;
  isEnabled: boolean;
};

export type Feedback = {
  kind: 'success' | 'error';
  text: string;
};

export type ChatMessagePart = {
  type: 'text';
  text: string;
};

export type ChatMessage = {
  id: string;
  role: 'user' | 'assistant';
  parts: ChatMessagePart[];
};
