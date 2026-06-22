import {
  useAuthContext,
  useChatContext,
  useResourcesContext,
  useStatusContext,
  useTeamContext,
  WorkspaceProvider,
} from './workspaceContexts';
import { AdminWorkspaceSection } from './components/AdminWorkspaceSection';
import { AuthSection } from './components/AuthSection';
import { BusinessWorkspaceSection } from './components/BusinessWorkspaceSection';
import { ChatPanel } from './components/ChatPanel';
import { CollapsibleSection } from './components/CollapsibleSection';
import { TeamSection } from './components/TeamSection';

function AppWorkspaceInner() {
  const { currentUser } = useAuthContext();
  const { currentTeamId } = useTeamContext();
  const { setChatInput } = useChatContext();

  return (
    <div className="shell">
      <header className="hero">
        <p className="eyebrow">AI Virtual Team Workspace</p>
        <h1>虚拟团队工作台</h1>
        <p className="lede">
          管理团队、项目、客户、工单和会话，与 AI 智能体高效协作。
        </p>
      </header>

      <div className="workspace">
        <aside className="panel panel-side">
          <CollapsibleSection title="账号" storageKey="section-auth" defaultExpanded={!currentUser}>
            <AuthSection />
          </CollapsibleSection>

          <CollapsibleSection title="团队" storageKey="section-team" defaultExpanded={!currentTeamId}>
            <TeamSection />
          </CollapsibleSection>

          {currentUser && currentTeamId ? (
            <CollapsibleSection title="管理" storageKey="section-admin">
              <AdminWorkspaceSection />
            </CollapsibleSection>
          ) : null}

          {currentUser && currentTeamId ? (
            <CollapsibleSection title="业务" storageKey="section-business">
              <BusinessWorkspaceSection />
            </CollapsibleSection>
          ) : null}

          <div className="panel-title panel-title-gap">快捷问题</div>
          <div className="prompt-list">
            {[
              '帮我总结今天的项目进展',
              '给出一个客服机器人欢迎语',
              '解释一下 Microsoft Agent Framework 的作用',
            ].map(prompt => (
              <button
                className="prompt-chip"
                key={prompt}
                type="button"
                onClick={() => setChatInput(prompt)}
              >
                {prompt}
              </button>
            ))}
          </div>
        </aside>

        <ChatPanel />
      </div>
    </div>
  );
}

export function AppWorkspace() {
  return (
    <WorkspaceProvider>
      <AppWorkspaceInner />
    </WorkspaceProvider>
  );
}
