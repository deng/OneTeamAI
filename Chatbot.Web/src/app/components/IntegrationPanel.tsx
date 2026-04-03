import type { Dispatch, SetStateAction } from 'react';
import {
  ExternalSystemType,
  type CreateIntegrationConnectionRequest,
  type FileKnowledgeItemResponse,
  type IntegrationConnectionHealthResponse,
  type IntegrationConnectionResponse,
  type IntegrationPreviewItemResponse,
} from '../../generated/api';
import {
  formatDateTime,
  formatExternalSystemType,
  formatNullableText,
} from '../formatters';

type IntegrationPanelProps = {
  integrationForm: CreateIntegrationConnectionRequest;
  onIntegrationFormChange: Dispatch<SetStateAction<CreateIntegrationConnectionRequest>>;
  busyAction: string | null;
  canManageIntegrations: boolean;
  onCreateIntegration: () => void;
  integrationConnections: IntegrationConnectionResponse[];
  selectedIntegrationId: string;
  onSelectIntegrationId: (integrationId: string) => void;
  selectedIntegration: IntegrationConnectionResponse | null;
  integrationPreviewCount: number;
  selectedIntegrationHealth: IntegrationConnectionHealthResponse | null;
  onValidateIntegration: () => void;
  integrationFiles: FileKnowledgeItemResponse[];
  integrationPreviewCustomers: IntegrationPreviewItemResponse[];
  integrationPreviewProjects: IntegrationPreviewItemResponse[];
  integrationPreviewTickets: IntegrationPreviewItemResponse[];
  integrationPreviewTasks: IntegrationPreviewItemResponse[];
};

export function IntegrationPanel({
  integrationForm,
  onIntegrationFormChange,
  busyAction,
  canManageIntegrations,
  onCreateIntegration,
  integrationConnections,
  selectedIntegrationId,
  onSelectIntegrationId,
  selectedIntegration,
  integrationPreviewCount,
  selectedIntegrationHealth,
  onValidateIntegration,
  integrationFiles,
  integrationPreviewCustomers,
  integrationPreviewProjects,
  integrationPreviewTickets,
  integrationPreviewTasks,
}: IntegrationPanelProps) {
  return (
    <>
      <div className="panel-title panel-title-gap">外部集成</div>
      <div className="settings-grid">
        <div className="settings-section-header">
          <strong>连接管理</strong>
          <span>优先接入协作资产系统与业务管理系统，当前首批适配 Nextcloud 和 ERPNext。</span>
        </div>
        <div className="entity-card">
          <div className="entity-card-title">创建连接</div>
          <div className="entity-card-body">
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
              onClick={onCreateIntegration}
            >
              {busyAction === 'create-integration' ? '创建中...' : '创建连接'}
            </button>
          </div>
        </div>
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
                integrationFiles.length > 0 ? (
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
                )
              ) : (
                <div className="integration-preview-grid">
                  <div className="entity-chip">
                    <strong>客户</strong>
                    <span>{integrationPreviewCustomers.map(item => item.displayName).join(' / ') || '暂无数据'}</span>
                  </div>
                  <div className="entity-chip">
                    <strong>项目</strong>
                    <span>{integrationPreviewProjects.map(item => item.displayName).join(' / ') || '暂无数据'}</span>
                  </div>
                  <div className="entity-chip">
                    <strong>工单</strong>
                    <span>{integrationPreviewTickets.map(item => item.displayName).join(' / ') || '暂无数据'}</span>
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
      </div>
    </>
  );
}
