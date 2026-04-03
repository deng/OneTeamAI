import type { Dispatch, SetStateAction } from 'react';
import {
  InvitationStatus,
  MemberRole,
  MemberType,
  type InvitationResponse,
  type MemberResponse,
  type TeamSummaryResponse,
  type UpdateTeamRequest,
  type UserResponse,
} from '../../generated/api';
import {
  canEditMemberRole,
  canRemoveMember,
  formatAuditResult,
  formatDateTime,
  formatInvitationStatus,
  formatMemberRole,
  formatMemberType,
  formatNullableText,
  getInvitationStatusClassName,
} from '../formatters';
import type { AuditLogItem, UserSessionItem } from '../types';

type TeamSettingsPanelProps = {
  createdTeam: TeamSummaryResponse | null;
  teamSettingsForm: UpdateTeamRequest;
  onTeamSettingsFormChange: Dispatch<SetStateAction<UpdateTeamRequest>>;
  busyAction: string | null;
  onSaveTeamSettings: () => void;
  currentUser: UserResponse | null;
  myTeamsCount: number;
  userSessions: UserSessionItem[];
  onRevokeSession: (sessionId: string, isCurrent: boolean) => void;
  onLogoutAll: () => void;
  teamMembers: MemberResponse[];
  projectsLeadCountByMemberId: Record<string, number>;
  conciergeCountByMemberId: Record<string, number>;
  onUpdateMemberRole: (member: MemberResponse, role: MemberRole) => void;
  onRemoveMember: (member: MemberResponse) => void;
  teamInvitations: InvitationResponse[];
  onRevokeInvitation: (invitation: InvitationResponse) => void;
  myInvitations: InvitationResponse[];
  onAcceptInvitation: (invitation: InvitationResponse) => void;
  teamAuditLogs: AuditLogItem[];
  myAuditLogs: AuditLogItem[];
};

