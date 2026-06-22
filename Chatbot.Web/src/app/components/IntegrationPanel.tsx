import { useState, type Dispatch, SetStateAction } from 'react';
import {
  type CustomerResponse,
  ExternalSystemType,
  type CreateIntegrationConnectionRequest,
  type FileKnowledgeItemResponse,
  type IntegrationConnectionHealthResponse,
  type IntegrationConnectionResponse,
  type IntegrationPreviewItemResponse,
  type ProjectResponse,
  type TicketResponse,
} from '../../generated/api';
import {
  formatAuditResult,
  formatDateTime,
  formatExternalSystemType,
  formatIntegrationAction,
  formatNullableText,
} from '../formatters';
import type { AuditLogItem } from '../types';
import { Modal } from './Modal';

type IntegrationPanelProps = {
  integrationForm: CreateIntegrationConnectionRequest;
  onIntegrationFormChange: Dispatch<SetStateAction<CreateIntegrationConnectionRequest>>;
  integrationFolderPath: string;
  onIntegrationFolderPathChange: (value: string) => void;
  busyAction: string | null;
  canManageIntegrations: boolean;
  onCreateIntegration: () => void;
  integrationConnections: IntegrationConnectionResponse[];
  selectedIntegrationId: string;
  onSelectIntegrationId: (integrationId: string) => void;
  selectedIntegration: IntegrationConnectionResponse | null;
  integrationPreviewCount: number;
  selectedIntegrationHealth: IntegrationConnectionHealthResponse | null;
  onRefreshIntegrationPreview: () => void;
  onRetryLatestIntegrationIssue: () => void;
  onValidateIntegration: () => void;
  integrationFiles: FileKnowledgeItemResponse[];
  integrationPreviewCustomers: IntegrationPreviewItemResponse[];
  integrationPreviewProjects: IntegrationPreviewItemResponse[];
  integrationPreviewTickets: IntegrationPreviewItemResponse[];
  integrationPreviewTasks: IntegrationPreviewItemResponse[];
  integrationAuditLogs: AuditLogItem[];
  importedCustomerExternalIds: string[];
  importedProjectExternalIds: string[];
  importedTicketExternalIds: string[];
  mappedCustomers: CustomerResponse[];
  mappedProjects: ProjectResponse[];
  mappedTickets: TicketResponse[];
  currentProjectId: string;
  currentProject: ProjectResponse | null;
  onNavigateToCustomer: (customerId: string) => void;
  onNavigateToProject: (projectId: string) => void;
  onNavigateToTicket: (ticketId: string) => void;
  onImportPreviewCustomer: (externalRecordId: string, forceUpdate?: boolean) => void;
  onImportPreviewProject: (externalRecordId: string, forceUpdate?: boolean) => void;
  onImportPreviewTicket: (externalRecordId: string, forceUpdate?: boolean) => void;
};

