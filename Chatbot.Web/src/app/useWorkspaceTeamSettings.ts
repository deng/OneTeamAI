import { useEffect, useMemo, useState } from 'react';
import {
  type CreateAiMemberTemplateRequest,
  MemberType,
  MemberRole,
  type UpdateAiMemberTemplateRequest,
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
import type {
  AiMemberTemplateEditorForm,
  AiMemberTemplateItem,
  AuditLogItem,
  UserSessionItem,
} from './types';
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

function toAiMemberForm(template: AiMemberTemplateItem): CreateAiMemberRequest {
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

function defaultAiMemberForm(templates: ReadonlyArray<AiMemberTemplateItem> = aiRoleTemplates): CreateAiMemberRequest {
  const template = templates[0] ?? aiRoleTemplates[0];
  return template ? toAiMemberForm(template) : {};
}

function emptyAiMemberTemplateEditorForm(): AiMemberTemplateEditorForm {
  return {
    key: '',
    label: '',
    displayName: '',
    jobTitle: '',
    responsibilitySummary: '',
    title: '',
    permissionBoundary: '',
    systemPrompt: '',
    allowedTools: '',
    executableActions: '',
    knowledgeScope: '',
    isAutonomous: false,
    sortOrder: '',
    isEnabled: true,
  };
}

function toAiMemberTemplateEditorForm(template: AiMemberTemplateItem): AiMemberTemplateEditorForm {
  return {
    key: template.key ?? '',
    label: template.label ?? '',
    displayName: template.displayName ?? '',
    jobTitle: template.jobTitle ?? '',
    responsibilitySummary: template.responsibilitySummary ?? '',
    title: template.title ?? '',
    permissionBoundary: template.permissionBoundary ?? '',
    systemPrompt: template.systemPrompt ?? '',
    allowedTools: template.allowedTools ?? '',
    executableActions: template.executableActions ?? '',
    knowledgeScope: template.knowledgeScope ?? '',
    isAutonomous: template.isAutonomous ?? false,
    sortOrder: template.sortOrder != null ? String(template.sortOrder) : '',
    isEnabled: template.isEnabled !== false,
  };
}

function toDuplicatedAiMemberTemplateEditorForm(template: AiMemberTemplateItem): AiMemberTemplateEditorForm {
  const labelSuffix = template.teamId ? ' 复制' : ' 团队版';

  return {
    ...toAiMemberTemplateEditorForm(template),
    key: '',
    label: `${template.label}${labelSuffix}`.trim(),
    isEnabled: true,
    sortOrder: '',
  };
}

function toAiMemberTemplateRequestPayload(form: AiMemberTemplateEditorForm) {
  const sortOrder = Number(form.sortOrder);

  return {
    key: form.key.trim() || undefined,
    label: form.label.trim(),
    displayName: form.displayName.trim(),
    jobTitle: form.jobTitle.trim(),
    responsibilitySummary: form.responsibilitySummary.trim(),
    title: form.title.trim() || undefined,
    permissionBoundary: form.permissionBoundary.trim() || undefined,
    systemPrompt: form.systemPrompt.trim() || undefined,
    allowedTools: form.allowedTools.trim() || undefined,
    executableActions: form.executableActions.trim() || undefined,
    knowledgeScope: form.knowledgeScope.trim() || undefined,
    isAutonomous: form.isAutonomous,
    sortOrder: Number.isFinite(sortOrder) ? sortOrder : undefined,
  };
}

function toCreateAiMemberTemplateRequest(form: AiMemberTemplateEditorForm): CreateAiMemberTemplateRequest {
  return toAiMemberTemplateRequestPayload(form);
}

function toUpdateAiMemberTemplateRequest(form: AiMemberTemplateEditorForm): UpdateAiMemberTemplateRequest {
  return {
    ...toAiMemberTemplateRequestPayload(form),
    isEnabled: form.isEnabled,
  };
}

function sortAiTemplates(templates: ReadonlyArray<AiMemberTemplateItem>) {
  return [...templates].sort((left, right) => {
    const leftScope = left.teamId ? 1 : 0;
    const rightScope = right.teamId ? 1 : 0;
    if (leftScope !== rightScope) {
      return leftScope - rightScope;
    }

    const leftSort = left.sortOrder ?? 0;
    const rightSort = right.sortOrder ?? 0;
    if (leftSort !== rightSort) {
      return leftSort - rightSort;
    }

    return (left.label ?? '').localeCompare(right.label ?? '', 'zh-CN');
  });
}

function isSameAiMemberForm(left: CreateAiMemberRequest, right: CreateAiMemberRequest) {
  return left.displayName === right.displayName
    && left.jobTitle === right.jobTitle
    && left.responsibilitySummary === right.responsibilitySummary
    && left.templateKey === right.templateKey
    && left.permissionBoundary === right.permissionBoundary
    && left.title === right.title
    && left.systemPrompt === right.systemPrompt
    && left.allowedTools === right.allowedTools
    && left.executableActions === right.executableActions
    && left.knowledgeScope === right.knowledgeScope
    && left.isAutonomous === right.isAutonomous;
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
  const [aiTemplateLibrary, setAiTemplateLibrary] = useState<ReadonlyArray<AiMemberTemplateItem>>(aiRoleTemplates);
  const [selectedAiTemplateId, setSelectedAiTemplateId] = useState('');
  const [aiTemplateEditorForm, setAiTemplateEditorForm] =
    useState<AiMemberTemplateEditorForm>(emptyAiMemberTemplateEditorForm);
  const [invitationForm, setInvitationForm] = useState<CreateInvitationRequest>(emptyInvitationForm);
  const [userSessions, setUserSessions] = useState<UserSessionItem[]>([]);
  const [teamInvitations, setTeamInvitations] = useState<InvitationResponse[]>([]);
  const [myInvitations, setMyInvitations] = useState<InvitationResponse[]>([]);
  const [teamAuditLogs, setTeamAuditLogs] = useState<AuditLogItem[]>([]);
  const [myAuditLogs, setMyAuditLogs] = useState<AuditLogItem[]>([]);
  const aiTemplateOptions = useMemo(
    () => sortAiTemplates(aiTemplateLibrary.filter(template => template.isEnabled !== false)),
    [aiTemplateLibrary],
  );

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
    if (!token) {
      setAiTemplateLibrary(aiRoleTemplates);
      return;
    }

    let cancelled = false;
    const fallbackDefault = defaultAiMemberForm(aiRoleTemplates);

    void (async () => {
      const templates = await membersApi.listAiMemberTemplates({
        teamId: currentTeamId || undefined,
        includeDisabled: Boolean(currentTeamId),
      });

      if (cancelled) {
        return;
      }

      const nextTemplates = sortAiTemplates(templates.length > 0 ? templates : aiRoleTemplates);
      const enabledTemplates = nextTemplates.filter(template => template.isEnabled !== false);
      setAiTemplateLibrary(nextTemplates);
      setAiMemberForm(current =>
        isSameAiMemberForm(current, fallbackDefault) ? defaultAiMemberForm(enabledTemplates) : current,
      );
    })().catch(error => {
      if (cancelled) {
        return;
      }

      setAiTemplateLibrary(aiRoleTemplates);
      setFeedback({
        kind: 'error',
        text: error instanceof Error ? `${error.message}，已回退到内置岗位模板。` : '岗位模板加载失败，已回退到内置岗位模板。',
      });
    });

    return () => {
      cancelled = true;
    };
  }, [currentTeamId, membersApi, setFeedback, token]);

  useEffect(() => {
    if (!selectedAiTemplateId) {
      return;
    }

    const template = aiTemplateLibrary.find(item => item.id === selectedAiTemplateId);
    if (!template) {
      setSelectedAiTemplateId('');
      setAiTemplateEditorForm(emptyAiMemberTemplateEditorForm());
      return;
    }

    setAiTemplateEditorForm(toAiMemberTemplateEditorForm(template));
  }, [aiTemplateLibrary, selectedAiTemplateId]);

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
    const template = aiTemplateOptions.find(item => item.key === templateKey);
    if (!template) {
      return;
    }

    setAiMemberForm(toAiMemberForm(template));
  }

  function handleStartNewAiTemplate() {
    setSelectedAiTemplateId('');
    setAiTemplateEditorForm(emptyAiMemberTemplateEditorForm());
  }

  function handleEditAiTemplate(templateId: string) {
    const template = aiTemplateLibrary.find(item => item.id === templateId);
    if (!template) {
      return;
    }

    setSelectedAiTemplateId(templateId);
    setAiTemplateEditorForm(toAiMemberTemplateEditorForm(template));
  }

  function handleDuplicateAiTemplate(templateId: string) {
    const template = aiTemplateLibrary.find(item => item.id === templateId);
    if (!template) {
      return;
    }

    setSelectedAiTemplateId('');
    setAiTemplateEditorForm(toDuplicatedAiMemberTemplateEditorForm(template));
    setFeedback({
      kind: 'success',
      text: `已将模板 ${template.label ?? ''} 复制到编辑器，可按需调整后保存。`,
    });
  }

  function upsertAiTemplate(template: AiMemberTemplateItem) {
    setAiTemplateLibrary(current =>
      sortAiTemplates([...current.filter(item => item.id !== template.id && item.key !== template.key), template]),
    );
  }

  async function handleCreateAiTemplateTemplate() {
    if (!currentTeamId || !token) {
      return;
    }

    await runAction('create-ai-member-template', async () => {
      const created = await membersApi.createAiMemberTemplate({
        teamId: currentTeamId,
        createAiMemberTemplateRequest: toCreateAiMemberTemplateRequest(aiTemplateEditorForm),
      });

      upsertAiTemplate(created);
      setSelectedAiTemplateId(created.id ?? '');
      setAiTemplateEditorForm(toAiMemberTemplateEditorForm(created));
      setFeedback({
        kind: 'success',
        text: `已创建岗位模板 ${created.label ?? ''}。`,
      });
    });
  }

  async function handleUpdateAiTemplateTemplate() {
    if (!currentTeamId || !token || !selectedAiTemplateId) {
      return;
    }

    await runAction('update-ai-member-template', async () => {
      const updated = await membersApi.updateAiMemberTemplate({
        teamId: currentTeamId,
        templateId: selectedAiTemplateId,
        updateAiMemberTemplateRequest: toUpdateAiMemberTemplateRequest(aiTemplateEditorForm),
      });

      upsertAiTemplate(updated);
      setAiTemplateEditorForm(toAiMemberTemplateEditorForm(updated));
      setFeedback({
        kind: 'success',
        text: `已更新岗位模板 ${updated.label ?? ''}。`,
      });
    });
  }

  async function handleToggleAiTemplate(template: AiMemberTemplateItem) {
    if (!currentTeamId || !token || !template.id || template.isBuiltIn) {
      return;
    }

    const templateId = template.id;

    await runAction(template.isEnabled === false ? 'enable-ai-member-template' : 'disable-ai-member-template', async () => {
      const nextTemplate =
        template.isEnabled === false
          ? await membersApi.updateAiMemberTemplate({
              teamId: currentTeamId,
              templateId,
              updateAiMemberTemplateRequest: {
                label: template.label,
                displayName: template.displayName,
                jobTitle: template.jobTitle,
                responsibilitySummary: template.responsibilitySummary,
                title: template.title,
                permissionBoundary: template.permissionBoundary,
                systemPrompt: template.systemPrompt,
                allowedTools: template.allowedTools,
                executableActions: template.executableActions,
                knowledgeScope: template.knowledgeScope,
                isAutonomous: template.isAutonomous,
                isEnabled: true,
                sortOrder: template.sortOrder,
              },
            })
          : await membersApi.disableAiMemberTemplate({
              teamId: currentTeamId,
              templateId,
            });

      upsertAiTemplate(nextTemplate);
      if (selectedAiTemplateId === nextTemplate.id) {
        setAiTemplateEditorForm(toAiMemberTemplateEditorForm(nextTemplate));
      }
      setFeedback({
        kind: 'success',
        text: template.isEnabled === false
          ? `已启用岗位模板 ${template.label ?? ''}。`
          : `已停用岗位模板 ${template.label ?? ''}。`,
      });
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
      setAiMemberForm(defaultAiMemberForm(aiTemplateOptions));
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
    aiTemplateEditorForm,
    aiTemplateLibrary,
    aiTemplateOptions,
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
  };
}
