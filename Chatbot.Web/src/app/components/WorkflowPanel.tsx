import { useState } from 'react';
import {
  AgentWorkflowTriggerMode,
  type AgentWorkflowResponse,
  type MemberResponse,
  type RunTicketWorkflowRequest,
} from '../../generated/api';
import {
  formatDateTime,
  formatExecutionLogStatus,
  formatNullableText,
  formatPreviewText,
  formatWorkflowTriggerMode,
  formatWorkflowStatus,
  formatWorkflowStepStatus,
} from '../formatters';
import type { WorkflowTemplateItem } from '../types';

type WorkflowScope = 'ticket' | 'conversation' | 'project' | null;
type AttemptFilter = 'all' | 'rejected';

async function copyToClipboard(text: string) {
  if (typeof navigator !== 'undefined' && navigator.clipboard?.writeText) {
    await navigator.clipboard.writeText(text);
    return;
  }

  if (typeof document === 'undefined') {
    throw new Error('clipboard_unavailable');
  }

  const textarea = document.createElement('textarea');
  textarea.value = text;
  textarea.setAttribute('readonly', 'true');
  textarea.style.position = 'absolute';
  textarea.style.left = '-9999px';
  document.body.appendChild(textarea);
  textarea.select();
  document.execCommand('copy');
  document.body.removeChild(textarea);
}

