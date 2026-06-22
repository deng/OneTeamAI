import { useAuthContext, useChatContext, useCustomersContext, useConversationsContext, useNavigationContext, useResourcesContext, useStatusContext, useTicketsContext } from '../workspaceContexts';

export function ChatPanel() {
  const { chatError, chatInput, displayedTexts, isStreaming, messages, scrollRef, setChatInput, handleSendMessage, handleStop } = useChatContext();
  const { selectedProjectId, selectedProject } = useResourcesContext();
  const { selectedCustomerId, selectedCustomer } = useCustomersContext();
  const { selectedTicketId, selectedTicket } = useTicketsContext();
  const { navigateToProject, navigateToCustomer, navigateToTicket } = useNavigationContext();
  const { setCreateConversationForm } = useConversationsContext();
  const { setCreateTicketForm } = useTicketsContext();
  const { setFeedback } = useStatusContext();

  const selectedProjectName = selectedProject?.name ?? '未选项目';
  const selectedCustomerName = selectedCustomer?.displayName ?? '未选客户';
  const selectedTicketTitle = selectedTicket?.title ?? '未选工单';

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
                onClick={() => selectedProjectId && navigateToProject(selectedProjectId)}
              >
                <strong>项目</strong>
                <span>{selectedProjectName}</span>
              </button>
              <button
                className="entity-chip entity-chip-button"
                disabled={!selectedCustomerId}
                type="button"
                onClick={() => selectedCustomerId && navigateToCustomer(selectedCustomerId)}
              >
                <strong>客户</strong>
                <span>{selectedCustomerName}</span>
              </button>
              <button
                className="entity-chip entity-chip-button"
                disabled={!selectedTicketId}
                type="button"
                onClick={() => selectedTicketId && navigateToTicket(selectedTicketId)}
              >
                <strong>工单</strong>
                <span>{selectedTicketTitle}</span>
              </button>
            </div>
          ) : null}
        </div>

        {isStreaming ? (
          <button className="secondary-button" onClick={handleStop} type="button">
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
          void handleSendMessage();
        }}
      >
        <textarea
          className="composer-input"
          onChange={event => setChatInput(event.target.value)}
          placeholder="输入你的问题，例如：帮我设计一个客服机器人知识库问答流程。"
          rows={4}
          value={chatInput}
        />

        <div className="composer-actions">
          <span className="hint">
            {chatError ? `请求失败: ${chatError}` : '聊天走 AI SDK，业务工作台走 OpenAPI 生成客户端'}
          </span>
          <div className="action-row">
            <button className="secondary-button" type="button" onClick={() => {
              const draftMessage = chatInput.trim();
              if (!draftMessage) {
                setFeedback({ kind: 'error', text: '先输入一段聊天内容，再同步到会话草稿。' });
                return;
              }
              setCreateConversationForm(current => ({
                ...current,
                customerId: current.customerId || (selectedCustomer?.id ?? ''),
                customerDisplayName: current.customerDisplayName || (selectedCustomer?.displayName ?? ''),
                customerEmail: current.customerEmail || (selectedCustomer?.email ?? ''),
                initialMessage: draftMessage,
              }));
              setFeedback({ kind: 'success', text: '已把聊天内容同步到会话草稿。' });
            }}>
              同步到会话草稿
            </button>
            <button className="secondary-button" type="button" onClick={() => {
              const draftMessage = chatInput.trim();
              if (!draftMessage) {
                setFeedback({ kind: 'error', text: '先输入一段聊天内容，再同步到工单草稿。' });
                return;
              }
              setCreateTicketForm(current => ({
                ...current,
                title: current.title || `${selectedCustomer?.displayName ?? '客户'}需求`,
                summary: draftMessage,
              }));
              setFeedback({ kind: 'success', text: '已把聊天内容同步到工单草稿。' });
            }}>
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
