# IntegrationsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**createIntegrationConnection**](IntegrationsApi.md#createintegrationconnectionoperation) | **POST** /api/teams/{teamId}/integrations |  |
| [**listIntegrationConnections**](IntegrationsApi.md#listintegrationconnections) | **GET** /api/teams/{teamId}/integrations |  |
| [**previewIntegrationCustomers**](IntegrationsApi.md#previewintegrationcustomers) | **GET** /api/teams/{teamId}/integrations/{connectionId}/customers |  |
| [**previewIntegrationFiles**](IntegrationsApi.md#previewintegrationfiles) | **GET** /api/teams/{teamId}/integrations/{connectionId}/files |  |
| [**previewIntegrationProjects**](IntegrationsApi.md#previewintegrationprojects) | **GET** /api/teams/{teamId}/integrations/{connectionId}/projects |  |
| [**previewIntegrationTasks**](IntegrationsApi.md#previewintegrationtasks) | **GET** /api/teams/{teamId}/integrations/{connectionId}/tasks |  |
| [**previewIntegrationTickets**](IntegrationsApi.md#previewintegrationtickets) | **GET** /api/teams/{teamId}/integrations/{connectionId}/tickets |  |
| [**validateIntegrationConnection**](IntegrationsApi.md#validateintegrationconnection) | **POST** /api/teams/{teamId}/integrations/{connectionId}/validate |  |



## createIntegrationConnection

> IntegrationConnectionResponse createIntegrationConnection(teamId, createIntegrationConnectionRequest)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { CreateIntegrationConnectionOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateIntegrationConnectionRequest
    createIntegrationConnectionRequest: ...,
  } satisfies CreateIntegrationConnectionOperationRequest;

  try {
    const data = await api.createIntegrationConnection(body);
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
| **createIntegrationConnectionRequest** | [CreateIntegrationConnectionRequest](CreateIntegrationConnectionRequest.md) |  | |

### Return type

[**IntegrationConnectionResponse**](IntegrationConnectionResponse.md)

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
| **409** | Conflict |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## listIntegrationConnections

> Array&lt;IntegrationConnectionResponse&gt; listIntegrationConnections(teamId)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { ListIntegrationConnectionsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ListIntegrationConnectionsRequest;

  try {
    const data = await api.listIntegrationConnections(body);
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

### Return type

[**Array&lt;IntegrationConnectionResponse&gt;**](IntegrationConnectionResponse.md)

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


## previewIntegrationCustomers

> Array&lt;IntegrationPreviewItemResponse&gt; previewIntegrationCustomers(teamId, connectionId)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { PreviewIntegrationCustomersRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    connectionId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies PreviewIntegrationCustomersRequest;

  try {
    const data = await api.previewIntegrationCustomers(body);
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
| **connectionId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**Array&lt;IntegrationPreviewItemResponse&gt;**](IntegrationPreviewItemResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## previewIntegrationFiles

> Array&lt;FileKnowledgeItemResponse&gt; previewIntegrationFiles(teamId, connectionId, folderPath)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { PreviewIntegrationFilesRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    connectionId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    folderPath: folderPath_example,
  } satisfies PreviewIntegrationFilesRequest;

  try {
    const data = await api.previewIntegrationFiles(body);
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
| **connectionId** | `string` |  | [Defaults to `undefined`] |
| **folderPath** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;FileKnowledgeItemResponse&gt;**](FileKnowledgeItemResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## previewIntegrationProjects

> Array&lt;IntegrationPreviewItemResponse&gt; previewIntegrationProjects(teamId, connectionId)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { PreviewIntegrationProjectsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    connectionId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies PreviewIntegrationProjectsRequest;

  try {
    const data = await api.previewIntegrationProjects(body);
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
| **connectionId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**Array&lt;IntegrationPreviewItemResponse&gt;**](IntegrationPreviewItemResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## previewIntegrationTasks

> Array&lt;IntegrationPreviewItemResponse&gt; previewIntegrationTasks(teamId, connectionId)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { PreviewIntegrationTasksRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    connectionId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies PreviewIntegrationTasksRequest;

  try {
    const data = await api.previewIntegrationTasks(body);
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
| **connectionId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**Array&lt;IntegrationPreviewItemResponse&gt;**](IntegrationPreviewItemResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## previewIntegrationTickets

> Array&lt;IntegrationPreviewItemResponse&gt; previewIntegrationTickets(teamId, connectionId)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { PreviewIntegrationTicketsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    connectionId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies PreviewIntegrationTicketsRequest;

  try {
    const data = await api.previewIntegrationTickets(body);
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
| **connectionId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**Array&lt;IntegrationPreviewItemResponse&gt;**](IntegrationPreviewItemResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## validateIntegrationConnection

> IntegrationConnectionHealthResponse validateIntegrationConnection(teamId, connectionId)



### Example

```ts
import {
  Configuration,
  IntegrationsApi,
} from '';
import type { ValidateIntegrationConnectionRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new IntegrationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    connectionId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ValidateIntegrationConnectionRequest;

  try {
    const data = await api.validateIntegrationConnection(body);
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
| **connectionId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**IntegrationConnectionHealthResponse**](IntegrationConnectionHealthResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

