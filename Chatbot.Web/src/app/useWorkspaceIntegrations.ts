import { useEffect, useMemo, useState } from 'react';
import {
  ExternalSystemType,
  type CreateIntegrationConnectionRequest,
  type FileKnowledgeItemResponse,
  type IntegrationConnectionHealthResponse,
  type IntegrationConnectionResponse,
  type IntegrationPreviewItemResponse,
} from '../generated/api';
import { createWorkspaceApis } from './workspaceApi';

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
  token,
  runAction,
  setFeedback,
}: {
  currentTeamId: string;
  token: string | null;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [integrationForm, setIntegrationForm] = useState<CreateIntegrationConnectionRequest>(emptyIntegrationForm);
  const [integrationConnections, setIntegrationConnections] = useState<IntegrationConnectionResponse[]>([]);
  const [selectedIntegrationId, setSelectedIntegrationId] = useState('');
  const [selectedIntegrationHealth, setSelectedIntegrationHealth] =
    useState<IntegrationConnectionHealthResponse | null>(null);
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

  async function loadIntegrationPreview(teamId: string, connection: IntegrationConnectionResponse | null) {
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
      const files = await integrationsApi.previewIntegrationFiles({
        teamId,
        connectionId: connection.id,
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
    await loadIntegrationPreview(teamId, nextSelectedConnection);
  }

  useEffect(() => {
    if (!token || !currentTeamId) {
      setIntegrationConnections([]);
      setSelectedIntegrationId('');
      setSelectedIntegrationHealth(null);
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

    const connection =
      integrationConnections.find(item => item.id === integrationId) ?? null;
    setSelectedIntegrationHealth(null);
    await loadIntegrationPreview(currentTeamId, connection);
  }

  return {
    integrationConnections,
    integrationFiles,
    integrationForm,
    integrationPreviewCount,
    integrationPreviewCustomers,
    integrationPreviewProjects,
    integrationPreviewTasks,
    integrationPreviewTickets,
    selectedIntegration,
    selectedIntegrationHealth,
    selectedIntegrationId,
    setIntegrationForm,
    handleCreateIntegration,
    handleSelectIntegrationId,
    handleValidateIntegration,
  };
}
