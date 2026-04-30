import {
  AgentExecutionLogStatus,
  AgentWorkflowTriggerMode,
  AgentWorkflowStatus,
  AgentWorkflowStepStatus,
  ConversationParticipantType,
  ConversationStatus,
  CustomerFollowUpStatus,
  CustomerStatus,
  ExternalSystemType,
  InvitationStatus,
  MemberRole,
  MemberType,
  RecordSourceType,
  TicketPriority,
  TicketStatus,
  type MemberResponse,
} from '../generated/api';

export function formatMemberRole(role?: number) {
  switch (role) {
    case MemberRole.NUMBER_0:
      return 'Owner';
    case MemberRole.NUMBER_1:
      return '管理员';
    case MemberRole.NUMBER_2:
      return '执行成员';
    case MemberRole.NUMBER_3:
      return '观察者';
    case MemberRole.NUMBER_4:
      return 'AI 员工';
    default:
      return '未知角色';
  }
}

export function formatMemberType(type?: number) {
  switch (type) {
    case MemberType.NUMBER_0:
      return '真人成员';
    case MemberType.NUMBER_1:
      return 'AI 员工';
    default:
      return '未知类型';
  }
}

export function canEditMemberRole(member: MemberResponse) {
  return member.role !== MemberRole.NUMBER_0 && member.memberType === MemberType.NUMBER_0;
}

export function canRemoveMember(member: MemberResponse) {
  return member.role !== MemberRole.NUMBER_0;
}

export function formatProjectStatus(status?: number) {
  switch (status) {
    case 0:
      return '草稿';
    case 1:
      return '进行中';
    case 2:
      return '暂停中';
    case 3:
      return '已完成';
    case 4:
      return '已归档';
    default:
      return '未知状态';
  }
}

export function formatInvitationStatus(status?: number) {
  switch (status) {
    case InvitationStatus.NUMBER_0:
      return '待接受';
    case InvitationStatus.NUMBER_1:
      return '已接受';
    case InvitationStatus.NUMBER_2:
      return '已撤销';
    case InvitationStatus.NUMBER_3:
      return '已过期';
    default:
      return '未知状态';
  }
}

export function getInvitationStatusClassName(status?: number) {
  switch (status) {
    case InvitationStatus.NUMBER_0:
      return 'status-pill status-pill-pending';
    case InvitationStatus.NUMBER_1:
      return 'status-pill status-pill-success';
    case InvitationStatus.NUMBER_2:
      return 'status-pill status-pill-muted';
    case InvitationStatus.NUMBER_3:
      return 'status-pill status-pill-warning';
    default:
      return 'status-pill';
  }
}

