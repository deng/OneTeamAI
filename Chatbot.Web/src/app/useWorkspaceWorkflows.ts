import { useEffect, useMemo, useState } from 'react';
import {
  AgentWorkflowTriggerMode,
  type AgentWorkflowResponse,
  type MemberResponse,
  type RunTicketWorkflowRequest,
} from '../generated/api';
import type { WorkflowTemplateItem } from './types';
import { createWorkspaceApis, fetchJson, getErrorMessage } from './workspaceApi';

export type WorkflowScope = 'ticket' | 'conversation' | 'project' | null;

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

function emptyWorkflowForm(): RunTicketWorkflowRequest {
  return {
    goal: '',
    startedByMemberId: '',
    triggerMode: AgentWorkflowTriggerMode.NUMBER_0,
  };
}

type RawTemplate = {
  key: string;
  scope: 'ticket' | 'conversation' | 'project';
  label: string;
  goal: string;
  summary: string;
};

export function useWorkspaceWorkflows({
  currentTeamId,
  scope,
  scopeId,
  scopeLabel,
  teamMembers,
  token,
  runAction,
  setFeedback,
}: {
  currentTeamId: string;
  scope: WorkflowScope;
  scopeId: string;
  scopeLabel: string;
  teamMembers: MemberResponse[];
  token: string | null;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [workflowForm, setWorkflowForm] = useState<RunTicketWorkflowRequest>(emptyWorkflowForm);
  const [workflowTemplates, setWorkflowTemplates] = useState<WorkflowTemplateItem[]>([]);
  const [workflows, setWorkflows] = useState<AgentWorkflowResponse[]>([]);
  const [selectedWorkflowId, setSelectedWorkflowId] = useState('');

  const { workflowsApi } = useMemo(() => createWorkspaceApis(token), [token]);
  const aiMembers = teamMembers.filter(member => member.aiProfile);
  const autonomousAiMembers = aiMembers.filter(member => member.aiProfile?.isAutonomous);
  const selectedWorkflow = workflows.find(workflow => workflow.id === selectedWorkflowId) ?? null;
  const currentScopeWorkflowTemplates = scope
    ? workflowTemplates.filter(template => template.scope === scope)
    : [];

  useEffect(() => {
    if (!scope) {
      setWorkflowTemplates([]);
      return;
    }

    let cancelled = false;

    void (async () => {
      const templates = await fetchJson<RawTemplate[]>(`/api/workflow-templates?scope=${scope}`);

      if (!cancelled) {
        setWorkflowTemplates(templates);
      }
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: getErrorMessage(error),
        });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [scope, setFeedback]);

  useEffect(() => {
    if (!token || !currentTeamId || !scope || !scopeId) {
      setWorkflows([]);
      setSelectedWorkflowId('');
      return;
    }

    let cancelled = false;

    void (async () => {
      const queryKey =
        scope === 'ticket' ? 'ticketId' : scope === 'conversation' ? 'conversationId' : 'projectId';
      const nextWorkflows = await fetchJson<AgentWorkflowResponse[]>(
        `/api/teams/${currentTeamId}/workflows?${queryKey}=${scopeId}`,
        {
          headers: {
            Authorization: `Bearer ${token ?? ''}`,
          },
        },
      );

      if (!cancelled) {
        setWorkflows(nextWorkflows);
        setSelectedWorkflowId(current =>
          nextWorkflows.some(workflow => workflow.id === current) ? current : (nextWorkflows[0]?.id ?? ''),
        );
      }
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: getErrorMessage(error),
        });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [currentTeamId, scope, scopeId, setFeedback, token, workflowsApi]);

  async function refreshWorkflows() {
    if (!currentTeamId || !scope || !scopeId) {
      return;
    }

    const queryKey =
      scope === 'ticket' ? 'ticketId' : scope === 'conversation' ? 'conversationId' : 'projectId';
    const nextWorkflows = await fetchJson<AgentWorkflowResponse[]>(
      `/api/teams/${currentTeamId}/workflows?${queryKey}=${scopeId}`,
      {
        headers: {
          Authorization: `Bearer ${token ?? ''}`,
        },
      },
    );
    setWorkflows(nextWorkflows);
    setSelectedWorkflowId(current =>
      nextWorkflows.some(workflow => workflow.id === current) ? current : (nextWorkflows[0]?.id ?? ''),
    );
  }

  async function handleRunWorkflow() {
    if (!currentTeamId || !scope || !scopeId) {
      return;
    }

    const path =
      scope === 'ticket'
        ? `/api/teams/${currentTeamId}/tickets/${scopeId}/workflows`
        : scope === 'conversation'
          ? `/api/teams/${currentTeamId}/conversations/${scopeId}/workflows`
          : `/api/teams/${currentTeamId}/projects/${scopeId}/workflows`;

    await runAction(`run-${scope}-workflow`, async () => {
      await fetchJson<AgentWorkflowResponse>(path, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token ?? ''}`,
        },
        body: JSON.stringify(workflowForm),
      });

      await refreshWorkflows();
      setFeedback({
        kind: 'success',
        text: `${scopeLabel}协作链已启动。`,
      });
    });
  }

  return {
    aiMembers,
    autonomousAiMembers,
    currentScopeWorkflowTemplates,
    currentWorkflowScope: scope,
    currentWorkflowScopeLabel: scopeLabel,
    selectedWorkflow,
    selectedWorkflowId,
    ticketWorkflows: workflows,
    workflowForm,
    setSelectedWorkflowId,
    setWorkflowForm,
    handleRunWorkflow,
  };
}
