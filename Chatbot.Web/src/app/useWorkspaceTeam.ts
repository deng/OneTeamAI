import { useEffect, useMemo, useState } from 'react';
import type { TeamSummaryResponse } from '../generated/api';
import { currentTeamStorageKey } from './constants';
import { createWorkspaceApis } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

export function useWorkspaceTeam({
  token,
  runAction,
  setFeedback,
}: {
  token: string | null;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [teams, setTeams] = useState<TeamSummaryResponse[]>([]);
  const [currentTeamId, setCurrentTeamId] = useState<string>(
    () => window.localStorage.getItem(currentTeamStorageKey) ?? '',
  );
  const [teamName, setTeamName] = useState('');
  const [teamDescription, setTeamDescription] = useState('');

  const { teamsApi } = useMemo(() => createWorkspaceApis(token), [token]);

  const currentTeam = teams.find(team => team.id === currentTeamId) ?? null;

  useEffect(() => {
    window.localStorage.setItem(currentTeamStorageKey, currentTeamId);
  }, [currentTeamId]);

  useEffect(() => {
    if (!token) {
      setTeams([]);
      setCurrentTeamId('');
      return;
    }

    let cancelled = false;

    void (async () => {
      const nextTeams = await teamsApi.listMyTeams();

      if (cancelled) {
        return;
      }

      setTeams(nextTeams);
      if (nextTeams.length === 0) {
        setCurrentTeamId('');
        return;
      }

      const hasExisting = nextTeams.some(team => team.id === currentTeamId);
      if (!hasExisting) {
        setCurrentTeamId(nextTeams[0].id ?? '');
      }
    })().catch(() => {
      if (!cancelled) {
        setTeams([]);
        setCurrentTeamId('');
      }
    });

    return () => {
      cancelled = true;
    };
  }, [currentTeamId, teamsApi, token]);

  async function refreshTeams() {
    if (!token) {
      return;
    }

    const nextTeams = await teamsApi.listMyTeams();
    setTeams(nextTeams);

    if (!currentTeamId && nextTeams.length > 0) {
      setCurrentTeamId(nextTeams[0].id ?? '');
    }
  }

  async function handleCreateTeam() {
    if (!token) {
      setFeedback({
        kind: 'error',
        text: '请先登录。',
      });
      return;
    }

    await runAction('create-team', async () => {
      const created = await teamsApi.createTeam({
        createTeamRequest: {
          name: teamName,
          description: teamDescription,
        },
      });

      await refreshTeams();
      setCurrentTeamId(created.id ?? '');
      setTeamName('');
      setTeamDescription('');
      setFeedback({
        kind: 'success',
        text: `已创建团队 ${created.name ?? ''}。`,
      });
    });
  }

  return {
    currentTeam,
    currentTeamId,
    refreshTeams,
    teamDescription,
    teamName,
    teams,
    setCurrentTeamId,
    setTeamDescription,
    setTeamName,
    handleCreateTeam,
  };
}
