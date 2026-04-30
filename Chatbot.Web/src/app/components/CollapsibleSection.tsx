import { type ReactNode, useCallback, useState } from 'react';

type CollapsibleSectionProps = {
  title: string;
  children: ReactNode;
  defaultExpanded?: boolean;
  storageKey?: string;
};

export function CollapsibleSection({ title, children, defaultExpanded = true, storageKey }: CollapsibleSectionProps) {
  const [expanded, setExpanded] = useState(() => {
    if (storageKey) {
      const stored = window.localStorage.getItem(storageKey);
      if (stored !== null) return stored === 'true';
    }
    return defaultExpanded;
  });

  const toggle = useCallback(() => {
    setExpanded(prev => {
      const next = !prev;
      if (storageKey) window.localStorage.setItem(storageKey, String(next));
      return next;
    });
  }, [storageKey]);

  return (
    <div className="collapsible-section">
      <button className="collapsible-header" type="button" onClick={toggle} aria-expanded={expanded}>
        <span>{title}</span>
        <span className={`collapsible-chevron ${expanded ? '' : 'collapsible-chevron-collapsed'}`} />
      </button>
      {expanded && <div className="collapsible-body">{children}</div>}
    </div>
  );
}
