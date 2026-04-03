import { useRef, useState } from 'react';
import type { UIMessage } from '@ai-sdk/react';
import { useTypewriterMessages } from './useTypewriterMessages';
import { fetchJson, getErrorMessage } from './workspaceApi';

function createMessage(role: 'user' | 'assistant', text: string): UIMessage {
  return {
    id: crypto.randomUUID(),
    role,
    parts: [{ type: 'text', text }] as UIMessage['parts'],
  } as UIMessage;
}

export function useWorkspaceChat() {
  const [chatInput, setChatInput] = useState('');
  const [chatError, setChatError] = useState<string | null>(null);
  const [chatSessionId, setChatSessionId] = useState<string | null>(null);
  const [isStreaming, setIsStreaming] = useState(false);
  const [messages, setMessages] = useState<UIMessage[]>([]);

  const scrollRef = useRef<HTMLDivElement | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const displayedTexts = useTypewriterMessages(messages, isStreaming);

  async function handleSendMessage() {
    const message = chatInput.trim();
    if (!message) {
      return;
    }

    const userMessage = createMessage('user', message);
    const placeholderMessage = createMessage('assistant', '');
    const controller = new AbortController();

    abortRef.current = controller;
    setChatError(null);
    setIsStreaming(true);
    setMessages(current => [...current, userMessage, placeholderMessage]);
    setChatInput('');

    try {
      const result = await fetchJson<{ sessionId: string; message: string }>('/api/chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          sessionId: chatSessionId,
          message,
        }),
        signal: controller.signal,
      });

      setChatSessionId(result.sessionId);
      setMessages(current =>
        current.map(item =>
          item.id === placeholderMessage.id ? createMessage('assistant', result.message ?? '') : item,
        ),
      );
    } catch (error) {
      if (controller.signal.aborted) {
        setMessages(current => current.filter(item => item.id !== placeholderMessage.id));
      } else {
        const nextError = getErrorMessage(error);
        setChatError(nextError);
        setMessages(current =>
          current.map(item =>
            item.id === placeholderMessage.id ? createMessage('assistant', nextError) : item,
          ),
        );
      }
    } finally {
      abortRef.current = null;
      setIsStreaming(false);
    }
  }

  function handleStop() {
    abortRef.current?.abort();
  }

  return {
    chatError,
    chatInput,
    displayedTexts,
    isStreaming,
    messages,
    scrollRef,
    setChatInput,
    handleSendMessage,
    handleStop,
  };
}
