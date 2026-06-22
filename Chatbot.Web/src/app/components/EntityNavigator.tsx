import { useState, useMemo } from 'react';
import type {
  ConciergeAppResponse,
  ConversationSummaryResponse,
  CustomerResponse,
  ProjectResponse,
  TicketResponse,
} from '../../generated/api';

type EntityType = 'project' | 'concierge' | 'customer' | 'conversation' | 'ticket';

type EntityItem = {
  id: string;
  type: EntityType;
  typeLabel: string;
  displayName: string;
  subtitle: string;
};

type EntityNavigatorProps = {
  projects: ProjectResponse[];
  conciergeApps: ConciergeAppResponse[];
  customers: CustomerResponse[];
  conversations: ConversationSummaryResponse[];
  tickets: TicketResponse[];

  selectedProjectId: string;
  selectedConciergeAppId: string;
  selectedCustomerId: string;
  selectedConversationId: string;
  selectedTicketId: string;

  onSelectProject: (id: string) => void;
  onSelectConciergeApp: (id: string) => void;
  onSelectCustomer: (id: string) => void;
  onSelectConversation: (id: string) => void;
  onSelectTicket: (id: string) => void;
};

const TYPE_TABS: { key: EntityType | 'all'; label: string }[] = [
  { key: 'all', label: '全部' },
  { key: 'project', label: '项目' },
  { key: 'concierge', label: '坐台' },
  { key: 'customer', label: '客户' },
  { key: 'conversation', label: '会话' },
  { key: 'ticket', label: '工单' },
];

