import { useState, type Dispatch, SetStateAction } from 'react';
import {
  MemberRole,
  type CreateAiMemberRequest,
  type CreateHumanMemberRequest,
  type CreateInvitationRequest,
} from '../../generated/api';
import type { AiMemberTemplateEditorForm, AiMemberTemplateItem } from '../types';
import { Modal } from './Modal';

type TeamManagementPanelProps = {
  busyAction: string | null;
  canManage: boolean;
  humanMemberForm: CreateHumanMemberRequest;
  aiMemberForm: CreateAiMemberRequest;
  aiTemplateEditorForm: AiMemberTemplateEditorForm;
  aiTemplateLibrary: ReadonlyArray<AiMemberTemplateItem>;
  invitationForm: CreateInvitationRequest;
  aiTemplateOptions: ReadonlyArray<AiMemberTemplateItem>;
  selectedAiTemplateId: string;
  onHumanMemberFormChange: Dispatch<SetStateAction<CreateHumanMemberRequest>>;
  onAiMemberFormChange: Dispatch<SetStateAction<CreateAiMemberRequest>>;
  onAiTemplateEditorFormChange: Dispatch<SetStateAction<AiMemberTemplateEditorForm>>;
  onInvitationFormChange: Dispatch<SetStateAction<CreateInvitationRequest>>;
  onApplyAiTemplate: (templateKey: string) => void;
  onStartNewAiTemplate: () => void;
  onDuplicateAiTemplate: (templateId: string) => void;
  onEditAiTemplate: (templateId: string) => void;
  onCreateAiTemplateTemplate: () => void;
  onUpdateAiTemplateTemplate: () => void;
  onToggleAiTemplate: (template: AiMemberTemplateItem) => void;
  onCreateHumanMember: () => void;
  onCreateAiMember: () => void;
  onCreateInvitation: () => void;
};

