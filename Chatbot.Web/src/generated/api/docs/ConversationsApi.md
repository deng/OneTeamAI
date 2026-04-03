# ConversationsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**createConversation**](ConversationsApi.md#createconversationoperation) | **POST** /api/concierge-apps/{conciergeAppId}/conversations |  |
| [**getConversation**](ConversationsApi.md#getconversation) | **GET** /api/conversations/{conversationId} |  |
| [**listConversations**](ConversationsApi.md#listconversations) | **GET** /api/concierge-apps/{conciergeAppId}/conversations |  |



## createConversation

> ConversationResponse createConversation(conciergeAppId, createConversationRequest)



### Example

```ts
import {
  Configuration,
  ConversationsApi,
} from '';
import type { CreateConversationOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ConversationsApi(config);

  const body = {
    // string
    conciergeAppId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateConversationRequest
    createConversationRequest: ...,
  } satisfies CreateConversationOperationRequest;

  try {
    const data = await api.createConversation(body);
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
| **conciergeAppId** | `string` |  | [Defaults to `undefined`] |
| **createConversationRequest** | [CreateConversationRequest](CreateConversationRequest.md) |  | |

### Return type

[**ConversationResponse**](ConversationResponse.md)

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


## getConversation

> ConversationDetailResponse getConversation(conversationId)



### Example

```ts
import {
  Configuration,
  ConversationsApi,
} from '';
import type { GetConversationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ConversationsApi(config);

  const body = {
    // string
    conversationId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetConversationRequest;

  try {
    const data = await api.getConversation(body);
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

### Return type

[**ConversationDetailResponse**](ConversationDetailResponse.md)

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


## listConversations

> Array&lt;ConversationSummaryResponse&gt; listConversations(conciergeAppId)



### Example

```ts
import {
  Configuration,
  ConversationsApi,
} from '';
import type { ListConversationsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ConversationsApi(config);

  const body = {
    // string
    conciergeAppId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ListConversationsRequest;

  try {
    const data = await api.listConversations(body);
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
| **conciergeAppId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**Array&lt;ConversationSummaryResponse&gt;**](ConversationSummaryResponse.md)

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