export function IntegrationPanel({
  integrationForm,
  onIntegrationFormChange,
  integrationFolderPath,
  onIntegrationFolderPathChange,
  busyAction,
  canManageIntegrations,
  onCreateIntegration,
  integrationConnections,
  selectedIntegrationId,
  onSelectIntegrationId,
  selectedIntegration,
  integrationPreviewCount,
  selectedIntegrationHealth,
  onRefreshIntegrationPreview,
  onRetryLatestIntegrationIssue,
  onValidateIntegration,
  integrationFiles,
  integrationPreviewCustomers,
  integrationPreviewProjects,
  integrationPreviewTickets,
  integrationPreviewTasks,
  integrationAuditLogs,
  importedCustomerExternalIds,
  importedProjectExternalIds,
  importedTicketExternalIds,
  mappedCustomers,
  mappedProjects,
  mappedTickets,
  currentProjectId,
  currentProject,
  onNavigateToCustomer,
  onNavigateToProject,
  onNavigateToTicket,
  onImportPreviewCustomer,
  onImportPreviewProject,
  onImportPreviewTicket,
}: IntegrationPanelProps) {
  const [showIntegrationForm, setShowIntegrationForm] = useState(false);
  const importedCustomerIdSet = new Set(importedCustomerExternalIds);
  const importedProjectIdSet = new Set(importedProjectExternalIds);
  const importedTicketIdSet = new Set(importedTicketExternalIds);
  const mappedCustomerByExternalId = new Map(
    mappedCustomers
      .filter(customer => customer.externalId)
      .map(customer => [customer.externalId as string, customer]),
  );
  const mappedProjectByExternalId = new Map(
    mappedProjects
      .filter(project => project.externalId)
      .map(project => [project.externalId as string, project]),
  );
  const mappedTicketByExternalId = new Map(
    mappedTickets
      .filter(ticket => ticket.externalId)
      .map(ticket => [ticket.externalId as string, ticket]),
  );
  const successCount = integrationAuditLogs.filter(log => log.result.toLowerCase() === 'success').length;
  const degradedCount = integrationAuditLogs.filter(log => log.result.toLowerCase() === 'degraded').length;
  const conflictCount = integrationAuditLogs.filter(log => log.result.toLowerCase() === 'conflict').length;
  const failureCount = integrationAuditLogs.filter(log => log.result.toLowerCase() === 'failed').length;
  const latestSyncIssue =
    integrationAuditLogs.find(log => ['failed', 'conflict', 'degraded'].includes(log.result.toLowerCase()))
    ?? null;
  const latestSuccessLog =
    integrationAuditLogs.find(log => log.result.toLowerCase() === 'success')
    ?? null;

  return (
    <>
      <div className="panel-title panel-title-gap">外部集成</div>
      <div className="settings-grid">
        <div className="settings-section-header">
          <strong>连接管理</strong>
          <span>优先接入协作资产系统与业务管理系统，当前首批适配 Nextcloud 和 ERPNext。</span>
        </div>
        {canManageIntegrations ? (
          <button className="secondary-button" type="button" onClick={() => setShowIntegrationForm(true)}>
            + 创建连接
          </button>
        ) : null}

        <Modal open={showIntegrationForm} onClose={() => setShowIntegrationForm(false)} title="创建连接">
          <label className="field">
            <span>系统类型</span>
            <select
              className="text-input"
              disabled={busyAction !== null}
              value={integrationForm.externalSystemType ?? ExternalSystemType.NUMBER_1}
              onChange={event => {
                const nextType = Number(event.currentTarget.value) as CreateIntegrationConnectionRequest['externalSystemType'];
                onIntegrationFormChange(current => ({
                  ...current,
                  externalSystemType: nextType,
                  name:
                    nextType === ExternalSystemType.NUMBER_2
                      ? 'ERPNext 业务系统'
                      : 'Nextcloud 主协作空间',
                  baseUrl:
                    nextType === ExternalSystemType.NUMBER_2
                      ? 'https://erpnext.example.com'
                      : 'https://nextcloud.example.com',
                  authConfig:
                    nextType === ExternalSystemType.NUMBER_2
                      ? '{"apiKey":"demo-key","apiSecret":"demo-secret"}'
                      : '{"username":"demo","appPassword":"demo-token"}',
                }));
              }}
            >
              <option value={ExternalSystemType.NUMBER_1}>Nextcloud</option>
              <option value={ExternalSystemType.NUMBER_2}>ERPNext</option>
            </select>
          </label>
          <label className="field">
            <span>连接名称</span>
            <input
              className="text-input"
              value={integrationForm.name ?? ''}
              onChange={event =>
                onIntegrationFormChange(current => ({ ...current, name: event.currentTarget.value }))
              }
            />
          </label>
          <label className="field">
            <span>Base URL</span>
            <input
              className="text-input"
              value={integrationForm.baseUrl ?? ''}
              onChange={event =>
                onIntegrationFormChange(current => ({ ...current, baseUrl: event.currentTarget.value }))
              }
            />
          </label>
          <label className="field">
            <span>认证配置</span>
            <textarea
              className="text-area"
              rows={3}
              value={integrationForm.authConfig ?? ''}
              onChange={event =>
                onIntegrationFormChange(current => ({ ...current, authConfig: event.currentTarget.value }))
              }
            />
          </label>
          <label className="checkbox-field">
            <input
              checked={integrationForm.isEnabled ?? true}
              type="checkbox"
              onChange={event =>
                onIntegrationFormChange(current => ({ ...current, isEnabled: event.currentTarget.checked }))
              }
            />
            启用该连接
          </label>
          <button
            className="secondary-button"
            disabled={busyAction !== null || !canManageIntegrations}
            type="button"
            onClick={() => { onCreateIntegration(); setShowIntegrationForm(false); }}
          >
            {busyAction === 'create-integration' ? '创建中...' : '创建连接'}
          </button>
        </Modal>
        <div className="entity-card">
          <div className="entity-card-title">连接概览</div>
          {integrationConnections.length > 0 ? (
            <div className="entity-chip-list">
              {integrationConnections.map(connection => (
                <button
                  className={
                    connection.id === selectedIntegrationId
                      ? 'entity-chip entity-chip-button entity-chip-selected'
                      : 'entity-chip entity-chip-button'
                  }
                  key={connection.id}
                  type="button"
                  onClick={() => onSelectIntegrationId(connection.id ?? '')}
                >
                  <strong>{connection.name ?? '未命名连接'}</strong>
                  <span>{formatExternalSystemType(connection.externalSystemType)}</span>
                  <span>{formatNullableText(connection.baseUrl, '未设置 Base URL')}</span>
                  <span>
                    状态：
                    {connection.isEnabled ? '已启用' : '已停用'}
                    {' '}· 认证：
                    {connection.hasAuthConfig ? '已配置' : '未配置'}
                  </span>
                  <span>创建于：{formatDateTime(connection.createdAt)}</span>
                </button>
              ))}
            </div>
          ) : (
            <span className="entity-placeholder">当前团队还没有外部系统连接。</span>
          )}
        </div>
        <div className="entity-card">
          <div className="entity-card-title">预览结果</div>
          {selectedIntegration ? (
            <div className="entity-card-body">
              <strong>{selectedIntegration.name ?? '未命名连接'}</strong>
              <span>
                {formatExternalSystemType(selectedIntegration.externalSystemType)} · 预览记录数：
                {integrationPreviewCount}
              </span>
              <button
                className="secondary-button"
                disabled={busyAction !== null || !canManageIntegrations || !selectedIntegration.id}
                type="button"
                onClick={onValidateIntegration}
              >
                {busyAction === 'validate-integration' ? '校验中...' : '校验连接'}
              </button>
              <button
                className="secondary-button"
                disabled={busyAction !== null || !selectedIntegration.id}
                type="button"
                onClick={onRefreshIntegrationPreview}
              >
                {busyAction === 'refresh-integration-preview' ? '刷新中...' : '刷新预览'}
              </button>
              {selectedIntegrationHealth ? (
                <div className="mini-meta">
                  <strong>
                    连通性：
                    {selectedIntegrationHealth.isReachable ? '可达' : '不可达'}
                    {' '}· 鉴权：
                    {selectedIntegrationHealth.isAuthenticated ? '成功' : '未通过'}
                  </strong>
                  <span>{formatNullableText(selectedIntegrationHealth.message, '暂无说明')}</span>
                  <span>
                    版本：{formatNullableText(selectedIntegrationHealth.systemVersion, '未知')}
                    {' '}· 检查时间：{formatDateTime(selectedIntegrationHealth.checkedAt)}
                  </span>
                </div>
              ) : null}
              {selectedIntegration.externalSystemType === ExternalSystemType.NUMBER_1 ? (
                <>
                  <label className="field">
                    <span>知识目录</span>
                    <input
                      className="text-input"
                      disabled={busyAction !== null}
                      placeholder="/Knowledge/Base"
                      value={integrationFolderPath}
                      onChange={event => onIntegrationFolderPathChange(event.currentTarget.value)}
                    />
                  </label>
                  {integrationFiles.length > 0 ? (
                  <div className="integration-preview-grid">
                    {integrationFiles.map(file => (
                      <div className="entity-chip" key={file.id ?? file.path}>
                        <strong>{file.name ?? '未命名文件'}</strong>
                        <span>{formatNullableText(file.path, '未设置路径')}</span>
                        <span>
                          {formatNullableText(file.mimeType, '未知类型')} · {file.size ?? 0} bytes
                        </span>
                        <span>更新于：{formatDateTime(file.updatedAt ?? undefined)}</span>
                      </div>
                    ))}
                  </div>
                  ) : (
                    <span className="entity-placeholder">暂无文件预览结果。</span>
                  )}
                </>
              ) : (
                <div className="integration-preview-grid">
                  <div className="entity-chip">
                    <strong>客户</strong>
                    {integrationPreviewCustomers.length > 0 ? (
                      <div className="entity-chip-list">
                        {integrationPreviewCustomers.map(item => (
                          (() => {
                            const itemId = item.id ?? '';
                            const mappedCustomer = mappedCustomerByExternalId.get(itemId);
                            return (
                              <div className="entity-chip" key={itemId}>
                                <strong>{item.displayName}</strong>
                                <span>{formatNullableText(item.summary, '无附加摘要')}</span>
                                <span>{importedCustomerIdSet.has(itemId) ? '已导入，点击同步更新' : '点击导入到本地客户'}</span>
                                <button
                                  className="secondary-button"
                                  type="button"
                                  disabled={!itemId}
                                  onClick={() => onImportPreviewCustomer(itemId, importedCustomerIdSet.has(itemId))}
                                >
                                  {importedCustomerIdSet.has(itemId) ? '同步更新' : '导入客户'}
                                </button>
                                {mappedCustomer?.id ? (
                                  <button
                                    className="secondary-button"
                                    type="button"
                                    onClick={() => onNavigateToCustomer(mappedCustomer.id ?? '')}
                                  >
                                    查看本地客户
                                  </button>
                                ) : null}
                              </div>
                            );
                          })()
                        ))}
                      </div>
                    ) : (
                      <span>暂无数据</span>
                    )}
                  </div>
                  <div className="entity-chip">
                    <strong>项目</strong>
                    {integrationPreviewProjects.length > 0 ? (
                      <div className="entity-chip-list">
                        {integrationPreviewProjects.map(item => (
                          (() => {
                            const itemId = item.id ?? '';
                            const mappedProject = mappedProjectByExternalId.get(itemId);
                            return (
                              <div className="entity-chip" key={itemId}>
                                <strong>{item.displayName}</strong>
                                <span>{formatNullableText(item.summary, '无附加摘要')}</span>
                                <span>{importedProjectIdSet.has(itemId) ? '已导入，点击同步更新' : '点击导入到本地项目'}</span>
                                <button
                                  className="secondary-button"
                                  type="button"
                                  disabled={!itemId}
                                  onClick={() => onImportPreviewProject(itemId, importedProjectIdSet.has(itemId))}
                                >
                                  {importedProjectIdSet.has(itemId) ? '同步更新' : '导入项目'}
                                </button>
                                {mappedProject?.id ? (
                                  <button
                                    className="secondary-button"
                                    type="button"
                                    onClick={() => onNavigateToProject(mappedProject.id ?? '')}
                                  >
                                    查看本地项目
                                  </button>
                                ) : null}
                              </div>
                            );
                          })()
                        ))}
                      </div>
                    ) : (
                      <span>暂无数据</span>
                    )}
                  </div>
                  <div className="entity-chip">
                    <strong>工单</strong>
                    {integrationPreviewTickets.length > 0 ? (
                      <div className="entity-chip-list">
                        {integrationPreviewTickets.map(item => {
                          const itemId = item.id ?? '';
                          const isImported = importedTicketIdSet.has(itemId);
                          const mappedTicket = mappedTicketByExternalId.get(itemId);
                          return (
                            <div className="entity-chip" key={itemId}>
                              <strong>{item.displayName}</strong>
                              <span>{formatNullableText(item.summary, '无附加摘要')}</span>
                              <span>
                                {!currentProjectId
                                  ? '请先在工作台选择一个本地项目'
                                  : (isImported ? '已导入，点击同步更新' : `导入到项目：${formatNullableText(currentProject?.name, '当前项目')}`)}
                              </span>
                              <button
                                className="secondary-button"
                                type="button"
                                disabled={!itemId || !currentProjectId}
                                onClick={() => onImportPreviewTicket(itemId, isImported)}
                              >
                                {isImported ? '同步更新' : '导入工单'}
                              </button>
                              {mappedTicket?.id ? (
                                <button
                                  className="secondary-button"
                                  type="button"
                                  onClick={() => onNavigateToTicket(mappedTicket.id ?? '')}
                                >
                                  查看本地工单
                                </button>
                              ) : null}
                            </div>
                          );
                        })}
                      </div>
                    ) : (
                      <span>暂无数据</span>
                    )}
                  </div>
                  <div className="entity-chip">
                    <strong>任务</strong>
                    <span>{integrationPreviewTasks.map(item => item.displayName).join(' / ') || '暂无数据'}</span>
                  </div>
                </div>
              )}
            </div>
          ) : (
            <span className="entity-placeholder">选择一个连接后，这里会展示适配器返回的预览结果。</span>
          )}
        </div>
        <div className="entity-card">
          <div className="entity-card-title">同步健康摘要</div>
          <div className="entity-card-body">
            <span>
              成功：{successCount} · 降级：{degradedCount} · 冲突：{conflictCount} · 失败：{failureCount}
            </span>
            <span>
              最近成功同步：{latestSuccessLog ? formatDateTime(latestSuccessLog.createdAt) : '暂无'}
            </span>
            {latestSyncIssue ? (
              <div className="mini-meta">
                <strong>最近需要处理的记录：{formatIntegrationAction(latestSyncIssue.actionType)}</strong>
                <span>{latestSyncIssue.summary}</span>
                <span>
                  {formatAuditResult(latestSyncIssue.result)} · {formatDateTime(latestSyncIssue.createdAt)}
                </span>
                <button
                  className="secondary-button"
                  disabled={busyAction !== null || !selectedIntegration}
                  type="button"
                  onClick={onRetryLatestIntegrationIssue}
                >
                  {busyAction === 'validate-integration'
                    ? '校验中...'
                    : busyAction === 'refresh-integration-preview'
                      ? '刷新中...'
                      : '重试当前连接'}
                </button>
              </div>
            ) : (
              <span className="entity-placeholder">最近没有冲突或失败记录。</span>
            )}
          </div>
        </div>
        <div className="entity-card">
          <div className="entity-card-title">
            最近同步记录
            {selectedIntegration ? ` · ${selectedIntegration.name}` : ''}
          </div>
          {integrationAuditLogs.length > 0 ? (
            <div className="entity-chip-list">
              {integrationAuditLogs.slice(0, 8).map(log => (
                <div className="entity-chip" key={log.id}>
                  <strong>{formatIntegrationAction(log.actionType)}</strong>
                  <span>{log.summary}</span>
                  <span>
                    {formatAuditResult(log.result)} · {formatDateTime(log.createdAt)}
                  </span>
                  <span>{formatNullableText(log.userDisplayName, '系统')}</span>
                </div>
              ))}
            </div>
          ) : (
            <span className="entity-placeholder">当前还没有集成同步记录。</span>
          )}
        </div>
        <div className="entity-card">
          <div className="entity-card-title">本地映射总览</div>
          {selectedIntegration ? (
            <div className="entity-card-body">
              <span>
                已映射客户：{mappedCustomers.length} · 项目：{mappedProjects.length} · 工单：{mappedTickets.length}
              </span>
              {mappedCustomers.length > 0 ? (
                <div className="entity-chip-list">
                  {mappedCustomers.slice(0, 4).map(customer => (
                    <button
                      className="entity-chip entity-chip-button"
                      key={customer.id}
                      type="button"
                      onClick={() => onNavigateToCustomer(customer.id ?? '')}
                    >
                      <strong>{formatNullableText(customer.displayName, '未命名客户')}</strong>
                      <span>客户映射</span>
                      <span>{formatNullableText(customer.externalId, '无外部 ID')}</span>
                    </button>
                  ))}
                </div>
              ) : null}
              {mappedProjects.length > 0 ? (
                <div className="entity-chip-list">
                  {mappedProjects.slice(0, 4).map(project => (
                    <button
                      className="entity-chip entity-chip-button"
                      key={project.id}
                      type="button"
                      onClick={() => onNavigateToProject(project.id ?? '')}
                    >
                      <strong>{formatNullableText(project.name, '未命名项目')}</strong>
                      <span>项目映射</span>
                      <span>{formatNullableText(project.externalId, '无外部 ID')}</span>
                    </button>
                  ))}
                </div>
              ) : null}
              {mappedTickets.length > 0 ? (
                <div className="entity-chip-list">
                  {mappedTickets.slice(0, 4).map(ticket => (
                    <button
                      className="entity-chip entity-chip-button"
                      key={ticket.id}
                      type="button"
                      onClick={() => onNavigateToTicket(ticket.id ?? '')}
                    >
                      <strong>{formatNullableText(ticket.title, '未命名工单')}</strong>
                      <span>工单映射</span>
                      <span>{formatNullableText(ticket.externalId, '无外部 ID')}</span>
                    </button>
                  ))}
                </div>
              ) : null}
              {mappedCustomers.length === 0 && mappedProjects.length === 0 && mappedTickets.length === 0 ? (
                <span className="entity-placeholder">当前连接还没有落到本地的映射记录。</span>
              ) : null}
            </div>
          ) : (
            <span className="entity-placeholder">选择一个连接后，这里会展示已经同步到本地的映射结果。</span>
          )}
        </div>
      </div>
    </>
  );
}
