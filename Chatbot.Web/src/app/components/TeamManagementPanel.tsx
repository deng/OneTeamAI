import type { Dispatch, SetStateAction } from 'react';
import {
  MemberRole,
  type CreateAiMemberRequest,
  type CreateHumanMemberRequest,
  type CreateInvitationRequest,
} from '../../generated/api';

type AiTemplateOption = {
  key: string;
  label: string;
} & CreateAiMemberRequest;

type TeamManagementPanelProps = {
  busyAction: string | null;
  canManage: boolean;
  humanMemberForm: CreateHumanMemberRequest;
  aiMemberForm: CreateAiMemberRequest;
  invitationForm: CreateInvitationRequest;
  aiTemplateOptions: ReadonlyArray<AiTemplateOption>;
  onHumanMemberFormChange: Dispatch<SetStateAction<CreateHumanMemberRequest>>;
  onAiMemberFormChange: Dispatch<SetStateAction<CreateAiMemberRequest>>;
  onInvitationFormChange: Dispatch<SetStateAction<CreateInvitationRequest>>;
  onApplyAiTemplate: (templateKey: string) => void;
  onCreateHumanMember: () => void;
  onCreateAiMember: () => void;
  onCreateInvitation: () => void;
};

export function TeamManagementPanel({
  busyAction,
  canManage,
  humanMemberForm,
  aiMemberForm,
  invitationForm,
  aiTemplateOptions,
  onHumanMemberFormChange,
  onAiMemberFormChange,
  onInvitationFormChange,
  onApplyAiTemplate,
  onCreateHumanMember,
  onCreateAiMember,
  onCreateInvitation,
}: TeamManagementPanelProps) {
  return (
    <>
      <div className="panel-title panel-title-gap">编制扩展</div>
      <div className="settings-grid">
        <div className="settings-section-header">
          <strong>新增成员</strong>
          <span>把真人成员、AI 员工和待确认邀请接回当前团队工作流。</span>
        </div>

        <div className="entity-card">
          <div className="entity-card-title">添加真人成员</div>
          <div className="entity-card-body">
            <label className="field">
              <span>成员邮箱</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={humanMemberForm.email ?? ''}
                onChange={event =>
                  onHumanMemberFormChange(current => ({ ...current, email: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>角色</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={humanMemberForm.role ?? MemberRole.NUMBER_2}
                onChange={event =>
                  onHumanMemberFormChange(current => ({
                    ...current,
                    role: Number(event.currentTarget.value) as MemberRole,
                  }))
                }
              >
                <option value={MemberRole.NUMBER_1}>管理员</option>
                <option value={MemberRole.NUMBER_2}>执行成员</option>
                <option value={MemberRole.NUMBER_3}>观察者</option>
              </select>
            </label>
            <label className="field">
              <span>头衔</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={humanMemberForm.title ?? ''}
                onChange={event =>
                  onHumanMemberFormChange(current => ({ ...current, title: event.currentTarget.value }))
                }
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManage}
              type="button"
              onClick={onCreateHumanMember}
            >
              {busyAction === 'create-human-member' ? '添加中...' : '添加真人成员'}
            </button>
          </div>
        </div>

        <div className="entity-card">
          <div className="entity-card-title">创建 AI 员工</div>
          <div className="entity-card-body">
            <label className="field">
              <span>岗位模板</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={aiMemberForm.templateKey ?? ''}
                onChange={event => onApplyAiTemplate(event.currentTarget.value)}
              >
                <option value="">手动配置</option>
                {aiTemplateOptions.map(template => (
                  <option key={template.key} value={template.key}>
                    {template.label}
                  </option>
                ))}
              </select>
            </label>
            <label className="field">
              <span>显示名称</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={aiMemberForm.displayName ?? ''}
                onChange={event =>
                  onAiMemberFormChange(current => ({ ...current, displayName: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>岗位</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={aiMemberForm.jobTitle ?? ''}
                onChange={event =>
                  onAiMemberFormChange(current => ({ ...current, jobTitle: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>职责摘要</span>
              <textarea
                className="text-area"
                rows={3}
                disabled={busyAction !== null}
                value={aiMemberForm.responsibilitySummary ?? ''}
                onChange={event =>
                  onAiMemberFormChange(current => ({
                    ...current,
                    responsibilitySummary: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>权限边界</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={aiMemberForm.permissionBoundary ?? ''}
                onChange={event =>
                  onAiMemberFormChange(current => ({
                    ...current,
                    permissionBoundary: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>可执行动作</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={aiMemberForm.executableActions ?? ''}
                onChange={event =>
                  onAiMemberFormChange(current => ({
                    ...current,
                    executableActions: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>允许工具</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={aiMemberForm.allowedTools ?? ''}
                onChange={event =>
                  onAiMemberFormChange(current => ({ ...current, allowedTools: event.currentTarget.value }))
                }
              />
            </label>
            <label className="checkbox-field">
              <input
                checked={aiMemberForm.isAutonomous ?? false}
                type="checkbox"
                onChange={event =>
                  onAiMemberFormChange(current => ({ ...current, isAutonomous: event.currentTarget.checked }))
                }
              />
              允许该 AI 员工自主推进部分任务
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManage}
              type="button"
              onClick={onCreateAiMember}
            >
              {busyAction === 'create-ai-member' ? '创建中...' : '创建 AI 员工'}
            </button>
          </div>
        </div>

        <div className="entity-card">
          <div className="entity-card-title">发起邀请</div>
          <div className="entity-card-body">
            <label className="field">
              <span>邀请邮箱</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={invitationForm.email ?? ''}
                onChange={event =>
                  onInvitationFormChange(current => ({ ...current, email: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>邀请角色</span>
              <select
                className="text-input"
                disabled={busyAction !== null}
                value={invitationForm.role ?? MemberRole.NUMBER_2}
                onChange={event =>
                  onInvitationFormChange(current => ({
                    ...current,
                    role: Number(event.currentTarget.value) as MemberRole,
                  }))
                }
              >
                <option value={MemberRole.NUMBER_1}>管理员</option>
                <option value={MemberRole.NUMBER_2}>执行成员</option>
                <option value={MemberRole.NUMBER_3}>观察者</option>
              </select>
            </label>
            <label className="field">
              <span>头衔</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                value={invitationForm.title ?? ''}
                onChange={event =>
                  onInvitationFormChange(current => ({ ...current, title: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>有效期（天）</span>
              <input
                className="text-input"
                disabled={busyAction !== null}
                min={1}
                type="number"
                value={invitationForm.expiresInDays ?? 7}
                onChange={event =>
                  onInvitationFormChange(current => ({
                    ...current,
                    expiresInDays: Number(event.currentTarget.value) || 7,
                  }))
                }
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canManage}
              type="button"
              onClick={onCreateInvitation}
            >
              {busyAction === 'create-invitation' ? '发送中...' : '发送邀请'}
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