export function formatDateTime(value?: Date | null) {
  if (!value) {
    return '未设置';
  }

  return new Intl.DateTimeFormat('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  }).format(value);
}

export function formatNullableText(value?: string | null, fallback = '未设置') {
  const trimmed = value?.trim();
  return trimmed ? trimmed : fallback;
}

export function formatPreviewText(value?: string | null, maxLength = 180, fallback = '未设置') {
  const trimmed = value?.trim();
  if (!trimmed) {
    return fallback;
  }

  return trimmed.length <= maxLength ? trimmed : `${trimmed.slice(0, maxLength).trimEnd()}...`;
}

export function formatConversationStatus(status?: number) {
  switch (status) {
    case ConversationStatus.NUMBER_0:
      return '进行中';
    case ConversationStatus.NUMBER_1:
      return '已完成';
    case ConversationStatus.NUMBER_2:
      return '已关闭';
    default:
      return '未知状态';
  }
}

export function formatConversationParticipant(type?: number) {
  switch (type) {
    case ConversationParticipantType.NUMBER_0:
      return '客户';
    case ConversationParticipantType.NUMBER_1:
      return 'AI 员工';
    case ConversationParticipantType.NUMBER_2:
      return '真人成员';
    case ConversationParticipantType.NUMBER_3:
      return '系统';
    default:
      return '未知参与方';
  }
}

export function formatTicketStatus(status?: number) {
  switch (status) {
    case TicketStatus.NUMBER_0:
      return '待处理';
    case TicketStatus.NUMBER_1:
      return '处理中';
    case TicketStatus.NUMBER_2:
      return '待确认';
    case TicketStatus.NUMBER_3:
      return '已完成';
    case TicketStatus.NUMBER_4:
      return '已关闭';
    default:
      return '未知状态';
  }
}

export function formatTicketPriority(priority?: number) {
  switch (priority) {
    case TicketPriority.NUMBER_0:
      return '低';
    case TicketPriority.NUMBER_1:
      return '中';
    case TicketPriority.NUMBER_2:
      return '高';
    case TicketPriority.NUMBER_3:
      return '紧急';
    default:
      return '未知优先级';
  }
}

export function formatTicketSlaStatus(
  dueAt?: Date | null,
  resolvedAt?: Date | null,
  status?: number,
) {
  if (!dueAt) {
    return '未设置 SLA';
  }

  if (resolvedAt) {
    return resolvedAt.getTime() <= dueAt.getTime() ? '已按时完成' : '已超时完成';
  }

  if (status === TicketStatus.NUMBER_3 || status === TicketStatus.NUMBER_4) {
    return '已结束';
  }

  return dueAt.getTime() < Date.now() ? '已超时' : '进行中';
}

export function formatCustomerFollowUpStatus(status?: number) {
  switch (status) {
    case CustomerFollowUpStatus.NUMBER_0:
      return '新客户';
    case CustomerFollowUpStatus.NUMBER_1:
      return '跟进中';
    case CustomerFollowUpStatus.NUMBER_2:
      return '已确认';
    case CustomerFollowUpStatus.NUMBER_3:
      return '持续培育';
    case CustomerFollowUpStatus.NUMBER_4:
      return '已流失';
    default:
      return '未知状态';
  }
}

export function formatCustomerStatus(status?: number) {
  switch (status) {
    case CustomerStatus.NUMBER_0:
      return '匿名客户';
    case CustomerStatus.NUMBER_1:
      return '活跃';
    case CustomerStatus.NUMBER_2:
      return '不活跃';
    case CustomerStatus.NUMBER_3:
      return '已屏蔽';
    default:
      return '未知状态';
  }
}

export function formatRecordSourceType(type?: number) {
  switch (type) {
    case RecordSourceType.NUMBER_0:
      return '本地';
    case RecordSourceType.NUMBER_1:
      return '外部';
    case RecordSourceType.NUMBER_2:
      return '混合';
    default:
      return '未知来源';
  }
}

export function formatConciergeStatus(status?: number) {
  switch (status) {
    case 0:
      return '草稿';
    case 1:
      return '已启用';
    case 2:
      return '已停用';
    case 3:
      return '已归档';
    default:
      return '未知状态';
  }
}

export function formatExternalSystemType(type?: number) {
  switch (type) {
    case ExternalSystemType.NUMBER_1:
      return 'Nextcloud';
    case ExternalSystemType.NUMBER_2:
      return 'ERPNext';
    default:
      return '未知系统';
  }
}

export function formatWorkflowStatus(status?: number) {
  switch (status) {
    case AgentWorkflowStatus.NUMBER_0:
      return '已计划';
    case AgentWorkflowStatus.NUMBER_1:
      return '运行中';
    case AgentWorkflowStatus.NUMBER_2:
      return '已完成';
    case AgentWorkflowStatus.NUMBER_3:
      return '失败';
    default:
      return '未知状态';
  }
}

export function formatWorkflowTriggerMode(triggerMode?: number) {
  switch (triggerMode) {
    case AgentWorkflowTriggerMode.NUMBER_0:
      return '手动触发';
    case AgentWorkflowTriggerMode.NUMBER_1:
      return '自动触发';
    default:
      return '未知触发方式';
  }
}

export function formatWorkflowStepStatus(status?: number) {
  switch (status) {
    case AgentWorkflowStepStatus.NUMBER_0:
      return '待执行';
    case AgentWorkflowStepStatus.NUMBER_1:
      return '已完成';
    case AgentWorkflowStepStatus.NUMBER_2:
      return '已跳过';
    case AgentWorkflowStepStatus.NUMBER_3:
      return '失败';
    default:
      return '未知状态';
  }
}

export function formatExecutionLogStatus(status?: number) {
  switch (status) {
    case AgentExecutionLogStatus.NUMBER_0:
      return '待执行';
    case AgentExecutionLogStatus.NUMBER_1:
      return '成功';
    case AgentExecutionLogStatus.NUMBER_2:
      return '被拦截';
    case AgentExecutionLogStatus.NUMBER_3:
      return '失败';
    default:
      return '未知状态';
  }
}

export function formatTicketActivityType(type?: number) {
  switch (type) {
    case 0:
      return '创建';
    case 1:
      return '状态变更';
    case 2:
      return '优先级变更';
    case 3:
      return '负责人变更';
    case 4:
      return '评论';
    case 5:
      return '备注';
    default:
      return '活动';
  }
}

export function formatAuditResult(result?: string | null) {
  switch (result?.toLowerCase()) {
    case 'success':
      return '成功';
    case 'degraded':
      return '降级';
    case 'conflict':
      return '冲突';
    case 'failed':
      return '失败';
    default:
      return formatNullableText(result, '未知结果');
  }
}

export function formatIntegrationAction(actionType?: string | null) {
  switch (actionType) {
    case 'integration.create':
      return '创建连接';
    case 'integration.validate':
      return '校验连接';
    case 'integration.import_customer':
      return '导入客户';
    case 'integration.import_project':
      return '导入项目';
    case 'integration.import_ticket':
      return '导入工单';
    default:
      return formatNullableText(actionType, '集成动作');
  }
}
