import { useEffect, useState } from 'react';
import type { UIMessage } from '@ai-sdk/react';
import { isTextUIPart } from 'ai';

function getMessageText(message: UIMessage) {
  return (
    message.parts
      .filter(isTextUIPart)
      .map(part => part.text)
      .join('') ?? ''
  );
}

export function useTypewriterMessages(messages: UIMessage[], isStreaming: boolean) {
  const [displayedTexts, setDisplayedTexts] = useState<Record<string, string>>({});

  useEffect(() => {
    const timer = window.setInterval(() => {
      setDisplayedTexts(current => {
        let changed = false;
        const next = { ...current };

        for (const message of messages) {
          const fullText = getMessageText(message);

          if (message.role === 'user') {
            if (next[message.id] !== fullText) {
              next[message.id] = fullText;
              changed = true;
            }
            continue;
          }

          const renderedText = next[message.id] ?? '';
          if (renderedText.length >= fullText.length) {
            continue;
          }

          const speed = isStreaming ? 2 : 6;
          next[message.id] = fullText.slice(0, renderedText.length + speed);
          changed = true;
        }

        for (const messageId of Object.keys(next)) {
          if (!messages.some(message => message.id === messageId)) {
            delete next[messageId];
            changed = true;
          }
        }

        return changed ? next : current;
      });
    }, 20);

    return () => window.clearInterval(timer);
  }, [messages, isStreaming]);

  return displayedTexts;
}
