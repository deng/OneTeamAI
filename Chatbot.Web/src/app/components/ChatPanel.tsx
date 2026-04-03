import type { RefObject } from 'react';
import type { UIMessage } from '@ai-sdk/react';

type ChatPanelProps = {
  isStreaming: boolean;
  selectedProjectId: string;
  selectedProjectName: string;
  selectedCustomerId: string;
  selectedCustomerName: string;
  selectedTicketId: string;
  selectedTicketTitle: string;
  onSelectProjectContext: () => void;
  onSelectCustomerContext: () => void;
  onSelectTicketContext: () => void;
  onStop: () => void;
  scrollRef: RefObject<HTMLDivElement | null>;
  messages: UIMessage[];
  displayedTexts: Record<string, string>;
  input: string;
  onInputChange: (value: string) => void;
  onSubmit: () => void;
  errorMessage: string | null;
  onSyncConversationDraft: () => void;
  onSyncTicketDraft: () => void;
};

export function ChatPanel({
  isStreaming,
  selectedProjectId,
  selectedProjectName,
  selectedCustomerId,
  selectedCustomerName,
  selectedTicketId,
  selectedTicketTitle,
  onSelectProjectContext,
  onSelectCustomerContext,
  onSelectTicketContext,
  onStop,
  scrollRef,
  messages,
  displayedTexts,
  input,
  onInputChange,
  onSubmit,
  errorMessage,
  onSyncConversationDraft,
  onSyncTicketDraft,
}: ChatPanelProps) {
  return (
    <section className="panel panel-chat">
      <div className="chat-header">
        <div>
          <div className="panel-title">对话窗口</div>
          <div className="chat-subtitle">
            {isStreaming
              ? '模型正在生成中...'
              : '可以直接开始提问。当前上下文如下：'}
          </div>
          {!isStreaming ? (
            <div className="entity-chip-list">
              <button
                className="entity-chip entity-chip-button"
                disabled={!selectedProjectId}
                type="button"
                onClick={onSelectProjectContext}
              >
                <strong>项目</strong>
                <span>{selectedProjectName}</span>
              </button>
              <button
                className="entity-chip entity-chip-button"
                disabled={!selectedCustomerId}
                type="button"
                onClick={onSelectCustomerContext}
              >
                <strong>客户</strong>
                <span>{selectedCustomerName}</span>
              </button>
              <button
                className="entity-chip entity-chip-button"
                disabled={!selectedTicketId}
                type="button"
                onClick={onSelectTicketContext}
              >
                <strong>工单</strong>
                <span>{selectedTicketTitle}</span>
              </button>
            </div>
          ) : null}
        </div>

        {isStreaming ? (
          <button className="secondary-button" onClick={onStop} type="button">
            停止生成
          </button>
        ) : null}
      </div>

      <div className="message-list" ref={scrollRef}>
        {messages.length === 0 ? (
          <div className="empty-state">
            <p>还没有消息。</p>
            <span>先在左侧跑通团队、坐台程序与客户工单流，再在这里测试聊天能力。</span>
          </div>
        ) : null}

        {messages.map(message => (
          <article
            className={message.role === 'user' ? 'message user-message' : 'message bot-message'}
            key={message.id}
          >
            <div className="message-role">{message.role === 'user' ? '用户' : '助手'}</div>
            <div className="message-text">
              {displayedTexts[message.id] ?? ''}
              {message.role === 'assistant' && isStreaming && message.id === messages.at(-1)?.id ? (
                <span aria-hidden="true" className="typing-cursor" />
              ) : null}
            </div>
          </article>
        ))}
      </div>

      <form
        className="composer"
        onSubmit={event => {
          event.preventDefault();
          onSubmit();
        }}
      >
        <textarea
          className="composer-input"
          onChange={event => onInputChange(event.currentTarget.value)}
          placeholder="输入你的问题，例如：帮我设计一个客服机器人知识库问答流程。"
          rows={4}
          value={input}
        />

        <div className="composer-actions">
          <span className="hint">
            {errorMessage ? `请求失败: ${errorMessage}` : '聊天走 AI SDK，业务工作台走 OpenAPI 生成客户端'}
          </span>
          <div className="action-row">
            <button className="secondary-button" type="button" onClick={onSyncConversationDraft}>
              同步到会话草稿
            </button>
            <button className="secondary-button" type="button" onClick={onSyncTicketDraft}>
              同步到工单草稿
            </button>
            <button className="primary-button" disabled={isStreaming} type="submit">
              发送消息
            </button>
          </div>
        </div>
      </form>
    </section>
  );
}