export function TeamManagementPanel({
  busyAction,
  canManage,
  humanMemberForm,
  aiMemberForm,
  aiTemplateEditorForm,
  aiTemplateLibrary,
  invitationForm,
  aiTemplateOptions,
  selectedAiTemplateId,
  onHumanMemberFormChange,
  onAiMemberFormChange,
  onAiTemplateEditorFormChange,
  onInvitationFormChange,
  onApplyAiTemplate,
  onStartNewAiTemplate,
  onDuplicateAiTemplate,
  onEditAiTemplate,
  onCreateAiTemplateTemplate,
  onUpdateAiTemplateTemplate,
  onToggleAiTemplate,
  onCreateHumanMember,
  onCreateAiMember,
  onCreateInvitation,
}: TeamManagementPanelProps) {
  const selectedAiTemplate =
    aiTemplateLibrary.find(template => template.id === selectedAiTemplateId) ?? null;
  const isEditingBuiltInTemplate = selectedAiTemplate?.isBuiltIn === true;
  const isCreatingTemplate = busyAction === 'create-ai-member-template';
  const isUpdatingTemplate = busyAction === 'update-ai-member-template';
  const isTogglingTemplate =
    busyAction === 'enable-ai-member-template' || busyAction === 'disable-ai-member-template';
  const [showHumanMemberForm, setShowHumanMemberForm] = useState(false);
  const [showAiMemberForm, setShowAiMemberForm] = useState(false);
  const [showInvitationForm, setShowInvitationForm] = useState(false);

  return (
    <>
      <div className="panel-title panel-title-gap">编制扩展</div>
      <div className="settings-grid">
        <div className="settings-section-header">
          <strong>新增成员</strong>
          <span>把真人成员、AI 员工和待确认邀请接回当前团队工作流。</span>
        </div>

        {canManage ? (
          <button className="secondary-button" type="button" onClick={() => setShowHumanMemberForm(true)}>
            + 添加真人成员
          </button>
        ) : null}

        <Modal open={showHumanMemberForm} onClose={() => setShowHumanMemberForm(false)} title="添加真人成员">
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
            onClick={() => { onCreateHumanMember(); setShowHumanMemberForm(false); }}
          >
            {busyAction === 'create-human-member' ? '添加中...' : '添加真人成员'}
          </button>
        </Modal>

        {canManage ? (
          <button className="secondary-button" type="button" onClick={() => setShowAiMemberForm(true)}>
            + 创建 AI 员工
          </button>
        ) : null}

        <Modal open={showAiMemberForm} onClose={() => setShowAiMemberForm(false)} title="创建 AI 员工">
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
                <option key={template.id ?? template.key ?? template.label ?? 'template-option'} value={template.key ?? ''}>
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
            onClick={() => { onCreateAiMember(); setShowAiMemberForm(false); }}
          >
            {busyAction === 'create-ai-member' ? '创建中...' : '创建 AI 员工'}
          </button>
        </Modal>

        <div className="entity-card">
          <div className="entity-card-title">岗位模板库</div>
          <div className="entity-card-body">
            <strong>团队岗位模板</strong>
            <span>系统内置模板只读；团队模板可新增、编辑、停用或重新启用。</span>
            <div className="template-library-list">
              {aiTemplateLibrary.map(template => {
                const isSelected = template.id === selectedAiTemplateId;
                const sourceLabel = template.teamId ? '团队模板' : '系统内置';
                const enabledLabel = template.isEnabled === false ? '已停用' : '启用中';

                return (
                  <div
                    className={`template-library-item${isSelected ? ' template-library-item-selected' : ''}`}
                    key={template.id ?? template.key}
                  >
                    <div className="template-library-row">
                      <strong>{template.label}</strong>
                      <div className="template-library-badges">
                        <span className="template-library-badge">{sourceLabel}</span>
                        <span className="template-library-badge">{enabledLabel}</span>
                      </div>
                    </div>
                    <span>{template.jobTitle ?? '未设置岗位'}</span>
                    <span>{template.responsibilitySummary ?? '未设置职责摘要'}</span>
                    <div className="template-library-actions">
                      <button
                        className="template-library-button"
                        disabled={busyAction !== null || template.isEnabled === false || !template.key}
                        type="button"
                        onClick={() => {
                          if (!template.key) {
                            return;
                          }

                          onApplyAiTemplate(template.key);
                        }}
                      >
                        套用到 AI 员工
                      </button>
                      <button
                        className="template-library-button"
                        disabled={busyAction !== null || !canManage}
                        type="button"
                        onClick={() => {
                          if (!template.id) {
                            return;
                          }

                          onDuplicateAiTemplate(template.id);
                        }}
                      >
                        复制为团队模板
                      </button>
                      <button
                        className="template-library-button"
                        disabled={busyAction !== null}
                        type="button"
                        onClick={() => {
                          if (!template.id) {
                            return;
                          }

                          onEditAiTemplate(template.id);
                        }}
                      >
                        {isSelected ? '正在编辑' : '编辑模板'}
                      </button>
                      {template.teamId && template.id ? (
                        <button
                          className="template-library-button"
                          disabled={busyAction !== null}
                          type="button"
                          onClick={() => onToggleAiTemplate(template)}
                        >
                          {template.isEnabled === false ? '重新启用' : '停用模板'}
                        </button>
                      ) : null}
                    </div>
                  </div>
                );
              })}
            </div>

            <div className="template-editor-header">
              <strong>{selectedAiTemplate ? `编辑模板：${selectedAiTemplate.label}` : '新建团队模板'}</strong>
              <div className="template-library-actions">
                <button
                  className="template-library-button"
                  disabled={busyAction !== null}
                  type="button"
                  onClick={onStartNewAiTemplate}
                >
                  新建模板
                </button>
              </div>
            </div>

            {isEditingBuiltInTemplate ? (
              <span>当前选中的是系统内置模板，只能查看和套用，不能直接修改。</span>
            ) : (
              <span>团队模板会优先出现在“创建 AI 员工”的岗位模板列表中。</span>
            )}

            <label className="field">
              <span>模板名称</span>
              <input
                className="text-input"
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                value={aiTemplateEditorForm.label}
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({ ...current, label: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>显示名称</span>
              <input
                className="text-input"
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                value={aiTemplateEditorForm.displayName}
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({
                    ...current,
                    displayName: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>岗位</span>
              <input
                className="text-input"
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                value={aiTemplateEditorForm.jobTitle}
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({ ...current, jobTitle: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>职责摘要</span>
              <textarea
                className="text-area"
                rows={3}
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                value={aiTemplateEditorForm.responsibilitySummary}
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({
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
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                value={aiTemplateEditorForm.permissionBoundary}
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({
                    ...current,
                    permissionBoundary: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>允许工具</span>
              <input
                className="text-input"
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                value={aiTemplateEditorForm.allowedTools}
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({
                    ...current,
                    allowedTools: event.currentTarget.value,
                  }))
                }
              />
            </label>
            <label className="field">
              <span>可执行动作</span>
              <input
                className="text-input"
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                value={aiTemplateEditorForm.executableActions}
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({
                    ...current,
                    executableActions: event.currentTarget.value,
                  }))
                }
              />
            </label>

            <details className="template-library-details">
              <summary>高级配置</summary>
              <label className="field">
                <span>模板 Key</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null || Boolean(selectedAiTemplate) || isEditingBuiltInTemplate}
                  placeholder="留空则自动生成"
                  value={aiTemplateEditorForm.key}
                  onChange={event =>
                    onAiTemplateEditorFormChange(current => ({ ...current, key: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="field">
                <span>头衔</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null || isEditingBuiltInTemplate}
                  value={aiTemplateEditorForm.title}
                  onChange={event =>
                    onAiTemplateEditorFormChange(current => ({ ...current, title: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="field">
                <span>知识域</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null || isEditingBuiltInTemplate}
                  value={aiTemplateEditorForm.knowledgeScope}
                  onChange={event =>
                    onAiTemplateEditorFormChange(current => ({
                      ...current,
                      knowledgeScope: event.currentTarget.value,
                    }))
                  }
                />
              </label>
              <label className="field">
                <span>系统提示词</span>
                <textarea
                  className="text-area"
                  rows={4}
                  disabled={busyAction !== null || isEditingBuiltInTemplate}
                  value={aiTemplateEditorForm.systemPrompt}
                  onChange={event =>
                    onAiTemplateEditorFormChange(current => ({
                      ...current,
                      systemPrompt: event.currentTarget.value,
                    }))
                  }
                />
              </label>
              <label className="field">
                <span>排序</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null || isEditingBuiltInTemplate}
                  type="number"
                  value={aiTemplateEditorForm.sortOrder}
                  onChange={event =>
                    onAiTemplateEditorFormChange(current => ({
                      ...current,
                      sortOrder: event.currentTarget.value,
                    }))
                  }
                />
              </label>
            </details>

            <label className="checkbox-field">
              <input
                checked={aiTemplateEditorForm.isAutonomous}
                disabled={busyAction !== null || isEditingBuiltInTemplate}
                type="checkbox"
                onChange={event =>
                  onAiTemplateEditorFormChange(current => ({
                    ...current,
                    isAutonomous: event.currentTarget.checked,
                  }))
                }
              />
              允许基于该模板创建可自主推进的 AI 员工
            </label>

            {selectedAiTemplate ? (
              <label className="checkbox-field">
                <input
                  checked={aiTemplateEditorForm.isEnabled}
                  disabled={busyAction !== null || isEditingBuiltInTemplate}
                  type="checkbox"
                  onChange={event =>
                    onAiTemplateEditorFormChange(current => ({
                      ...current,
                      isEnabled: event.currentTarget.checked,
                    }))
                  }
                />
                模板保持启用
              </label>
            ) : null}

            <div className="template-library-actions">
              {selectedAiTemplate && !isEditingBuiltInTemplate ? (
                <>
                  <button
                    className="primary-button"
                    disabled={busyAction !== null || !canManage}
                    type="button"
                    onClick={onUpdateAiTemplateTemplate}
                  >
                    {isUpdatingTemplate ? '保存中...' : '保存模板'}
                  </button>
                  <button
                    className="secondary-button"
                    disabled={busyAction !== null || !canManage}
                    type="button"
                    onClick={() => onToggleAiTemplate(selectedAiTemplate)}
                  >
                    {isTogglingTemplate
                      ? '处理中...'
                      : (selectedAiTemplate.isEnabled === false ? '重新启用模板' : '停用模板')}
                  </button>
                </>
              ) : null}
              {!selectedAiTemplate ? (
                <button
                  className="primary-button"
                  disabled={busyAction !== null || !canManage}
                  type="button"
                  onClick={onCreateAiTemplateTemplate}
                >
                  {isCreatingTemplate ? '创建中...' : '创建模板'}
                </button>
              ) : null}
            </div>
          </div>
        </div>

        {canManage ? (
          <button className="secondary-button" type="button" onClick={() => setShowInvitationForm(true)}>
            + 发起邀请
          </button>
        ) : null}

        <Modal open={showInvitationForm} onClose={() => setShowInvitationForm(false)} title="发起邀请">
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
            onClick={() => { onCreateInvitation(); setShowInvitationForm(false); }}
          >
            {busyAction === 'create-invitation' ? '发送中...' : '发送邀请'}
          </button>
        </Modal>
      </div>
    </>
  );
}