export function TeamSettingsPanel({
  createdTeam,
  teamSettingsForm,
  onTeamSettingsFormChange,
  busyAction,
  onSaveTeamSettings,
  currentUser,
  myTeamsCount,
  userSessions,
  onRevokeSession,
  onLogoutAll,
  teamMembers,
  projectsLeadCountByMemberId,
  conciergeCountByMemberId,
  onUpdateMemberRole,
  onRemoveMember,
  teamInvitations,
  onRevokeInvitation,
  myInvitations,
  onAcceptInvitation,
  teamAuditLogs,
  myAuditLogs,
}: TeamSettingsPanelProps) {
  return (
    <>
      <div className="panel-title panel-title-gap">团队设置</div>
      <div className="settings-grid">
        <div className="settings-section-header">
          <strong>资料与身份</strong>
          <span>围绕当前团队与当前登录用户的基本上下文。</span>
        </div>
        <div className="entity-card">
          <div className="entity-card-title">当前团队</div>
          {createdTeam ? (
            <div className="entity-card-body">
              <label className="field">
                <span>团队名称</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null}
                  value={teamSettingsForm.name ?? ''}
                  onChange={event =>
                    onTeamSettingsFormChange(current => ({ ...current, name: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="field">
                <span>团队说明</span>
                <textarea
                  className="text-area"
                  rows={3}
                  disabled={busyAction !== null}
                  value={teamSettingsForm.description ?? ''}
                  onChange={event =>
                    onTeamSettingsFormChange(current => ({
                      ...current,
                      description: event.currentTarget.value,
                    }))
                  }
                />
              </label>
              <label className="field">
                <span>品牌名</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null}
                  value={teamSettingsForm.brandName ?? ''}
                  onChange={event =>
                    onTeamSettingsFormChange(current => ({
                      ...current,
                      brandName: event.currentTarget.value,
                    }))
                  }
                />
              </label>
              <span>我的角色：{formatMemberRole(createdTeam.currentMemberRole)}</span>
              <button
                className="secondary-button"
                disabled={busyAction !== null || !createdTeam.id}
                type="button"
                onClick={onSaveTeamSettings}
              >
                {busyAction === 'update-team' ? '保存中...' : '保存团队设置'}
              </button>
            </div>
          ) : (
            <span className="entity-placeholder">尚未选择团队</span>
          )}
        </div>
        <div className="entity-card">
          <div className="entity-card-title">当前用户</div>
          {currentUser ? (
            <div className="entity-card-body">
              <strong>{currentUser.displayName}</strong>
              <span>{currentUser.email}</span>
              <span>可访问团队：{myTeamsCount}</span>
              <span>活跃会话：{userSessions.filter(session => !session.revokedAt).length}</span>
              {userSessions.length > 0 ? (
                <div className="entity-chip-list">
                  {userSessions.slice(0, 4).map(session => (
                    <div className="entity-chip" key={session.id}>
                      <strong>{session.isCurrent ? '当前设备' : '其他会话'}</strong>
                      <span>
                        {session.revokedAt ? '已退出' : '活跃'} · 最近：{formatDateTime(session.lastSeenAt)}
                      </span>
                      <span>过期：{formatDateTime(session.expiresAt)}</span>
                      <span>{formatNullableText(session.ipAddress, '未记录 IP')}</span>
                      <span>{formatNullableText(session.userAgent, '未记录设备')}</span>
                      {!session.revokedAt ? (
                        <button
                          className="secondary-button"
                          disabled={busyAction !== null}
                          type="button"
                          onClick={() => onRevokeSession(session.id, session.isCurrent)}
                        >
                          {busyAction === 'revoke-session' ? '处理中...' : '撤销会话'}
                        </button>
                      ) : null}
                    </div>
                  ))}
                </div>
              ) : null}
              <button
                className="secondary-button"
                disabled={busyAction !== null || userSessions.length === 0}
                type="button"
                onClick={onLogoutAll}
              >
                {busyAction === 'logout-all' ? '处理中...' : '退出全部设备'}
              </button>
            </div>
          ) : (
            <span className="entity-placeholder">尚未登录</span>
          )}
        </div>
        <div className="settings-section-header">
          <strong>成员与权限</strong>
          <span>真人成员和 AI 员工统一放在一个团队编制里管理。</span>
        </div>
        <div className="entity-card">
          <div className="entity-card-title">成员</div>
          {teamMembers.length > 0 ? (
            <div className="entity-chip-list">
              {teamMembers.map(member => (
                <div className="entity-chip" key={member.id}>
                  <strong>{member.displayName}</strong>
                  <span>
                    {formatMemberType(member.memberType)} · {formatMemberRole(member.role)}
                  </span>
                  <span>{formatNullableText(member.aiProfile?.jobTitle ?? member.title, '尚未设置岗位或头衔')}</span>
                  {member.aiProfile?.templateKey ? <span>模板：{member.aiProfile.templateKey}</span> : null}
                  {member.aiProfile?.responsibilitySummary ? (
                    <span>{member.aiProfile.responsibilitySummary}</span>
                  ) : null}
                  {member.aiProfile?.permissionBoundary ? <span>边界：{member.aiProfile.permissionBoundary}</span> : null}
                  {member.aiProfile?.executableActions ? <span>动作：{member.aiProfile.executableActions}</span> : null}
                  {member.aiProfile?.allowedTools ? <span>工具：{member.aiProfile.allowedTools}</span> : null}
                  {member.aiProfile?.knowledgeScope ? <span>知识域：{member.aiProfile.knowledgeScope}</span> : null}
                  {member.memberType === MemberType.NUMBER_1 ? (
                    <span>
                      绑定项目：{projectsLeadCountByMemberId[member.id ?? ''] ?? 0}
                      {' '}· 负责坐台程序：{conciergeCountByMemberId[member.id ?? ''] ?? 0}
                    </span>
                  ) : null}
                  {canEditMemberRole(member) && createdTeam?.id && member.id ? (
                    <select
                      className="text-input"
                      disabled={busyAction !== null}
                      value={member.role ?? MemberRole.NUMBER_2}
                      onChange={event =>
                        onUpdateMemberRole(member, Number(event.currentTarget.value) as MemberRole)
                      }
                    >
                      <option value={MemberRole.NUMBER_1}>管理员</option>
                      <option value={MemberRole.NUMBER_2}>执行成员</option>
                      <option value={MemberRole.NUMBER_3}>观察者</option>
                    </select>
                  ) : null}
                  {canRemoveMember(member) && createdTeam?.id && member.id ? (
                    <button
                      className="secondary-button"
                      disabled={busyAction !== null}
                      type="button"
                      onClick={() => onRemoveMember(member)}
                    >
                      {busyAction === 'remove-member' ? '处理中...' : '移除'}
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <span className="entity-placeholder">暂无成员</span>
          )}
        </div>
        <div className="settings-section-header">
          <strong>邀请管理</strong>
          <span>处理待接受、已接受、已撤销和已过期的团队邀请。</span>
        </div>
        <div className="entity-card">
          <div className="entity-card-title">团队邀请</div>
          {teamInvitations.length > 0 ? (
            <div className="entity-chip-list">
              {teamInvitations.map(invitation => (
                <div className="entity-chip" key={invitation.id}>
                  <strong>{invitation.email}</strong>
                  <span>{formatMemberRole(invitation.role)} / {formatNullableText(invitation.title, '未设置头衔')}</span>
                  <span className={getInvitationStatusClassName(invitation.status)}>
                    {formatInvitationStatus(invitation.status)}
                  </span>
                  <span>
                    邀请人：{invitation.invitedByDisplayName ?? '未知'} · 发起：{formatDateTime(invitation.createdAt)}
                  </span>
                  <span>
                    到期：{formatDateTime(invitation.expiresAt)} · 响应：{formatDateTime(invitation.respondedAt)}
                  </span>
                  {invitation.id && invitation.status === InvitationStatus.NUMBER_0 ? (
                    <button
                      className="secondary-button"
                      disabled={busyAction !== null}
                      type="button"
                      onClick={() => onRevokeInvitation(invitation)}
                    >
                      {busyAction === 'revoke-invitation' ? '处理中...' : '撤销'}
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <span className="entity-placeholder">暂无邀请</span>
          )}
        </div>
        <div className="entity-card">
          <div className="entity-card-title">我的邀请</div>
          {myInvitations.length > 0 ? (
            <div className="entity-chip-list">
              {myInvitations.map(invitation => (
                <div className="entity-chip" key={invitation.id}>
                  <strong>{invitation.teamName ?? '未命名团队'}</strong>
                  <span>{formatMemberRole(invitation.role)} / {formatNullableText(invitation.title, '未设置头衔')}</span>
                  <span className={getInvitationStatusClassName(invitation.status)}>
                    {formatInvitationStatus(invitation.status)}
                  </span>
                  <span>
                    邀请人：{invitation.invitedByDisplayName ?? '未知'} · 发起：{formatDateTime(invitation.createdAt)}
                  </span>
                  <span>
                    到期：{formatDateTime(invitation.expiresAt)} · 响应：{formatDateTime(invitation.respondedAt)}
                  </span>
                  {invitation.id ? (
                    <button
                      className="secondary-button"
                      disabled={busyAction !== null}
                      type="button"
                      onClick={() => onAcceptInvitation(invitation)}
                    >
                      {busyAction === 'accept-invitation' ? '处理中...' : '接受'}
                    </button>
                  ) : null}
                </div>
              ))}
            </div>
          ) : (
            <span className="entity-placeholder">暂无待处理邀请</span>
          )}
        </div>
        <div className="settings-section-header">
          <strong>审计与操作</strong>
          <span>帮助你回看团队最近做了什么，以及当前登录用户的关键操作记录。</span>
        </div>
        <div className="entity-card">
          <div className="entity-card-title">团队审计日志</div>
          {teamAuditLogs.length > 0 ? (
            <div className="entity-chip-list">
              {teamAuditLogs.map(log => (
                <div className="entity-chip" key={log.id}>
                  <strong>{log.actionType}</strong>
                  <span>
                    {formatAuditResult(log.result)} · {formatNullableText(log.userDisplayName, '未知操作者')}
                  </span>
                  <span>{log.summary}</span>
                  <span>
                    实体：{log.entityType} · 时间：{formatDateTime(log.createdAt)}
                  </span>
                  <span>来源 IP：{formatNullableText(log.ipAddress, '未记录')}</span>
                </div>
              ))}
            </div>
          ) : (
            <span className="entity-placeholder">当前团队还没有审计日志。</span>
          )}
        </div>
        <div className="entity-card">
          <div className="entity-card-title">我的最近操作</div>
          {myAuditLogs.length > 0 ? (
            <div className="entity-chip-list">
              {myAuditLogs.map(log => (
                <div className="entity-chip" key={log.id}>
                  <strong>{log.actionType}</strong>
                  <span>
                    {formatAuditResult(log.result)} · {formatDateTime(log.createdAt)}
                  </span>
                  <span>{log.summary}</span>
                  <span>
                    实体：{log.entityType} · 操作人：{formatNullableText(log.userDisplayName, '当前用户')}
                  </span>
                </div>
              ))}
            </div>
          ) : (
            <span className="entity-placeholder">当前用户还没有最近操作记录。</span>
          )}
        </div>
      </div>
    </>
  );
}
