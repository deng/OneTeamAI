import { TicketPriority } from '../../generated/api';
import { formatNullableText } from '../formatters';
import { usePublicConcierge } from '../usePublicConcierge';

type PublicConciergePageProps = {
  appId: string;
};

export function PublicConciergePage({ appId }: PublicConciergePageProps) {
  const { busyAction, conciergeApp, feedback, intakeForm, setIntakeForm, handleSubmitIntake } =
    usePublicConcierge(appId);

  return (
    <div className="public-shell">
      <section className="public-hero panel">
        <p className="eyebrow">Customer Intake</p>
        <h1>{conciergeApp?.name ?? '客户入口'}</h1>
        <p className="lede">
          {conciergeApp?.welcomeMessage
            ?? '请留下你的需求，我们会自动帮你整理会话，并在需要时生成工单。'}
        </p>
        <div className="public-meta-grid">
          <div className="mini-meta">
            <strong>品牌</strong>
            <span>{formatNullableText(conciergeApp?.teamBrandName, '未设置')}</span>
          </div>
          <div className="mini-meta">
            <strong>项目</strong>
            <span>{formatNullableText(conciergeApp?.projectName, '未绑定')}</span>
          </div>
          <div className="mini-meta">
            <strong>服务范围</strong>
            <span>{formatNullableText(conciergeApp?.serviceScope, '未设置')}</span>
          </div>
          <div className="mini-meta">
            <strong>服务时间</strong>
            <span>{formatNullableText(conciergeApp?.businessHours, '未设置')}</span>
          </div>
          <div className="mini-meta">
            <strong>资料要求</strong>
            <span>
              邮箱 {conciergeApp?.requireEmail ? '必填' : '选填'} · 手机 {conciergeApp?.requirePhoneNumber ? '必填' : '选填'}
            </span>
          </div>
        </div>
      </section>

      <section className="public-layout">
        <div className="panel panel-side public-side">
          <div className="panel-title">提交需求</div>
          <div className="setup-stack">
            <div className="form-card">
              <label className="field">
                <span>你的姓名</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null}
                  value={intakeForm.displayName}
                  onChange={event =>
                    setIntakeForm(current => ({ ...current, displayName: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="field">
                <span>邮箱</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null}
                  placeholder={conciergeApp?.requireEmail ? '必填' : '选填'}
                  value={intakeForm.email}
                  onChange={event =>
                    setIntakeForm(current => ({ ...current, email: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="field">
                <span>手机号</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null}
                  placeholder={conciergeApp?.requirePhoneNumber ? '必填' : '选填'}
                  value={intakeForm.phoneNumber}
                  onChange={event =>
                    setIntakeForm(current => ({ ...current, phoneNumber: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="field">
                <span>公司或品牌</span>
                <input
                  className="text-input"
                  disabled={busyAction !== null}
                  value={intakeForm.companyName}
                  onChange={event =>
                    setIntakeForm(current => ({ ...current, companyName: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="field">
                <span>需求内容</span>
                <textarea
                  className="text-area"
                  disabled={busyAction !== null}
                  rows={6}
                  value={intakeForm.message}
                  onChange={event =>
                    setIntakeForm(current => ({ ...current, message: event.currentTarget.value }))
                  }
                />
              </label>
              <label className="checkbox-field">
                <input
                  checked={intakeForm.autoCreateTicket}
                  type="checkbox"
                  onChange={event =>
                    setIntakeForm(current => ({ ...current, autoCreateTicket: event.currentTarget.checked }))
                  }
                />
                自动创建工单
              </label>
              <label className="field">
                <span>工单优先级</span>
                <select
                  className="text-input"
                  disabled={busyAction !== null || !intakeForm.autoCreateTicket}
                  value={intakeForm.autoTicketPriority}
                  onChange={event =>
                    setIntakeForm(current => ({
                      ...current,
                      autoTicketPriority: Number(event.currentTarget.value) as TicketPriority,
                    }))
                  }
                >
                  <option value={TicketPriority.NUMBER_0}>低</option>
                  <option value={TicketPriority.NUMBER_1}>中</option>
                  <option value={TicketPriority.NUMBER_2}>高</option>
                  <option value={TicketPriority.NUMBER_3}>紧急</option>
                </select>
              </label>
              <button
                className="primary-button"
                disabled={busyAction !== null}
                type="button"
                onClick={() => {
                  void handleSubmitIntake();
                }}
              >
                {busyAction === 'submit-public-intake' ? '提交中...' : '提交需求'}
              </button>
            </div>

            {feedback ? (
              <div className={feedback.kind === 'error' ? 'status-box status-box-error' : 'status-box'}>
                <strong>{feedback.kind === 'error' ? '提交失败' : '提交成功'}</strong>
                <span>{feedback.text}</span>
              </div>
            ) : null}
          </div>
        </div>

        <section className="panel public-content">
          <div className="panel-title">接待说明</div>
          <div className="public-copy">
            <div className="mini-meta">
              <strong>填写指引</strong>
              <span>{formatNullableText(conciergeApp?.intakeGuidance, '请尽量描述清楚你的问题、目标和期望结果。')}</span>
            </div>
            <div className="mini-meta">
              <strong>FAQ 范围</strong>
              <span>{formatNullableText(conciergeApp?.faqScope, '暂未设置')}</span>
            </div>
            <div className="mini-meta">
              <strong>渠道</strong>
              <span>{formatNullableText(conciergeApp?.channelLabel, '默认入口')}</span>
            </div>
            <div className="mini-meta">
              <strong>补充说明</strong>
              <span>{formatNullableText(conciergeApp?.description, '暂无附加说明')}</span>
            </div>
            {conciergeApp?.suggestedPrompts?.trim() ? (
              <div className="mini-meta">
                <strong>建议这样提问</strong>
                <div className="entity-chip-list">
                  {conciergeApp.suggestedPrompts
                    .split(/\r?\n|[;；]/)
                    .map(prompt => prompt.trim())
                    .filter(Boolean)
                    .slice(0, 6)
                    .map(prompt => (
                      <button
                        className="entity-chip entity-chip-button"
                        key={prompt}
                        type="button"
                        onClick={() =>
                          setIntakeForm(current => ({
                            ...current,
                            message: current.message.trim() ? current.message : prompt,
                          }))
                        }
                      >
                        {prompt}
                      </button>
                    ))}
                </div>
              </div>
            ) : null}
          </div>
        </section>
      </section>
    </div>
  );
}