function downloadTextFile(filename: string, text: string) {
  if (typeof document === 'undefined' || typeof URL === 'undefined') {
    return;
  }

  const blob = new Blob([text], { type: 'application/json;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

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

function AttemptTimeline({
  title,
  filePrefix,
  attempts,
  attemptFilter,
  onCopyText,
  onDownloadText,
}: {
  title: string;
  filePrefix: string;
  attempts:
    | Array<{
        attempt?: number;
        outcome?: string | null;
        schemaVersion?: string | null;
        validationError?: string | null;
        rawResponse?: string | null;
        createdAt?: Date;
      }>
    | null
    | undefined;
  attemptFilter: AttemptFilter;
  onCopyText: (text: string, label: string) => Promise<void>;
  onDownloadText: (filename: string, text: string) => void;
}) {
  if (!attempts || attempts.length === 0) {
    return <span>{title}：无记录</span>;
  }

  const rejectedAttempts = attempts.filter(
    attempt => (attempt.outcome ?? '').toLowerCase() === 'rejected',
  );
  const visibleAttempts =
    attemptFilter === 'rejected'
      ? rejectedAttempts
      : attempts;

  if (visibleAttempts.length === 0) {
    return <span>{title}：当前筛选下无记录</span>;
  }

  return (
    <details>
      <summary>
        {title}（显示 {visibleAttempts.length} / 共 {attempts.length} 次）
      </summary>
      <div className="entity-chip-list">
        {rejectedAttempts.length > 0 ? (
          <>
            <button
              className="entity-chip entity-chip-button"
              type="button"
              onClick={() => void onCopyText(JSON.stringify(rejectedAttempts, null, 2), `${title} rejected attempts`)}
            >
              <strong>复制 Rejected</strong>
              <span>{rejectedAttempts.length} 条失败尝试</span>
            </button>
            <button
              className="entity-chip entity-chip-button"
              type="button"
              onClick={() =>
                onDownloadText(`${filePrefix}-rejected-attempts.json`, JSON.stringify(rejectedAttempts, null, 2))
              }
            >
              <strong>导出 Rejected</strong>
              <span>JSON</span>
            </button>
          </>
        ) : null}
      </div>
      <div className="integration-preview-grid">
        {visibleAttempts.map((attempt, index) => (
          <div className="entity-chip" key={`${attempt.attempt ?? index}-${attempt.createdAt?.toString() ?? index}`}>
            <strong>
              第 {attempt.attempt ?? index + 1} 次 · {formatNullableText(attempt.outcome, 'unknown')}
            </strong>
            <span>Schema：{formatNullableText(attempt.schemaVersion, '未记录')}</span>
            <span>时间：{formatDateTime(attempt.createdAt)}</span>
            <span>校验结果：{formatNullableText(attempt.validationError, '通过')}</span>
            <div className="entity-chip-list">
              <button
                className="entity-chip entity-chip-button"
                disabled={!attempt.rawResponse}
                type="button"
                onClick={() => void onCopyText(attempt.rawResponse ?? '', `${title} attempt ${attempt.attempt ?? index + 1}`)}
              >
                <strong>复制 Raw</strong>
                <span>第 {attempt.attempt ?? index + 1} 次</span>
              </button>
              <button
                className="entity-chip entity-chip-button"
                disabled={!attempt.rawResponse}
                type="button"
                onClick={() =>
                  onDownloadText(
                    `${filePrefix}-attempt-${attempt.attempt ?? index + 1}.json`,
                    attempt.rawResponse ?? '',
                  )
                }
              >
                <strong>导出 Raw</strong>
                <span>JSON</span>
              </button>
            </div>
            <details>
              <summary>查看完整 raw response</summary>
              <pre className="detail-message-body">{formatNullableText(attempt.rawResponse, '未记录')}</pre>
            </details>
          </div>
        ))}
      </div>
    </details>
  );
}

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
  const [clipboardStatus, setClipboardStatus] = useState<string | null>(null);
  const [attemptFilter, setAttemptFilter] = useState<AttemptFilter>('all');
  const [focusedStepId, setFocusedStepId] = useState('');

  const availableSteps = selectedWorkflow?.steps ?? [];
  const effectiveFocusedStepId = availableSteps.some(step => step.id === focusedStepId) ? focusedStepId : '';
  const visibleSteps = effectiveFocusedStepId
    ? availableSteps.filter(step => step.id === effectiveFocusedStepId)
    : availableSteps;

  async function handleCopyText(text: string, label: string) {
    try {
      await copyToClipboard(text);
      setClipboardStatus(`${label} 已复制`);
      window.setTimeout(() => {
        setClipboardStatus(current => (current === `${label} 已复制` ? null : current));
      }, 1800);
    } catch {
      setClipboardStatus(`${label} 复制失败`);
    }
  }

  function handleDownloadText(filename: string, text: string) {
    downloadTextFile(filename, text);
    setClipboardStatus(`${filename} 已导出`);
    window.setTimeout(() => {
      setClipboardStatus(current => (current === `${filename} 已导出` ? null : current));
    }, 1800);
  }

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
              <span>触发方式</span>
              <select
                className="text-input"
                value={workflowForm.triggerMode ?? AgentWorkflowTriggerMode.NUMBER_0}
                onChange={event =>
                  onWorkflowFormChange(current => ({
                    ...current,
                    triggerMode: Number(event.currentTarget.value) as AgentWorkflowTriggerMode,
                  }))
                }
              >
                <option value={AgentWorkflowTriggerMode.NUMBER_0}>手动触发</option>
                <option value={AgentWorkflowTriggerMode.NUMBER_1}>自动触发</option>
              </select>
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
                <span>触发：{formatWorkflowTriggerMode(workflow.triggerMode)}</span>
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
            {clipboardStatus ? <span>{clipboardStatus}</span> : null}
            <strong>{formatNullableText(selectedWorkflow.goal, '未设置目标')}</strong>
            <span>
              {formatWorkflowStatus(selectedWorkflow.status)} · 创建于：
              {formatDateTime(selectedWorkflow.createdAt)}
            </span>
            <span>类型：{formatNullableText(selectedWorkflow.workflowType, '默认协作链')}</span>
            <span>触发方式：{formatWorkflowTriggerMode(selectedWorkflow.triggerMode)}</span>
            <span>摘要 Schema：{formatNullableText(selectedWorkflow.summarySchemaVersion, '未记录')}</span>
            <span>摘要原始返回：{formatPreviewText(selectedWorkflow.summaryRawResponse, 220, '未记录')}</span>
            <div className="field-grid">
              <label className="field">
                <span>步骤筛选</span>
                <select
                  className="text-input"
                  value={effectiveFocusedStepId}
                  onChange={event => setFocusedStepId(event.currentTarget.value)}
                >
                  <option value="">全部步骤</option>
                  {availableSteps.map(step => (
                    <option key={step.id} value={step.id ?? ''}>
                      {step.sequence}. {formatNullableText(step.memberName, '系统')} · {formatNullableText(step.actionType, '未设置动作')}
                    </option>
                  ))}
                </select>
              </label>
              <label className="field">
                <span>尝试筛选</span>
                <select
                  className="text-input"
                  value={attemptFilter}
                  onChange={event => setAttemptFilter(event.currentTarget.value as AttemptFilter)}
                >
                  <option value="all">全部 attempts</option>
                  <option value="rejected">仅 rejected</option>
                </select>
              </label>
            </div>
            <div className="entity-chip-list">
              <button
                className="entity-chip entity-chip-button"
                disabled={!selectedWorkflow.summaryRawResponse}
                type="button"
                onClick={() => void handleCopyText(selectedWorkflow.summaryRawResponse ?? '', '摘要 raw response')}
              >
                <strong>复制摘要 Raw</strong>
                <span>完整原文</span>
              </button>
              <button
                className="entity-chip entity-chip-button"
                disabled={!selectedWorkflow.summaryRawResponse}
                type="button"
                onClick={() =>
                  handleDownloadText(
                    `workflow-${selectedWorkflow.id ?? 'summary'}-summary-raw.json`,
                    selectedWorkflow.summaryRawResponse ?? '',
                  )
                }
              >
                <strong>导出摘要 Raw</strong>
                <span>JSON</span>
              </button>
            </div>
            <details>
              <summary>查看摘要完整 raw response</summary>
              <pre className="detail-message-body">
                {formatNullableText(selectedWorkflow.summaryRawResponse, '未记录')}
              </pre>
            </details>
            <AttemptTimeline
              title="摘要生成时间线"
              filePrefix={`workflow-${selectedWorkflow.id ?? 'summary'}-summary`}
              attempts={selectedWorkflow.summaryAttempts}
              attemptFilter={attemptFilter}
              onCopyText={handleCopyText}
              onDownloadText={handleDownloadText}
            />
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
              {visibleSteps.map(step => (
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
                    输出 Schema：{formatNullableText(step.outputSchemaVersion, '未记录')}
                    {'\n'}
                    原始返回：{formatPreviewText(step.outputRawResponse, 220, '未记录')}
                    {'\n'}
                    交接：{formatNullableText(step.handoffSummary, '无交接说明')}
                    {'\n'}
                    下一位：{formatNullableText(step.handoffToMemberName, '协作结束')}
                  </div>
                  <div className="entity-chip-list">
                    <button
                      className="entity-chip entity-chip-button"
                      disabled={!step.outputRawResponse}
                      type="button"
                      onClick={() =>
                        void handleCopyText(
                          step.outputRawResponse ?? '',
                          `步骤 ${step.sequence} raw response`,
                        )
                      }
                    >
                      <strong>复制步骤 Raw</strong>
                      <span>第 {step.sequence} 步</span>
                    </button>
                    <button
                      className="entity-chip entity-chip-button"
                      disabled={!step.outputRawResponse}
                      type="button"
                      onClick={() =>
                        handleDownloadText(
                          `workflow-${selectedWorkflow.id ?? 'step'}-step-${step.sequence}-raw.json`,
                          step.outputRawResponse ?? '',
                        )
                      }
                    >
                      <strong>导出步骤 Raw</strong>
                      <span>JSON</span>
                    </button>
                  </div>
                  <details>
                    <summary>查看步骤完整 raw response</summary>
                    <pre className="detail-message-body">{formatNullableText(step.outputRawResponse, '未记录')}</pre>
                  </details>
                  <AttemptTimeline
                    title="步骤生成时间线"
                    filePrefix={`workflow-${selectedWorkflow.id ?? 'step'}-step-${step.sequence}`}
                    attempts={step.outputAttempts}
                    attemptFilter={attemptFilter}
                    onCopyText={handleCopyText}
                    onDownloadText={handleDownloadText}
                  />
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
