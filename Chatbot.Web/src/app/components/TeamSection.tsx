import { useEffect, useRef, useState } from 'react';
import { useAuthContext, useStatusContext, useTeamContext } from '../workspaceContexts';
import { Modal } from './Modal';

export function TeamSection() {
  const { busyAction } = useStatusContext();
  const { currentUser } = useAuthContext();
  const {
    currentTeam,
    currentTeamId,
    teamDescription,
    teamName,
    teams,
    handleCreateTeam,
    setCurrentTeamId,
    setTeamDescription,
    setTeamName,
  } = useTeamContext();
  const [showForm, setShowForm] = useState(false);
  const prevBusyRef = useRef(busyAction);
  useEffect(() => {
    if (showForm && prevBusyRef.current === 'create-team' && busyAction === null) {
      setShowForm(false);
    }
    prevBusyRef.current = busyAction;
  }, [busyAction, showForm]);

  return (
    <>
      <div className="panel-title panel-title-gap">团队</div>
      {currentUser ? (
        <div className="setup-stack">
          <div className="form-card">
            <div className="form-card-title">切换团队</div>
            <label className="field">
              <span>当前团队</span>
              <select
                className="text-input"
                value={currentTeamId}
                onChange={e => setCurrentTeamId(e.target.value)}
              >
                <option value="">暂未选择</option>
                {teams.map(team => (
                  <option key={team.id} value={team.id}>
                    {team.name}
                  </option>
                ))}
              </select>
            </label>
            <div className="mini-meta">
              <span>可访问团队数：{teams.length}</span>
              <span>团队说明：{currentTeam?.description ?? '未设置'}</span>
              <span>品牌：{currentTeam?.brandName ?? '未设置'}</span>
            </div>
          </div>

          <button className="secondary-button" type="button" onClick={() => setShowForm(true)}>
            + 创建团队
          </button>

          <Modal open={showForm} onClose={() => setShowForm(false)} title="创建团队">
            <label className="field">
              <span>团队名称</span>
              <input
                className="text-input"
                value={teamName}
                onChange={e => setTeamName(e.target.value)}
              />
            </label>
            <label className="field">
              <span>团队说明</span>
              <textarea
                className="text-area"
                rows={3}
                value={teamDescription}
                onChange={e => setTeamDescription(e.target.value)}
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null}
              type="button"
              onClick={handleCreateTeam}
            >
              {busyAction === 'create-team' ? '创建中...' : '创建团队'}
            </button>
          </Modal>
        </div>
      ) : (
        <span className="entity-placeholder">登录后可以创建和切换团队。</span>
      )}
    </>
  );
}
