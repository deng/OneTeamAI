# TicketsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**addTicketComment**](TicketsApi.md#addticketcommentoperation) | **POST** /api/teams/{teamId}/tickets/{ticketId}/comments |  |
| [**createTicket**](TicketsApi.md#createticketoperation) | **POST** /api/conversations/{conversationId}/tickets |  |
| [**getTicket**](TicketsApi.md#getticket) | **GET** /api/teams/{teamId}/tickets/{ticketId} |  |
| [**listTickets**](TicketsApi.md#listtickets) | **GET** /api/teams/{teamId}/tickets |  |
| [**updateTicket**](TicketsApi.md#updateticketoperation) | **PATCH** /api/teams/{teamId}/tickets/{ticketId} |  |



## addTicketComment

> TicketActivityResponse addTicketComment(teamId, ticketId, addTicketCommentRequest)



### Example

```ts
import {
  Configuration,
  TicketsApi,
} from '';
import type { AddTicketCommentOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new TicketsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    ticketId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // AddTicketCommentRequest
    addTicketCommentRequest: ...,
  } satisfies AddTicketCommentOperationRequest;

  try {
    const data = await api.addTicketComment(body);
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
| **addTicketCommentRequest** | [AddTicketCommentRequest](AddTicketCommentRequest.md) |  | |

### Return type

[**TicketActivityResponse**](TicketActivityResponse.md)

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


## createTicket

> TicketResponse createTicket(conversationId, createTicketRequest)



### Example

```ts
import {
  Configuration,
  TicketsApi,
} from '';
import type { CreateTicketOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new TicketsApi(config);

  const body = {
    // string
    conversationId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateTicketRequest
    createTicketRequest: ...,
  } satisfies CreateTicketOperationRequest;

  try {
    const data = await api.createTicket(body);
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
| **conversationId** | `string` |  | [Defaults to `undefined`] |
| **createTicketRequest** | [CreateTicketRequest](CreateTicketRequest.md) |  | |

### Return type

[**TicketResponse**](TicketResponse.md)

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


## getTicket

> TicketDetailResponse getTicket(teamId, ticketId)



### Example

```ts
import {
  Configuration,
  TicketsApi,
} from '';
import type { GetTicketRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new TicketsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    ticketId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetTicketRequest;

  try {
    const data = await api.getTicket(body);
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

### Return type

[**TicketDetailResponse**](TicketDetailResponse.md)

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


## listTickets

> Array&lt;TicketResponse&gt; listTickets(teamId)



### Example

```ts
import {
  Configuration,
  TicketsApi,
} from '';
import type { ListTicketsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new TicketsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ListTicketsRequest;

  try {
    const data = await api.listTickets(body);
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

[**Array&lt;TicketResponse&gt;**](TicketResponse.md)

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


## updateTicket

> TicketResponse updateTicket(teamId, ticketId, updateTicketRequest)



### Example

```ts
import {
  Configuration,
  TicketsApi,
} from '';
import type { UpdateTicketOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new TicketsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    ticketId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // UpdateTicketRequest
    updateTicketRequest: ...,
  } satisfies UpdateTicketOperationRequest;

  try {
    const data = await api.updateTicket(body);
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
| **updateTicketRequest** | [UpdateTicketRequest](UpdateTicketRequest.md) |  | |

### Return type

[**TicketResponse**](TicketResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
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

