import type {
  AgentWorkflowResponse,
  MemberResponse,
  RunTicketWorkflowRequest,
} from '../../generated/api';
import {
  formatDateTime,
  formatExecutionLogStatus,
  formatNullableText,
  formatWorkflowStatus,
  formatWorkflowStepStatus,
} from '../formatters';
import type { WorkflowTemplateItem } from '../types';

type WorkflowScope = 'ticket' | 'conversation' | 'project' | null;

type WorkflowPanelProps = {
  currentWorkflowScope: WorkflowScope;
  currentWorkflowScopeLabel: string;
  currentScopeWorkflowTemplates: WorkflowTemplateItem[];
  workflowForm: RunTicketWorkflowRequest;
  onWorkflowFormChange: (updater: (current: RunTicketWorkflowRequest) => RunTicketWorkflowRequest) => void;
  aiMembers: MemberResponse[];
  autonomousAiMembers: MemberResponse[];
  busyAction: string | null;
  canRunWorkflow: boolean;
  onRunWorkflow: () => void;
  ticketWorkflows: AgentWorkflowResponse[];
  selectedWorkflowId: string;
  onSelectWorkflow: (workflowId: string) => void;
  selectedWorkflow: AgentWorkflowResponse | null;
  onSelectRelatedProjectId: (projectId: string) => void;
  onSelectRelatedConversationId: (conversationId: string) => void;
  onSelectRelatedTicketId: (ticketId: string) => void;
};

