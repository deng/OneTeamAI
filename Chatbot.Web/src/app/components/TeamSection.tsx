import { formatNullableText } from '../formatters';
import type { TeamSummaryResponse, UserResponse } from '../../generated/api';

type TeamSectionProps = {
  busyAction: string | null;
  currentTeam: TeamSummaryResponse | null;
  currentTeamId: string;
  currentUser: UserResponse | null;
  teamDescription: string;
  teamName: string;
  teams: TeamSummaryResponse[];
  onCreateTeam: () => void;
  onCurrentTeamIdChange: (teamId: string) => void;
  onTeamDescriptionChange: (value: string) => void;
  onTeamNameChange: (value: string) => void;
};

export function TeamSection({
  busyAction,
  currentTeam,
  currentTeamId,
  currentUser,
  teamDescription,
  teamName,
  teams,
  onCreateTeam,
  onCurrentTeamIdChange,
  onTeamDescriptionChange,
  onTeamNameChange,
}: TeamSectionProps) {
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
                onChange={event => onCurrentTeamIdChange(event.currentTarget.value)}
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
              <span>团队说明：{formatNullableText(currentTeam?.description, '未设置')}</span>
              <span>品牌：{formatNullableText(currentTeam?.brandName, '未设置')}</span>
            </div>
          </div>

          <div className="form-card">
            <div className="form-card-title">创建团队</div>
            <label className="field">
              <span>团队名称</span>
              <input
                className="text-input"
                value={teamName}
                onChange={event => onTeamNameChange(event.currentTarget.value)}
              />
            </label>
            <label className="field">
              <span>团队说明</span>
              <textarea
                className="text-area"
                rows={3}
                value={teamDescription}
                onChange={event => onTeamDescriptionChange(event.currentTarget.value)}
              />
            </label>
            <button
              className="secondary-button"
              disabled={busyAction !== null}
              type="button"
              onClick={onCreateTeam}
            >
              {busyAction === 'create-team' ? '创建中...' : '创建团队'}
            </button>
          </div>
        </div>
      ) : (
        <span className="entity-placeholder">登录后可以创建和切换团队。</span>
      )}
    </>
  );
}
