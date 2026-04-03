import {
  AuthApi,
  ConciergeAppsApi,
  Configuration,
  ConversationsApi,
  CustomersApi,
  IntegrationsApi,
  InvitationsApi,
  MembersApi,
  ProjectsApi,
  TicketsApi,
  TeamsApi,
  WorkflowsApi,
  type AuthResponse,
} from '../generated/api';
import { apiBaseUrl, authStorageKey } from './constants';

export function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message;
  }

  return '请求失败，请稍后重试。';
}

export async function fetchJson<T>(path: string, init?: RequestInit) {
  const response = await fetch(`${apiBaseUrl}${path}`, init);
  const rawText = await response.text();
  const data = rawText ? JSON.parse(rawText) : null;

  if (!response.ok) {
    throw new Error(data?.error ?? rawText ?? `请求失败 (${response.status})`);
  }

  return data as T;
}

export function applyAuth(auth: AuthResponse) {
  if (!auth.accessToken) {
    throw new Error('登录响应缺少 accessToken。');
  }

  window.localStorage.setItem(authStorageKey, auth.accessToken);
  return auth.accessToken;
}

export function createWorkspaceApis(token: string | null) {
  const config = new Configuration({
    basePath: apiBaseUrl,
    accessToken: token ?? undefined,
  });

  return {
    authApi: new AuthApi(config),
    teamsApi: new TeamsApi(config),
    membersApi: new MembersApi(config),
    projectsApi: new ProjectsApi(config),
    conciergeAppsApi: new ConciergeAppsApi(config),
    conversationsApi: new ConversationsApi(config),
    customersApi: new CustomersApi(config),
    ticketsApi: new TicketsApi(config),
    invitationsApi: new InvitationsApi(config),
    integrationsApi: new IntegrationsApi(config),
    workflowsApi: new WorkflowsApi(config),
  };
}