export function EntityNavigator({
  projects,
  conciergeApps,
  customers,
  conversations,
  tickets,
  selectedProjectId,
  selectedConciergeAppId,
  selectedCustomerId,
  selectedConversationId,
  selectedTicketId,
  onSelectProject,
  onSelectConciergeApp,
  onSelectCustomer,
  onSelectConversation,
  onSelectTicket,
}: EntityNavigatorProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<EntityType | 'all'>('all');

  const displayedEntities = useMemo(() => {
    let entities: EntityItem[] = [];

    if (searchQuery.trim()) {
      for (const p of projects) {
        entities.push({ id: p.id!, type: 'project', typeLabel: '项目', displayName: p.name ?? '', subtitle: '' });
      }
      for (const c of conciergeApps) {
        entities.push({ id: c.id!, type: 'concierge', typeLabel: '坐台程序', displayName: c.name ?? '', subtitle: c.description ?? '' });
      }
      for (const c of customers) {
        entities.push({ id: c.id!, type: 'customer', typeLabel: '客户', displayName: c.displayName ?? '', subtitle: c.email ?? '' });
      }
      for (const c of conversations) {
        entities.push({ id: c.id!, type: 'conversation', typeLabel: '会话', displayName: c.customerName ?? '', subtitle: '' });
      }
      for (const t of tickets) {
        entities.push({ id: t.id!, type: 'ticket', typeLabel: '工单', displayName: t.title ?? '', subtitle: t.customerName ?? '' });
      }
    } else if (selectedCustomerId) {
      for (const c of conversations) {
        if (c.customerId === selectedCustomerId) entities.push({ id: c.id!, type: 'conversation', typeLabel: '会话', displayName: c.customerName ?? '', subtitle: '' });
      }
      for (const t of tickets) {
        if (t.customerId === selectedCustomerId) entities.push({ id: t.id!, type: 'ticket', typeLabel: '工单', displayName: t.title ?? '', subtitle: t.customerName ?? '' });
      }
    } else if (selectedProjectId) {
      for (const c of conciergeApps) {
        if (c.projectId === selectedProjectId) entities.push({ id: c.id!, type: 'concierge', typeLabel: '坐台程序', displayName: c.name ?? '', subtitle: c.description ?? '' });
      }
      for (const c of customers) {
        if (c.projectId === selectedProjectId) entities.push({ id: c.id!, type: 'customer', typeLabel: '客户', displayName: c.displayName ?? '', subtitle: c.email ?? '' });
      }
    } else {
      for (const p of projects) {
        entities.push({ id: p.id!, type: 'project', typeLabel: '项目', displayName: p.name ?? '', subtitle: '' });
      }
    }

    if (activeTab !== 'all') {
      entities = entities.filter(e => e.type === activeTab);
    }

    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase();
      entities = entities.filter(e => e.displayName.toLowerCase().includes(q) || e.subtitle.toLowerCase().includes(q));
    }

    return entities;
  }, [projects, conciergeApps, customers, conversations, tickets, selectedProjectId, selectedCustomerId, searchQuery, activeTab]);

  const groupedResults = useMemo(() => {
    const groups = new Map<EntityType, EntityItem[]>();
    for (const item of displayedEntities) {
      const list = groups.get(item.type) ?? [];
      list.push(item);
      groups.set(item.type, list);
    }
    return groups;
  }, [displayedEntities]);

  const handleSelect = (item: EntityItem) => {
    switch (item.type) {
      case 'project': onSelectProject(item.id); break;
      case 'concierge': onSelectConciergeApp(item.id); break;
      case 'customer': onSelectCustomer(item.id); break;
      case 'conversation': onSelectConversation(item.id); break;
      case 'ticket': onSelectTicket(item.id); break;
    }
  };

  const isSelected = (item: EntityItem): boolean => {
    switch (item.type) {
      case 'project': return item.id === selectedProjectId;
      case 'concierge': return item.id === selectedConciergeAppId;
      case 'customer': return item.id === selectedCustomerId;
      case 'conversation': return item.id === selectedConversationId;
      case 'ticket': return item.id === selectedTicketId;
    }
  };

  return (
    <div className="entity-navigator">
      <input
        className="search-input"
        placeholder="搜索项目、坐台程序、客户、会话、工单..."
        value={searchQuery}
        onChange={e => setSearchQuery(e.target.value)}
      />

      <div className="entity-nav-tabs">
        {TYPE_TABS.map(tab => (
          <button
            key={tab.key}
            className={`entity-nav-tab${activeTab === tab.key ? ' entity-nav-tab-active' : ''}`}
            type="button"
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="entity-nav-breadcrumb">
        {selectedProjectId ? (
          <button className="entity-nav-crumb" type="button" onClick={() => onSelectProject(selectedProjectId)}>
            <span className="entity-nav-crumb-type">项目</span>
            {projects.find(p => p.id === selectedProjectId)?.name ?? '未知'}
          </button>
        ) : null}
        {selectedConciergeAppId ? (
          <>
            <span className="entity-nav-crumb-sep">&rarr;</span>
            <button className="entity-nav-crumb" type="button" onClick={() => onSelectConciergeApp(selectedConciergeAppId)}>
              <span className="entity-nav-crumb-type">坐台</span>
              {conciergeApps.find(c => c.id === selectedConciergeAppId)?.name ?? '未知'}
            </button>
          </>
        ) : null}
        {selectedCustomerId ? (
          <>
            <span className="entity-nav-crumb-sep">&rarr;</span>
            <button className="entity-nav-crumb" type="button" onClick={() => onSelectCustomer(selectedCustomerId)}>
              <span className="entity-nav-crumb-type">客户</span>
              {customers.find(c => c.id === selectedCustomerId)?.displayName ?? '未知'}
            </button>
          </>
        ) : null}
        {selectedConversationId ? (
          <>
            <span className="entity-nav-crumb-sep">&rarr;</span>
            <button className="entity-nav-crumb" type="button" onClick={() => onSelectConversation(selectedConversationId)}>
              <span className="entity-nav-crumb-type">会话</span>
              {conversations.find(c => c.id === selectedConversationId)?.customerName ?? '未知'}
            </button>
          </>
        ) : null}
        {selectedTicketId ? (
          <>
            <span className="entity-nav-crumb-sep">&rarr;</span>
            <button className="entity-nav-crumb" type="button" onClick={() => onSelectTicket(selectedTicketId)}>
              <span className="entity-nav-crumb-type">工单</span>
              {tickets.find(t => t.id === selectedTicketId)?.title ?? '未知'}
            </button>
          </>
        ) : null}
      </div>

      <div className="entity-nav-results">
        {groupedResults.size === 0 ? (
          <span className="entity-placeholder">
            {searchQuery ? '没有匹配结果' : '暂无数据'}
          </span>
        ) : (
          Array.from(groupedResults.entries()).map(([type, items]) => (
            <div key={type} className="entity-nav-group">
              <div className="entity-nav-group-label">
                {TYPE_TABS.find(t => t.key === type)?.label ?? type}
                <span className="entity-nav-count">{items.length}</span>
              </div>
              {items.map(item => (
                <button
                  key={`${item.type}-${item.id}`}
                  className={`entity-chip entity-chip-button${isSelected(item) ? ' entity-chip-selected' : ''}`}
                  type="button"
                  onClick={() => handleSelect(item)}
                >
                  <strong>{item.displayName}</strong>
                  {item.subtitle ? <span>{item.subtitle}</span> : null}
                </button>
              ))}
            </div>
          ))
        )}
      </div>
    </div>
  );
}