export function WorkflowPanel({
  currentWorkflowScope,
  currentWorkflowScopeLabel,
  currentScopeWorkflowTemplates,
  workflowForm,
  onWorkflowFormChange,
  aiMembers,
  autonomousAiMembers,
  busyAction,
  canRunWorkflow,
  onRunWorkflow,
  ticketWorkflows,
  selectedWorkflowId,
  onSelectWorkflow,
  selectedWorkflow,
  onSelectRelatedProjectId,
  onSelectRelatedConversationId,
  onSelectRelatedTicketId,
}: WorkflowPanelProps) {
  return (
    <>
      <div className="entity-card">
        <div className="entity-card-title">多智能体协作</div>
        {currentWorkflowScope ? (
          <div className="entity-card-body">
            {currentScopeWorkflowTemplates.length > 0 ? (
              <div className="entity-chip-list">
                {currentScopeWorkflowTemplates.map(template => (
                  <button
                    className="entity-chip entity-chip-button"
                    key={template.key}
                    type="button"
                    onClick={() =>
                      onWorkflowFormChange(current => ({
                        ...current,
                        goal: template.goal,
                      }))
                    }
                  >
                    <strong>{template.label}</strong>
                    <span>{template.summary}</span>
                  </button>
                ))}
              </div>
            ) : null}
            <label className="field">
              <span>协作目标</span>
              <textarea
                className="text-area"
                rows={3}
                value={workflowForm.goal ?? ''}
                onChange={event =>
                  onWorkflowFormChange(current => ({ ...current, goal: event.currentTarget.value }))
                }
              />
            </label>
            <label className="field">
              <span>起始 AI 员工</span>
              <select
                className="text-input"
                disabled={autonomousAiMembers.length === 0 && aiMembers.length === 0}
                value={workflowForm.startedByMemberId ?? ''}
                onChange={event =>
                  onWorkflowFormChange(current => ({
                    ...current,
                    startedByMemberId: event.currentTarget.value || undefined,
                  }))
                }
              >
                <option value="">自动选择</option>
                {aiMembers.map(member => (
                  <option key={member.id} value={member.id}>
                    {member.displayName} · {formatNullableText(member.aiProfile?.jobTitle, '未设置岗位')}
                  </option>
                ))}
              </select>
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null || !canRunWorkflow}
              type="button"
              onClick={onRunWorkflow}
            >
              {busyAction === `run-${currentWorkflowScope}-workflow` ? '运行中...' : '运行协作链'}
            </button>
            <span>
              当前上下文：{currentWorkflowScopeLabel}。系统会按岗位把 AI 员工串成协作链，并记录每一步日志。
            </span>
          </div>
        ) : (
          <span className="entity-placeholder">先选择项目、会话或工单中的任意一个，再发起多智能体协作。</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">协作运行记录</div>
        <div className="entity-card-body">
          <span>
            当前查看：{currentWorkflowScopeLabel} · 范围：
            {currentWorkflowScope === 'project'
              ? '项目'
              : currentWorkflowScope === 'conversation'
                ? '会话'
                : currentWorkflowScope === 'ticket'
                  ? '工单'
                  : '未选择'}
          </span>
        </div>
        {ticketWorkflows.length > 0 ? (
          <div className="entity-chip-list">
            {ticketWorkflows.map(workflow => (
              <button
                className={
                  workflow.id === selectedWorkflowId
                    ? 'entity-chip entity-chip-button entity-chip-selected'
                    : 'entity-chip entity-chip-button'
                }
                key={workflow.id}
                type="button"
                onClick={() => onSelectWorkflow(workflow.id ?? '')}
              >
                <strong>{formatNullableText(workflow.summary, '未生成摘要')}</strong>
                <span>{formatWorkflowStatus(workflow.status)} · {workflow.steps?.length ?? 0} 步</span>
                <span>类型：{formatNullableText(workflow.workflowType, '默认协作链')}</span>
                <span>发起 AI：{formatNullableText(workflow.startedByMemberName, '自动选择')}</span>
                <span>完成于：{formatDateTime(workflow.completedAt ?? workflow.createdAt)}</span>
              </button>
            ))}
          </div>
        ) : (
          <span className="entity-placeholder">当前上下文还没有多智能体协作记录。</span>
        )}
      </div>

      <div className="entity-card">
        <div className="entity-card-title">协作步骤详情</div>
        {selectedWorkflow ? (
          <div className="entity-card-body">
            <strong>{formatNullableText(selectedWorkflow.goal, '未设置目标')}</strong>
            <span>
              {formatWorkflowStatus(selectedWorkflow.status)} · 创建于：
              {formatDateTime(selectedWorkflow.createdAt)}
            </span>
            <span>类型：{formatNullableText(selectedWorkflow.workflowType, '默认协作链')}</span>
            <div className="entity-chip-list">
              {selectedWorkflow.projectId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedProjectId(selectedWorkflow.projectId ?? '')}
                >
                  <strong>关联项目</strong>
                  <span>{selectedWorkflow.projectId}</span>
                </button>
              ) : null}
              {selectedWorkflow.conversationId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedConversationId(selectedWorkflow.conversationId ?? '')}
                >
                  <strong>关联会话</strong>
                  <span>{selectedWorkflow.conversationId}</span>
                </button>
              ) : null}
              {selectedWorkflow.ticketId ? (
                <button
                  className="entity-chip entity-chip-button"
                  type="button"
                  onClick={() => onSelectRelatedTicketId(selectedWorkflow.ticketId ?? '')}
                >
                  <strong>关联工单</strong>
                  <span>{selectedWorkflow.ticketId}</span>
                </button>
              ) : null}
            </div>
            {!selectedWorkflow.projectId && !selectedWorkflow.conversationId && !selectedWorkflow.ticketId ? (
              <span>关联上下文：未关联</span>
            ) : null}
            <div className="detail-timeline">
              {(selectedWorkflow.steps ?? []).map(step => (
                <div className="detail-message" key={step.id}>
                  <div className="detail-message-meta">
                    <strong>
                      {step.sequence}. {formatNullableText(step.memberName, '系统')}
                    </strong>
                    <span>
                      {formatWorkflowStepStatus(step.status)} · {formatNullableText(step.actionType, '未设置动作')}
                    </span>
                  </div>
                  <div className="detail-message-body">
                    岗位：{formatNullableText(step.memberTitle, '未设置')}
                    {'\n'}
                    执行时间：{formatDateTime(step.executedAt)}
                  </div>
                  <div className="detail-message-body">
                    输入：{formatNullableText(step.inputSummary, '无输入摘要')}
                    {'\n'}
                    输出：{formatNullableText(step.outputSummary, '无输出摘要')}
                    {'\n'}
                    交接：{formatNullableText(step.handoffSummary, '无交接说明')}
                    {'\n'}
                    下一位：{formatNullableText(step.handoffToMemberName, '协作结束')}
                  </div>
                  {(step.executionLogs ?? []).length > 0 ? (
                    <div className="integration-preview-grid">
                      {step.executionLogs?.map(log => (
                        <div className="entity-chip" key={log.id}>
                          <strong>
                            {formatNullableText(log.toolName, '未命名工具')}
                            {' '}· {formatExecutionLogStatus(log.status)}
                          </strong>
                          <span>
                            分类：{formatNullableText(log.toolCategory, '未设置')}
                            {' '}· 允许：{log.wasAllowed ? '是' : '否'}
                          </span>
                          <span>边界：{formatNullableText(log.boundarySummary, '未设置权限边界')}</span>
                          <span>输入：{formatNullableText(log.inputSummary, '无输入摘要')}</span>
                          <span>输出：{formatNullableText(log.outputSummary, '无输出摘要')}</span>
                          <span>
                            执行者：{formatNullableText(log.memberName, '系统')}
                            {' '}· 时间：{formatDateTime(log.executedAt)}
                          </span>
                        </div>
                      ))}
                    </div>
                  ) : null}
                </div>
              ))}
            </div>
          </div>
        ) : (
          <span className="entity-placeholder">选择一条协作运行记录后，这里会展示每一步的输入、输出和交接。</span>
        )}
      </div>
    </>
  );
}
