import { useEffect, useMemo, useState } from 'react';
import {
  CustomerFollowUpStatus,
  type CreateCustomerRequest,
  type CustomerResponse,
  type ProjectResponse,
  type UpdateCustomerRequest,
} from '../generated/api';
import { createWorkspaceApis, fetchJson } from './workspaceApi';

type RunAction = (name: string, action: () => Promise<void>) => Promise<void>;

function emptyCreateCustomerForm(): CreateCustomerRequest {
  return {
    displayName: '',
    email: '',
    phoneNumber: '',
    companyName: '',
    sourceLabel: '',
    tags: '',
    followUpStatus: CustomerFollowUpStatus.NUMBER_0,
    projectId: '',
    notes: '',
  };
}

function emptyUpdateCustomerForm(): UpdateCustomerRequest {
  return {
    displayName: '',
    email: '',
    phoneNumber: '',
    companyName: '',
    sourceLabel: '',
    tags: '',
    followUpStatus: CustomerFollowUpStatus.NUMBER_0,
    projectId: '',
    notes: '',
  };
}

export function useWorkspaceCustomers({
  currentTeamId,
  projects,
  token,
  runAction,
  setFeedback,
}: {
  currentTeamId: string;
  projects: ProjectResponse[];
  token: string | null;
  runAction: RunAction;
  setFeedback: (value: { kind: 'success' | 'error'; text: string } | null) => void;
}) {
  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [selectedCustomerId, setSelectedCustomerId] = useState('');
  const [createCustomerForm, setCreateCustomerForm] = useState<CreateCustomerRequest>(emptyCreateCustomerForm);
  const [customerUpdateForm, setCustomerUpdateForm] = useState<UpdateCustomerRequest>(emptyUpdateCustomerForm);

  const { customersApi } = useMemo(() => createWorkspaceApis(token), [token]);
  const selectedCustomer = customers.find(customer => customer.id === selectedCustomerId) ?? null;

  useEffect(() => {
    if (!token || !currentTeamId) {
      setCustomers([]);
      setSelectedCustomerId('');
      return;
    }

    let cancelled = false;

    void (async () => {
      const nextCustomers = await customersApi.listCustomers({ teamId: currentTeamId });

      if (cancelled) {
        return;
      }

      setCustomers(nextCustomers);
      setSelectedCustomerId(current =>
        nextCustomers.some(customer => customer.id === current) ? current : (nextCustomers[0]?.id ?? ''),
      );
    })().catch(error => {
      if (!cancelled) {
        setFeedback({
          kind: 'error',
          text: error instanceof Error ? error.message : '客户数据加载失败。',
        });
      }
    });

    return () => {
      cancelled = true;
    };
  }, [currentTeamId, customersApi, setFeedback, token]);

  useEffect(() => {
    if (!selectedCustomer) {
      setCustomerUpdateForm(emptyUpdateCustomerForm());
      return;
    }

    setCustomerUpdateForm({
      displayName: selectedCustomer.displayName ?? '',
      email: selectedCustomer.email ?? '',
      phoneNumber: selectedCustomer.phoneNumber ?? '',
      companyName: selectedCustomer.companyName ?? '',
      sourceLabel: selectedCustomer.sourceLabel ?? '',
      tags: selectedCustomer.tags ?? '',
      followUpStatus: selectedCustomer.followUpStatus ?? CustomerFollowUpStatus.NUMBER_0,
      lastContactedAt: selectedCustomer.lastContactedAt ?? undefined,
      projectId: selectedCustomer.projectId ?? '',
      notes: selectedCustomer.notes ?? '',
      status: selectedCustomer.status,
    });
  }, [selectedCustomer]);

  useEffect(() => {
    if (!createCustomerForm.projectId && projects.length === 1 && projects[0]?.id) {
      setCreateCustomerForm(current => ({ ...current, projectId: projects[0]?.id ?? '' }));
    }
  }, [createCustomerForm.projectId, projects]);

  async function refreshCustomers(teamId: string) {
    const nextCustomers = await customersApi.listCustomers({ teamId });
    setCustomers(nextCustomers);
    setSelectedCustomerId(current =>
      nextCustomers.some(customer => customer.id === current) ? current : (nextCustomers[0]?.id ?? ''),
    );
  }

  async function handleCreateCustomer() {
    if (!currentTeamId) {
      return;
    }

    await runAction('create-customer', async () => {
      const created = await customersApi.createCustomer({
        teamId: currentTeamId,
        createCustomerRequest: createCustomerForm,
      });

      await refreshCustomers(currentTeamId);
      setSelectedCustomerId(created.id ?? '');
      setCreateCustomerForm(emptyCreateCustomerForm());
      setFeedback({
        kind: 'success',
        text: `已创建客户 ${created.displayName ?? ''}。`,
      });
    });
  }

  async function handleSaveCustomer() {
    if (!currentTeamId || !selectedCustomer?.id) {
      return;
    }

    const customerId = selectedCustomer.id;

    await runAction('update-customer', async () => {
      await fetchJson(`/api/teams/${currentTeamId}/customers/${customerId}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token ?? ''}`,
        },
        body: JSON.stringify(customerUpdateForm),
      });

      await refreshCustomers(currentTeamId);
      setFeedback({
        kind: 'success',
        text: `已更新客户 ${customerUpdateForm.displayName ?? ''}。`,
      });
    });
  }

  return {
    createCustomerForm,
    customerUpdateForm,
    customers,
    selectedCustomer,
    selectedCustomerId,
    setCreateCustomerForm,
    setCustomerUpdateForm,
    setSelectedCustomerId,
    handleCreateCustomer,
    handleSaveCustomer,
  };
}
