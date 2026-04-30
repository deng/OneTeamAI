import { useEffect, useMemo, useState } from 'react';
import type {
  ConciergeAppResponse,
  CreateConciergeAppRequest,
  CreateProjectRequest,
  MemberResponse,
  ProjectResponse,
  UpdateConciergeAppRequest,
  UpdateProjectRequest,
} from '../generated/api';
import { createWorkspaceApis, fetchJson } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

function emptyProjectUpdateForm(): UpdateProjectRequest {
  return {
    name: '',
    description: '',
    stageLabel: '',
    summary: '',
    riskSummary: '',
    nextSteps: '',
    leadMemberId: '',
    participantMemberIds: [],
  };
}

function emptyConciergeUpdateForm(): UpdateConciergeAppRequest {
  return {
    name: '',
    description: '',
    serviceScope: '',
    welcomeMessage: '',
    faqScope: '',
    intakeGuidance: '',
    suggestedPrompts: '',
    businessHours: '',
    channelLabel: '',
    requireEmail: false,
    requirePhoneNumber: false,
    primaryAiMemberId: '',
    ticketCreationPolicy: '',
    humanHandoffPolicy: '',
    status: 0,
  };
}

export function useWorkspaceResources({
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
  const [teamMembers, setTeamMembers] = useState<MemberResponse[]>([]);
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [conciergeApps, setConciergeApps] = useState<ConciergeAppResponse[]>([]);
  const [selectedProjectId, setSelectedProjectId] = useState('');
  const [selectedConciergeAppId, setSelectedConciergeAppId] = useState('');
  const [isResourcesLoading, setIsResourcesLoading] = useState(false);
  const [resourcesError, setResourcesError] = useState<string | null>(null);
  const [createProjectForm, setCreateProjectForm] = useState<CreateProjectRequest>({
    name: '',
    description: '',
    stageLabel: '',
    summary: '',
  });
  const [projectUpdateForm, setProjectUpdateForm] = useState<UpdateProjectRequest>(emptyProjectUpdateForm);
  const [createConciergeForm, setCreateConciergeForm] = useState<CreateConciergeAppRequest>({
    projectId: '',
    name: '',
    description: '',
    serviceScope: '',
    welcomeMessage: '',
    faqScope: '',
    intakeGuidance: '',
    suggestedPrompts: '',
    businessHours: '',
    channelLabel: '',
    requireEmail: false,
    requirePhoneNumber: false,
    primaryAiMemberId: '',
    ticketCreationPolicy: '',
    humanHandoffPolicy: '',
  });
  const [conciergeUpdateForm, setConciergeUpdateForm] =
    useState<UpdateConciergeAppRequest>(emptyConciergeUpdateForm);

  const { membersApi, projectsApi, conciergeAppsApi } = useMemo(() => createWorkspaceApis(token), [token]);

  const selectedProject = projects.find(project => project.id === selectedProjectId) ?? null;
  const selectedConciergeApp = conciergeApps.find(app => app.id === selectedConciergeAppId) ?? null;
  const selectedProjectParticipants = teamMembers.filter(member =>
    selectedProject?.participantMemberIds?.includes(member.id ?? ''),
  );

  useEffect(() => {
    if (!token || !currentTeamId) {
      setTeamMembers([]);
      setProjects([]);
      setConciergeApps([]);
      setSelectedProjectId('');
      setSelectedConciergeAppId('');
      return;
    }

    let cancelled = false;

    setIsResourcesLoading(true);
    setResourcesError(null);

    void (async () => {
      try {
        const [members, nextProjects, nextConciergeApps] = await Promise.all([
          membersApi.listTeamMembers({ teamId: currentTeamId }),
          projectsApi.listProjects({ teamId: currentTeamId }),
          conciergeAppsApi.listConciergeApps({ teamId: currentTeamId }),
        ]);

        if (cancelled) {
          return;
        }

        setTeamMembers(members);
        setProjects(nextProjects);
        setConciergeApps(nextConciergeApps);
        setSelectedProjectId(current =>
          nextProjects.some(project => project.id === current) ? current : (nextProjects[0]?.id ?? ''),
        );
        setSelectedConciergeAppId(current =>
          nextConciergeApps.some(app => app.id === current) ? current : (nextConciergeApps[0]?.id ?? ''),
        );
      } catch (error) {
        if (!cancelled) {
          setResourcesError(error instanceof Error ? error.message : '工作台数据加载失败。');
          setFeedback({
            kind: 'error',
            text: error instanceof Error ? error.message : '工作台数据加载失败。',
          });
        }
      } finally {
        if (!cancelled) {
          setIsResourcesLoading(false);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [conciergeAppsApi, currentTeamId, membersApi, projectsApi, setFeedback, token]);

  useEffect(() => {
    if (!selectedProject) {
      setProjectUpdateForm(emptyProjectUpdateForm());
      return;
    }

    setProjectUpdateForm({
      name: selectedProject.name ?? '',
      description: selectedProject.description ?? '',
      stageLabel: selectedProject.stageLabel ?? '',
      summary: selectedProject.summary ?? '',
      riskSummary: selectedProject.riskSummary ?? '',
      nextSteps: selectedProject.nextSteps ?? '',
      leadMemberId: selectedProject.leadMemberId ?? '',
      participantMemberIds: selectedProject.participantMemberIds ?? [],
    });
  }, [selectedProject]);

  useEffect(() => {
    if (!selectedConciergeApp) {
      setConciergeUpdateForm(emptyConciergeUpdateForm());
      return;
    }

    setConciergeUpdateForm({
      name: selectedConciergeApp.name ?? '',
      description: selectedConciergeApp.description ?? '',
      serviceScope: selectedConciergeApp.serviceScope ?? '',
      welcomeMessage: selectedConciergeApp.welcomeMessage ?? '',
      faqScope: selectedConciergeApp.faqScope ?? '',
      intakeGuidance: selectedConciergeApp.intakeGuidance ?? '',
      suggestedPrompts: selectedConciergeApp.suggestedPrompts ?? '',
      businessHours: selectedConciergeApp.businessHours ?? '',
      channelLabel: selectedConciergeApp.channelLabel ?? '',
      requireEmail: selectedConciergeApp.requireEmail ?? false,
      requirePhoneNumber: selectedConciergeApp.requirePhoneNumber ?? false,
      primaryAiMemberId: selectedConciergeApp.primaryAiMemberId ?? '',
      ticketCreationPolicy: selectedConciergeApp.ticketCreationPolicy ?? '',
      humanHandoffPolicy: selectedConciergeApp.humanHandoffPolicy ?? '',
      status: selectedConciergeApp.status ?? 0,
    });
  }, [selectedConciergeApp]);

  async function refreshWorkspaceData(teamId: string) {
    const [members, nextProjects, nextConciergeApps] = await Promise.all([
      membersApi.listTeamMembers({ teamId }),
      projectsApi.listProjects({ teamId }),
      conciergeAppsApi.listConciergeApps({ teamId }),
    ]);

    setTeamMembers(members);
    setProjects(nextProjects);
    setConciergeApps(nextConciergeApps);
    setSelectedProjectId(current =>
      nextProjects.some(project => project.id === current) ? current : (nextProjects[0]?.id ?? ''),
    );
    setSelectedConciergeAppId(current =>
      nextConciergeApps.some(app => app.id === current) ? current : (nextConciergeApps[0]?.id ?? ''),
    );
  }

  async function handleCreateProject() {
    if (!currentTeamId) {
      return;
    }

    await runAction('create-project', async () => {
      const created = await projectsApi.createProject({
        teamId: currentTeamId,
        createProjectRequest: createProjectForm,
      });

      await refreshWorkspaceData(currentTeamId);
      setSelectedProjectId(created.id ?? '');
      setCreateProjectForm({
        name: '',
        description: '',
        stageLabel: '',
        summary: '',
      });
      setFeedback({
        kind: 'success',
        text: `已创建项目 ${created.name ?? ''}。`,
      });
    });
  }

  async function handleSaveProject() {
    if (!currentTeamId || !selectedProject?.id) {
      return;
    }

    await runAction('update-project', async () => {
      await fetchJson(`/api/teams/${currentTeamId}/projects/${selectedProject.id}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token ?? ''}`,
        },
        body: JSON.stringify(projectUpdateForm),
      });

      await refreshWorkspaceData(currentTeamId);
      setFeedback({
        kind: 'success',
        text: `已更新项目 ${projectUpdateForm.name ?? ''}。`,
      });
    });
  }

  async function handleCreateConciergeApp() {
    if (!currentTeamId) {
      return;
    }

    await runAction('create-concierge', async () => {
      const created = await conciergeAppsApi.createConciergeApp({
        teamId: currentTeamId,
        createConciergeAppRequest: createConciergeForm,
      });

      await refreshWorkspaceData(currentTeamId);
      setSelectedConciergeAppId(created.id ?? '');
      setCreateConciergeForm(current => ({
        ...current,
        name: '',
        description: '',
        serviceScope: '',
        welcomeMessage: '',
        faqScope: '',
        intakeGuidance: '',
        suggestedPrompts: '',
        businessHours: '',
        channelLabel: '',
        requireEmail: false,
        requirePhoneNumber: false,
        primaryAiMemberId: '',
        ticketCreationPolicy: '',
        humanHandoffPolicy: '',
      }));
      setFeedback({
        kind: 'success',
        text: `已创建坐台程序 ${created.name ?? ''}。`,
      });
    });
  }

  async function handleSaveConciergeApp() {
    if (!currentTeamId || !selectedConciergeApp?.id) {
      return;
    }

    const conciergeAppId = selectedConciergeApp.id;

    await runAction('update-concierge', async () => {
      await conciergeAppsApi.updateConciergeApp({
        teamId: currentTeamId,
        conciergeAppId,
        updateConciergeAppRequest: conciergeUpdateForm,
      });

      await refreshWorkspaceData(currentTeamId);
      setFeedback({
        kind: 'success',
        text: `已更新坐台程序 ${conciergeUpdateForm.name ?? ''}。`,
      });
    });
  }

  return {
    conciergeApps,
    conciergeUpdateForm,
    createConciergeForm,
    createProjectForm,
    isResourcesLoading,
    resourcesError,
    refreshWorkspaceData,
    projectUpdateForm,
    projects,
    selectedConciergeApp,
    selectedConciergeAppId,
    selectedProject,
    selectedProjectId,
    selectedProjectParticipants,
    teamMembers,
    setConciergeUpdateForm,
    setCreateConciergeForm,
    setCreateProjectForm,
    setProjectUpdateForm,
    setSelectedConciergeAppId,
    setSelectedProjectId,
    handleCreateConciergeApp,
    handleCreateProject,
    handleSaveConciergeApp,
    handleSaveProject,
  };
}
