import { useEffect, useMemo, useState } from 'react';
import {
  MemberType,
  MemberRole,
  type CreateAiMemberRequest,
  type CreateHumanMemberRequest,
  type CreateInvitationRequest,
  type InvitationResponse,
  type MemberResponse,
  type TeamSummaryResponse,
  type UpdateTeamRequest,
  type UserResponse,
} from '../generated/api';
import { aiRoleTemplates } from './constants';
import { normalizeAuditLogItem, normalizeUserSessionItem } from './normalizers';
import type { AuditLogItem, UserSessionItem } from './types';
import { createWorkspaceApis, fetchJson } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

function emptyTeamSettingsForm(): UpdateTeamRequest {
  return {
    name: '',
    description: '',
    brandName: '',
  };
}

function emptyHumanMemberForm(): CreateHumanMemberRequest {
  return {
    email: '',
    role: MemberRole.NUMBER_2,
    title: '执行成员',
  };
}

function emptyInvitationForm(): CreateInvitationRequest {
  return {
    email: '',
    role: MemberRole.NUMBER_2,
    title: '执行成员',
    expiresInDays: 7,
  };
}

function defaultAiMemberForm(): CreateAiMemberRequest {
  const template = aiRoleTemplates[0];
  return {
    displayName: template.displayName,
    jobTitle: template.jobTitle,
    responsibilitySummary: template.responsibilitySummary,
    templateKey: template.key,
    permissionBoundary: template.permissionBoundary,
    title: template.title,
    systemPrompt: template.systemPrompt,
    allowedTools: template.allowedTools,
    executableActions: template.executableActions,
    knowledgeScope: template.knowledgeScope,
    isAutonomous: template.isAutonomous,
  };
}

