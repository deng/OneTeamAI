import { useEffect, useMemo, useState } from 'react';
import {
  ExternalSystemType,
  type CreateIntegrationConnectionRequest,
  type FileKnowledgeItemResponse,
  type IntegrationConnectionHealthResponse,
  type IntegrationConnectionResponse,
  type IntegrationPreviewItemResponse,
} from '../generated/api';
import { createWorkspaceApis, fetchJson } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

function emptyIntegrationForm(): CreateIntegrationConnectionRequest {
  return {
    externalSystemType: ExternalSystemType.NUMBER_1,
    name: 'Nextcloud 主协作空间',
    baseUrl: 'https://nextcloud.example.com',
    authConfig: '{"username":"demo","appPassword":"demo-token"}',
    isEnabled: true,
  };
}

export function useWorkspaceIntegrations({
  currentTeamId,
  refreshWorkspaceData,
  refreshCustomers,
  refreshTickets,
  selectedProjectId,
  selectedCustomerId,
  token,
  runAction,
  setFeedback,
}: {
  currentTeamId: string;
  refreshWorkspaceData?: (teamId: string) => Promise<void>;
  refreshCustomers?: (teamId: string) => Promise<void>;
  refreshTickets?: () => Promise<void>;
  selectedProjectId?: string;
  selectedCustomerId?: string;
  token: string | null;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [integrationForm, setIntegrationForm] = useState<CreateIntegrationConnectionRequest>(emptyIntegrationForm);
  const [integrationConnections, setIntegrationConnections] = useState<IntegrationConnectionResponse[]>([]);
  const [selectedIntegrationId, setSelectedIntegrationId] = useState('');
  const [selectedIntegrationHealth, setSelectedIntegrationHealth] =
    useState<IntegrationConnectionHealthResponse | null>(null);
  const [integrationFolderPath, setIntegrationFolderPath] = useState('');
  const [integrationFiles, setIntegrationFiles] = useState<FileKnowledgeItemResponse[]>([]);
  const [integrationPreviewCustomers, setIntegrationPreviewCustomers] = useState<IntegrationPreviewItemResponse[]>([]);
  const [integrationPreviewProjects, setIntegrationPreviewProjects] = useState<IntegrationPreviewItemResponse[]>([]);
  const [integrationPreviewTickets, setIntegrationPreviewTickets] = useState<IntegrationPreviewItemResponse[]>([]);
  const [integrationPreviewTasks, setIntegrationPreviewTasks] = useState<IntegrationPreviewItemResponse[]>([]);

  const { integrationsApi } = useMemo(() => createWorkspaceApis(token), [token]);
  const selectedIntegration =
    integrationConnections.find(connection => connection.id === selectedIntegrationId) ?? null;
  const integrationPreviewCount =
    integrationFiles.length
    + integrationPreviewCustomers.length
    + integrationPreviewProjects.length
    + integrationPreviewTickets.length
    + integrationPreviewTasks.length;

  async function loadIntegrationPreview(
    teamId: string,
    connection: IntegrationConnectionResponse | null,
    folderPathOverride?: string,
  ) {
    if (!connection?.id) {
      setSelectedIntegrationHealth(null);
      setIntegrationFiles([]);
      setIntegrationPreviewCustomers([]);
      setIntegrationPreviewProjects([]);
      setIntegrationPreviewTickets([]);
      setIntegrationPreviewTasks([]);
      return;
    }

    if (connection.externalSystemType === ExternalSystemType.NUMBER_1) {
      const folderPath = (folderPathOverride ?? integrationFolderPath.trim()) || undefined;
      const files = await integrationsApi.previewIntegrationFiles({
        teamId,
        connectionId: connection.id,
        folderPath,
      });

      setIntegrationFiles(files);
      setIntegrationPreviewCustomers([]);
      setIntegrationPreviewProjects([]);
      setIntegrationPreviewTickets([]);
      setIntegrationPreviewTasks([]);
      return;
    }

    const [customers, projects, tickets, tasks] = await Promise.all([
      integrationsApi.previewIntegrationCustomers({
        teamId,
        connectionId: connection.id,
      }),
      integrationsApi.previewIntegrationProjects({
        teamId,
        connectionId: connection.id,
      }),
      integrationsApi.previewIntegrationTickets({
        teamId,
        connectionId: connection.id,
      }),
      integrationsApi.previewIntegrationTasks({
        teamId,
        connectionId: connection.id,
      }),
    ]);

    setIntegrationFiles([]);
    setIntegrationPreviewCustomers(customers);
    setIntegrationPreviewProjects(projects);
    setIntegrationPreviewTickets(tickets);
    setIntegrationPreviewTasks(tasks);
  }

  async function refreshIntegrations(teamId: string) {
    const connections = await integrationsApi.listIntegrationConnections({ teamId });
    setIntegrationConnections(connections);

    const nextSelectedId =
      connections.some(connection => connection.id === selectedIntegrationId)
        ? selectedIntegrationId
        : (connections[0]?.id ?? '');
    setSelectedIntegrationId(nextSelectedId);

    const nextSelectedConnection =
      connections.find(connection => connection.id === nextSelectedId) ?? null;
    if (nextSelectedConnection?.externalSystemType !== ExternalSystemType.NUMBER_1) {
      setIntegrationFolderPath('');
    }
    await loadIntegrationPreview(teamId, nextSelectedConnection);
  }

  useEffect(() => {
    if (!token || !currentTeamId) {
      setIntegrationConnections([]);
      setSelectedIntegrationId('');
      setSelectedIntegrationHealth(null);
      setIntegrationFolderPath('');
      setIntegrationFiles([]);
      setIntegrationPreviewCustomers([]);
      setIntegrationPreviewProjects([]);
      setIntegrationPreviewTickets([]);
      setIntegrationPreviewTasks([]);
      return;
    }

    let cancelled = false;

    void (async () => {
      const connections = await integrationsApi.listIntegrationConnections({ teamId: currentTeamId });

      if (cancelled) {
        return;
      }

      setIntegrationConnections(connections);
      const nextSelectedId =
        connections.some(connection => connection.id === selectedIntegrationId)
          ? selectedIntegrationId
          : (connections[0]?.id ?? '');
      setSelectedIntegrationId(nextSelectedId);

      const nextSelectedConnection =
        connections.find(connection => connection.id === nextSelectedId) ?? null;
      if (nextSelectedConnection?.externalSystemType !== ExternalSystemType.NUMBER_1) {
        setIntegrationFolderPath('');
      }
      await loadIntegrationPreview(currentTeamId, nextSelectedConnection);
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: error instanceof Error ? error.message : '外部集成加载失败。',
        });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [currentTeamId, integrationsApi, selectedIntegrationId, setFeedback, token]);

  async function handleCreateIntegration() {
    if (!currentTeamId) {
      return;
    }

    await runAction('create-integration', async () => {
      const created = await integrationsApi.createIntegrationConnection({
        teamId: currentTeamId,
        createIntegrationConnectionRequest: integrationForm,
      });

      await refreshIntegrations(currentTeamId);
      setSelectedIntegrationId(created.id ?? '');
      setSelectedIntegrationHealth(null);
      setFeedback({
        kind: 'success',
        text: `已创建连接 ${created.name ?? ''}。`,
      });
    });
  }

  async function handleValidateIntegration() {
    if (!currentTeamId || !selectedIntegration?.id) {
      return;
    }

    const connectionId = selectedIntegration.id;

    await runAction('validate-integration', async () => {
      const health = await integrationsApi.validateIntegrationConnection({
        teamId: currentTeamId,
        connectionId,
      });

      setSelectedIntegrationHealth(health);
      await loadIntegrationPreview(currentTeamId, selectedIntegration);
      setFeedback({
        kind: 'success',
        text: `已校验连接 ${selectedIntegration.name ?? ''}。`,
      });
    });
  }

  async function handleSelectIntegrationId(integrationId: string) {
    setSelectedIntegrationId(integrationId);
    if (!currentTeamId) {
      return;
    }

    const connection = integrationConnections.find(item => item.id === integrationId) ?? null;
    setSelectedIntegrationHealth(null);
    if (connection?.externalSystemType !== ExternalSystemType.NUMBER_1) {
      setIntegrationFolderPath('');
    }
    await loadIntegrationPreview(currentTeamId, connection);
  }

  async function handleRefreshIntegrationPreview() {
    if (!currentTeamId || !selectedIntegration) {
      return;
    }

    await runAction('refresh-integration-preview', async () => {
      await loadIntegrationPreview(currentTeamId, selectedIntegration);
      setFeedback({
        kind: 'success',
        text:
          selectedIntegration.externalSystemType === ExternalSystemType.NUMBER_1
            ? `已刷新 ${selectedIntegration.name ?? ''} 的文件预览。`
            : `已刷新 ${selectedIntegration.name ?? ''} 的业务预览。`,
      });
    });
  }

  async function handleImportPreviewCustomer(externalRecordId: string, forceUpdate = false) {
    if (!currentTeamId || !selectedIntegration?.id || !externalRecordId) {
      return;
    }

    await runAction('import-integration-customer', async () => {
      await fetchJson(`/api/teams/${currentTeamId}/integrations/${selectedIntegration.id}/customers/import`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token ?? ''}`,
        },
        body: JSON.stringify({
          externalRecordId,
          forceUpdate,
        }),
      });

      if (refreshCustomers) {
        await refreshCustomers(currentTeamId);
      }

      setFeedback({
        kind: 'success',
        text: forceUpdate ? '已同步更新外部客户到当前团队。' : '已将外部客户导入到当前团队。',
      });
    });
  }

  async function handleImportPreviewProject(externalRecordId: string, forceUpdate = false) {
    if (!currentTeamId || !selectedIntegration?.id || !externalRecordId) {
      return;
    }

    await runAction('import-integration-project', async () => {
      await fetchJson(`/api/teams/${currentTeamId}/integrations/${selectedIntegration.id}/projects/import`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token ?? ''}`,
        },
        body: JSON.stringify({
          externalRecordId,
          forceUpdate,
        }),
      });

      if (refreshWorkspaceData) {
        await refreshWorkspaceData(currentTeamId);
      }

      setFeedback({
        kind: 'success',
        text: forceUpdate ? '已同步更新外部项目到当前团队。' : '已将外部项目导入到当前团队。',
      });
    });
  }

  async function handleImportPreviewTicket(externalRecordId: string, forceUpdate = false) {
    if (!currentTeamId || !selectedIntegration?.id || !externalRecordId || !selectedProjectId) {
      return;
    }

    await runAction('import-integration-ticket', async () => {
      await fetchJson(`/api/teams/${currentTeamId}/integrations/${selectedIntegration.id}/tickets/import`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token ?? ''}`,
        },
        body: JSON.stringify({
          externalRecordId,
          projectId: selectedProjectId,
          customerId: selectedCustomerId || undefined,
          forceUpdate,
        }),
      });

      if (refreshTickets) {
        await refreshTickets();
      }

      setFeedback({
        kind: 'success',
        text: forceUpdate ? '已同步更新外部工单到当前团队。' : '已将外部工单导入到当前团队。',
      });
    });
  }

  return {
    integrationConnections,
    integrationFiles,
    integrationFolderPath,
    integrationForm,
    integrationPreviewCount,
    integrationPreviewCustomers,
    integrationPreviewProjects,
    integrationPreviewTasks,
    integrationPreviewTickets,
    selectedIntegration,
    selectedIntegrationHealth,
    selectedIntegrationId,
    setIntegrationFolderPath,
    setIntegrationForm,
    handleCreateIntegration,
    handleImportPreviewCustomer,
    handleImportPreviewProject,
    handleImportPreviewTicket,
    handleRefreshIntegrationPreview,
    handleSelectIntegrationId,
    handleValidateIntegration,
  };
}
