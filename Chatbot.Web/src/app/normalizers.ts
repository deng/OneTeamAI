import type { AuditLogItem, TicketDetailItem, UserSessionItem } from './types';

export function normalizeAuditLogItem(value: {
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
  createdAt: string | Date;
}): AuditLogItem {
  return {
    ...value,
    createdAt: value.createdAt instanceof Date ? value.createdAt : new Date(value.createdAt),
  };
}

export function normalizeUserSessionItem(value: {
  id: string;
  createdAt: string | Date;
  lastSeenAt: string | Date;
  expiresAt: string | Date;
  revokedAt?: string | Date | null;
  revokedReason?: string | null;
  userAgent?: string | null;
  ipAddress?: string | null;
  isCurrent: boolean;
}): UserSessionItem {
  return {
    ...value,
    createdAt: value.createdAt instanceof Date ? value.createdAt : new Date(value.createdAt),
    lastSeenAt: value.lastSeenAt instanceof Date ? value.lastSeenAt : new Date(value.lastSeenAt),
    expiresAt: value.expiresAt instanceof Date ? value.expiresAt : new Date(value.expiresAt),
    revokedAt:
      value.revokedAt instanceof Date
        ? value.revokedAt
        : (value.revokedAt ? new Date(value.revokedAt) : null),
  };
}

export function normalizeTicketDetailItem(value: {
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
  dueAt?: string | Date | null;
  lastActivityAt?: string | Date | null;
  assignedMemberId?: string | null;
  assignedMemberName?: string | null;
  activities: Array<{
    id: string;
    activityType: number;
    summary: string;
    detail?: string | null;
    actorMemberId?: string | null;
    actorMemberName?: string | null;
    actorUserId?: string | null;
    actorUserName?: string | null;
    createdAt: string | Date;
  }>;
}): TicketDetailItem {
  return {
    ...value,
    dueAt: value.dueAt instanceof Date ? value.dueAt : (value.dueAt ? new Date(value.dueAt) : null),
    lastActivityAt:
      value.lastActivityAt instanceof Date
        ? value.lastActivityAt
        : (value.lastActivityAt ? new Date(value.lastActivityAt) : null),
    activities: value.activities.map(activity => ({
      ...activity,
      createdAt: activity.createdAt instanceof Date ? activity.createdAt : new Date(activity.createdAt),
    })),
  };
}
