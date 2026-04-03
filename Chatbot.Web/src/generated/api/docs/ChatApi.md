# ChatApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**createChatResponse**](ChatApi.md#createchatresponse) | **POST** /api/chat |  |
| [**createChatStream**](ChatApi.md#createchatstream) | **POST** /api/chat/stream |  |
| [**createChatTextStream**](ChatApi.md#createchattextstream) | **POST** /api/chat/text-stream |  |
| [**runAguiAgent**](ChatApi.md#runaguiagent) | **POST** /agui |  |



## createChatResponse

> createChatResponse(chatRequest)



### Example

```ts
import {
  Configuration,
  ChatApi,
} from '';
import type { CreateChatResponseRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ChatApi(config);

  const body = {
    // ChatRequest
    chatRequest: ...,
  } satisfies CreateChatResponseRequest;

  try {
    const data = await api.createChatResponse(body);
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
| **chatRequest** | [ChatRequest](ChatRequest.md) |  | |

### Return type

`void` (Empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## createChatStream

> createChatStream(chatRequest)



### Example

```ts
import {
  Configuration,
  ChatApi,
} from '';
import type { CreateChatStreamRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ChatApi(config);

  const body = {
    // ChatRequest
    chatRequest: ...,
  } satisfies CreateChatStreamRequest;

  try {
    const data = await api.createChatStream(body);
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
| **chatRequest** | [ChatRequest](ChatRequest.md) |  | |

### Return type

`void` (Empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## createChatTextStream

> createChatTextStream(aiSdkChatRequest)



### Example

```ts
import {
  Configuration,
  ChatApi,
} from '';
import type { CreateChatTextStreamRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ChatApi(config);

  const body = {
    // AiSdkChatRequest
    aiSdkChatRequest: ...,
  } satisfies CreateChatTextStreamRequest;

  try {
    const data = await api.createChatTextStream(body);
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
| **aiSdkChatRequest** | [AiSdkChatRequest](AiSdkChatRequest.md) |  | |

### Return type

`void` (Empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## runAguiAgent

> runAguiAgent(runAgentInput)



### Example

```ts
import {
  Configuration,
  ChatApi,
} from '';
import type { RunAguiAgentRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ChatApi(config);

  const body = {
    // RunAgentInput (optional)
    runAgentInput: ...,
  } satisfies RunAguiAgentRequest;

  try {
    const data = await api.runAguiAgent(body);
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
| **runAgentInput** | [RunAgentInput](RunAgentInput.md) |  | [Optional] |

### Return type

`void` (Empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