export function useWorkspaceTeamSettings({
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
}: {
  currentTeam: TeamSummaryResponse | null;
  currentTeamId: string;
  currentUser: UserResponse | null;
  token: string | null;
  teamMembers: MemberResponse[];
  projectsLeadCountByMemberId: Record<string, number>;
  conciergeCountByMemberId: Record<string, number>;
  refreshWorkspaceData: (teamId: string) => Promise<void>;
  refreshTeams: () => Promise<void>;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const { teamsApi, membersApi, invitationsApi } = useMemo(() => createWorkspaceApis(token), [token]);
  const [teamSettingsForm, setTeamSettingsForm] = useState<UpdateTeamRequest>(emptyTeamSettingsForm);
  const [humanMemberForm, setHumanMemberForm] = useState<CreateHumanMemberRequest>(emptyHumanMemberForm);
  const [aiMemberForm, setAiMemberForm] = useState<CreateAiMemberRequest>(defaultAiMemberForm);
  const [invitationForm, setInvitationForm] = useState<CreateInvitationRequest>(emptyInvitationForm);
  const [userSessions, setUserSessions] = useState<UserSessionItem[]>([]);
  const [teamInvitations, setTeamInvitations] = useState<InvitationResponse[]>([]);
  const [myInvitations, setMyInvitations] = useState<InvitationResponse[]>([]);
  const [teamAuditLogs, setTeamAuditLogs] = useState<AuditLogItem[]>([]);
  const [myAuditLogs, setMyAuditLogs] = useState<AuditLogItem[]>([]);

  useEffect(() => {
    if (!currentTeam) {
      setTeamSettingsForm(emptyTeamSettingsForm());
      return;
    }

    setTeamSettingsForm({
      name: currentTeam.name ?? '',
      description: currentTeam.description ?? '',
      brandName: currentTeam.brandName ?? '',
    });
  }, [currentTeam]);

  useEffect(() => {
    if (!token || !currentUser) {
      setUserSessions([]);
      setMyInvitations([]);
      setMyAuditLogs([]);
      return;
    }

    let cancelled = false;

    void (async () => {
      const [sessions, invitations, auditLogs] = await Promise.all([
        fetchJson<Array<{
          id: string;
          createdAt: string;
          lastSeenAt: string;
          expiresAt: string;
          revokedAt?: string | null;
          revokedReason?: string | null;
          userAgent?: string | null;
          ipAddress?: string | null;
          isCurrent: boolean;
        }>>('/api/auth/sessions', {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }),
        invitationsApi.listMyInvitations(),
        fetchJson<Array<{
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
          createdAt: string;
        }>>('/api/audit-logs/me', {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }),
      ]);

      if (cancelled) {
        return;
      }

      setUserSessions(sessions.map(normalizeUserSessionItem));
      setMyInvitations(invitations);
      setMyAuditLogs(auditLogs.map(normalizeAuditLogItem));
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: error instanceof Error ? error.message : '团队设置数据加载失败。',
        });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [currentUser, invitationsApi, setFeedback, token]);

  useEffect(() => {
    if (!token || !currentTeamId) {
      setTeamInvitations([]);
      setTeamAuditLogs([]);
      return;
    }

    let cancelled = false;

    void (async () => {
      const [invitations, auditLogs] = await Promise.all([
        invitationsApi.listTeamInvitations({ teamId: currentTeamId }),
        fetchJson<Array<{
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
          createdAt: string;
        }>>(`/api/teams/${currentTeamId}/audit-logs`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }),
      ]);

      if (cancelled) {
        return;
      }

      setTeamInvitations(invitations);
      setTeamAuditLogs(auditLogs.map(normalizeAuditLogItem));
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: error instanceof Error ? error.message : '团队管理数据加载失败。',
        });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [currentTeamId, invitationsApi, setFeedback, token]);

  async function reloadTeamSettingsData() {
    if (!token || !currentUser) {
      return;
    }

    const sessionsPromise = fetchJson<Array<{
      id: string;
      createdAt: string;
      lastSeenAt: string;
      expiresAt: string;
      revokedAt?: string | null;
      revokedReason?: string | null;
      userAgent?: string | null;
      ipAddress?: string | null;
      isCurrent: boolean;
    }>>('/api/auth/sessions', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    const myInvitationsPromise = invitationsApi.listMyInvitations();
    const myAuditLogsPromise = fetchJson<Array<{
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
      createdAt: string;
    }>>('/api/audit-logs/me', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    const teamScopedPromise = currentTeamId
      ? Promise.all([
          invitationsApi.listTeamInvitations({ teamId: currentTeamId }),
          fetchJson<Array<{
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
            createdAt: string;
          }>>(`/api/teams/${currentTeamId}/audit-logs`, {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }),
        ])
      : Promise.resolve<[InvitationResponse[], AuditLogItem[] | Array<{
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
          createdAt: string;
        }>]>([[], []]);

    const [sessions, myInvitationsResult, myAuditLogsResult, teamScoped] = await Promise.all([
      sessionsPromise,
      myInvitationsPromise,
      myAuditLogsPromise,
      teamScopedPromise,
    ]);

    setUserSessions(sessions.map(normalizeUserSessionItem));
    setMyInvitations(myInvitationsResult);
    setMyAuditLogs(myAuditLogsResult.map(normalizeAuditLogItem));
    setTeamInvitations(teamScoped[0]);
    setTeamAuditLogs((teamScoped[1] as Array<{
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
      createdAt: string;
    }>).map(normalizeAuditLogItem));
  }

  async function handleSaveTeamSettings() {
    if (!currentTeamId) {
      return;
    }

    await runAction('update-team', async () => {
      await teamsApi.updateTeam({
        teamId: currentTeamId,
        updateTeamRequest: teamSettingsForm,
      });

      await Promise.all([refreshTeams(), reloadTeamSettingsData()]);
      setFeedback({
        kind: 'success',
        text: '团队设置已更新。',
      });
    });
  }

  function handleApplyAiTemplate(templateKey: string) {
    const template = aiRoleTemplates.find(item => item.key === templateKey);
    if (!template) {
      return;
    }

    setAiMemberForm({
      displayName: template.displayName,
      jobTitle: template.jobTitle,
      responsibilitySummary: template.responsibilitySummary,
      templateKey: template.key,
      permissionBoundary: template.permissionBoundary,
      title: template.title,
      systemPrompt: template.systemPrompt,
      allowedTools: template.allowedTools,
      executableActions: template.executableActions,
      knowledgeScope: template.knowledgeScope,
      isAutonomous: template.isAutonomous,
    });
  }

  async function handleCreateHumanMember() {
    if (!currentTeamId) {
      return;
    }

    await runAction('create-human-member', async () => {
      await membersApi.createHumanMember({
        teamId: currentTeamId,
        createHumanMemberRequest: humanMemberForm,
      });

      await Promise.all([refreshWorkspaceData(currentTeamId), reloadTeamSettingsData()]);
      setHumanMemberForm(emptyHumanMemberForm());
      setFeedback({
        kind: 'success',
        text: `已添加真人成员 ${humanMemberForm.email ?? ''}。`,
      });
    });
  }

  async function handleCreateAiMember() {
    if (!currentTeamId) {
      return;
    }

    await runAction('create-ai-member', async () => {
      await membersApi.createAiMember({
        teamId: currentTeamId,
        createAiMemberRequest: aiMemberForm,
      });

      await Promise.all([refreshWorkspaceData(currentTeamId), reloadTeamSettingsData()]);
      setAiMemberForm(defaultAiMemberForm());
      setFeedback({
        kind: 'success',
        text: `已创建 AI 员工 ${aiMemberForm.displayName ?? ''}。`,
      });
    });
  }

  async function handleCreateInvitation() {
    if (!currentTeamId) {
      return;
    }

    await runAction('create-invitation', async () => {
      await invitationsApi.createInvitation({
        teamId: currentTeamId,
        createInvitationRequest: invitationForm,
      });

      await reloadTeamSettingsData();
      setInvitationForm(emptyInvitationForm());
      setFeedback({
        kind: 'success',
        text: `已向 ${invitationForm.email ?? ''} 发送邀请。`,
      });
    });
  }

  async function handleRevokeSession(sessionId: string, isCurrent: boolean) {
    await runAction('revoke-session', async () => {
      await fetchJson(`/api/auth/sessions/${sessionId}/revoke`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token ?? ''}`,
        },
      });

      await reloadTeamSettingsData();
      if (isCurrent) {
        setFeedback({
          kind: 'success',
          text: '当前会话已撤销，请重新登录。',
        });
      } else {
        setFeedback({
          kind: 'success',
          text: '会话已撤销。',
        });
      }
    });
  }

  async function handleLogoutAll() {
    await runAction('logout-all', async () => {
      await fetchJson('/api/auth/logout-all', {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token ?? ''}`,
        },
      });

      await reloadTeamSettingsData();
      setFeedback({
        kind: 'success',
        text: '已退出全部设备。',
      });
    });
  }

  async function handleUpdateMemberRole(member: MemberResponse, role: MemberRole) {
    if (!currentTeamId || !member.id) {
      return;
    }

    const memberId = member.id;

    await runAction('update-member', async () => {
      await membersApi.updateMember({
        teamId: currentTeamId,
        memberId,
        updateMemberRequest: {
          role,
          title: member.title ?? null,
        },
      });

      await refreshWorkspaceData(currentTeamId);
      setFeedback({
        kind: 'success',
        text: `已更新成员 ${member.displayName ?? ''} 的角色。`,
      });
    });
  }

  async function handleRemoveMember(member: MemberResponse) {
    if (!currentTeamId || !member.id) {
      return;
    }

    const memberId = member.id;

    await runAction('remove-member', async () => {
      await membersApi.removeMember({
        teamId: currentTeamId,
        memberId,
      });

      await refreshWorkspaceData(currentTeamId);
      setFeedback({
        kind: 'success',
        text: `已移除成员 ${member.displayName ?? ''}。`,
      });
    });
  }

  async function handleRevokeInvitation(invitation: InvitationResponse) {
    if (!invitation.id) {
      return;
    }

    const invitationId = invitation.id;

    await runAction('revoke-invitation', async () => {
      await invitationsApi.revokeInvitation({
        invitationId,
      });

      await reloadTeamSettingsData();
      setFeedback({
        kind: 'success',
        text: `已撤销对 ${invitation.email ?? ''} 的邀请。`,
      });
    });
  }

  async function handleAcceptInvitation(invitation: InvitationResponse) {
    if (!invitation.id) {
      return;
    }

    const invitationId = invitation.id;

    await runAction('accept-invitation', async () => {
      await invitationsApi.acceptInvitation({
        invitationId,
      });

      await Promise.all([refreshTeams(), reloadTeamSettingsData()]);
      setFeedback({
        kind: 'success',
        text: `已加入团队 ${invitation.teamName ?? ''}。`,
      });
    });
  }

  return {
    conciergeCountByMemberId,
    aiMemberForm,
    aiTemplateOptions: aiRoleTemplates,
    humanMemberForm,
    invitationForm,
    myAuditLogs,
    myInvitations,
    projectsLeadCountByMemberId,
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
  };
}
