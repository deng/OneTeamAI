# WorkflowsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**getAgentWorkflow**](WorkflowsApi.md#getagentworkflow) | **GET** /api/teams/{teamId}/workflows/{workflowId} |  |
| [**listAgentWorkflows**](WorkflowsApi.md#listagentworkflows) | **GET** /api/teams/{teamId}/workflows |  |
| [**listWorkflowTemplates**](WorkflowsApi.md#listworkflowtemplates) | **GET** /api/workflow-templates |  |
| [**runConversationWorkflow**](WorkflowsApi.md#runconversationworkflow) | **POST** /api/teams/{teamId}/conversations/{conversationId}/workflows |  |
| [**runProjectWorkflow**](WorkflowsApi.md#runprojectworkflow) | **POST** /api/teams/{teamId}/projects/{projectId}/workflows |  |
| [**runTicketWorkflow**](WorkflowsApi.md#runticketworkflowoperation) | **POST** /api/teams/{teamId}/tickets/{ticketId}/workflows |  |



## getAgentWorkflow

> AgentWorkflowResponse getAgentWorkflow(teamId, workflowId)



### Example

```ts
import {
  Configuration,
  WorkflowsApi,
} from '';
import type { GetAgentWorkflowRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new WorkflowsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    workflowId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetAgentWorkflowRequest;

  try {
    const data = await api.getAgentWorkflow(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **teamId** | `string` |  | [Defaults to `undefined`] |
| **workflowId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**AgentWorkflowResponse**](AgentWorkflowResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## listAgentWorkflows

> Array&lt;AgentWorkflowResponse&gt; listAgentWorkflows(teamId, ticketId, conversationId, projectId)



### Example

```ts
import {
  Configuration,
  WorkflowsApi,
} from '';
import type { ListAgentWorkflowsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new WorkflowsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    ticketId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    conversationId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    projectId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ListAgentWorkflowsRequest;

  try {
    const data = await api.listAgentWorkflows(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **teamId** | `string` |  | [Defaults to `undefined`] |
| **ticketId** | `string` |  | [Optional] [Defaults to `undefined`] |
| **conversationId** | `string` |  | [Optional] [Defaults to `undefined`] |
| **projectId** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;AgentWorkflowResponse&gt;**](AgentWorkflowResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## listWorkflowTemplates

> Array&lt;WorkflowTemplateResponse&gt; listWorkflowTemplates(scope)



### Example

```ts
import {
  Configuration,
  WorkflowsApi,
} from '';
import type { ListWorkflowTemplatesRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new WorkflowsApi(config);

  const body = {
    // string (optional)
    scope: scope_example,
  } satisfies ListWorkflowTemplatesRequest;

  try {
    const data = await api.listWorkflowTemplates(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **scope** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;WorkflowTemplateResponse&gt;**](WorkflowTemplateResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **401** | Unauthorized |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## runConversationWorkflow

> AgentWorkflowResponse runConversationWorkflow(teamId, conversationId, runAgentWorkflowRequest)



### Example

```ts
import {
  Configuration,
  WorkflowsApi,
} from '';
import type { RunConversationWorkflowRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new WorkflowsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    conversationId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // RunAgentWorkflowRequest
    runAgentWorkflowRequest: ...,
  } satisfies RunConversationWorkflowRequest;

  try {
    const data = await api.runConversationWorkflow(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **teamId** | `string` |  | [Defaults to `undefined`] |
| **conversationId** | `string` |  | [Defaults to `undefined`] |
| **runAgentWorkflowRequest** | [RunAgentWorkflowRequest](RunAgentWorkflowRequest.md) |  | |

### Return type

[**AgentWorkflowResponse**](AgentWorkflowResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## runProjectWorkflow

> AgentWorkflowResponse runProjectWorkflow(teamId, projectId, runAgentWorkflowRequest)



### Example

```ts
import {
  Configuration,
  WorkflowsApi,
} from '';
import type { RunProjectWorkflowRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new WorkflowsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    projectId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // RunAgentWorkflowRequest
    runAgentWorkflowRequest: ...,
  } satisfies RunProjectWorkflowRequest;

  try {
    const data = await api.runProjectWorkflow(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **teamId** | `string` |  | [Defaults to `undefined`] |
| **projectId** | `string` |  | [Defaults to `undefined`] |
| **runAgentWorkflowRequest** | [RunAgentWorkflowRequest](RunAgentWorkflowRequest.md) |  | |

### Return type

[**AgentWorkflowResponse**](AgentWorkflowResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## runTicketWorkflow

> AgentWorkflowResponse runTicketWorkflow(teamId, ticketId, runTicketWorkflowRequest)



### Example

```ts
import {
  Configuration,
  WorkflowsApi,
} from '';
import type { RunTicketWorkflowOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new WorkflowsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    ticketId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // RunTicketWorkflowRequest
    runTicketWorkflowRequest: ...,
  } satisfies RunTicketWorkflowOperationRequest;

  try {
    const data = await api.runTicketWorkflow(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **teamId** | `string` |  | [Defaults to `undefined`] |
| **ticketId** | `string` |  | [Defaults to `undefined`] |
| **runTicketWorkflowRequest** | [RunTicketWorkflowRequest](RunTicketWorkflowRequest.md) |  | |

### Return type

[**AgentWorkflowResponse**](AgentWorkflowResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

