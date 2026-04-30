# ConciergeAppsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**createConciergeApp**](ConciergeAppsApi.md#createconciergeappoperation) | **POST** /api/teams/{teamId}/concierge-apps |  |
| [**listConciergeApps**](ConciergeAppsApi.md#listconciergeapps) | **GET** /api/teams/{teamId}/concierge-apps |  |
| [**updateConciergeApp**](ConciergeAppsApi.md#updateconciergeappoperation) | **PATCH** /api/teams/{teamId}/concierge-apps/{conciergeAppId} |  |



## createConciergeApp

> ConciergeAppResponse createConciergeApp(teamId, createConciergeAppRequest)



### Example

```ts
import {
  Configuration,
  ConciergeAppsApi,
} from '';
import type { CreateConciergeAppOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ConciergeAppsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateConciergeAppRequest
    createConciergeAppRequest: ...,
  } satisfies CreateConciergeAppOperationRequest;

  try {
    const data = await api.createConciergeApp(body);
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
| **createConciergeAppRequest** | [CreateConciergeAppRequest](CreateConciergeAppRequest.md) |  | |

### Return type

[**ConciergeAppResponse**](ConciergeAppResponse.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## listConciergeApps

> Array&lt;ConciergeAppResponse&gt; listConciergeApps(teamId)



### Example

```ts
import {
  Configuration,
  ConciergeAppsApi,
} from '';
import type { ListConciergeAppsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ConciergeAppsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ListConciergeAppsRequest;

  try {
    const data = await api.listConciergeApps(body);
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

[**Array&lt;ConciergeAppResponse&gt;**](ConciergeAppResponse.md)

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


## updateConciergeApp

> ConciergeAppResponse updateConciergeApp(teamId, conciergeAppId, updateConciergeAppRequest)



### Example

```ts
import {
  Configuration,
  ConciergeAppsApi,
} from '';
import type { UpdateConciergeAppOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new ConciergeAppsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    conciergeAppId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // UpdateConciergeAppRequest
    updateConciergeAppRequest: ...,
  } satisfies UpdateConciergeAppOperationRequest;

  try {
    const data = await api.updateConciergeApp(body);
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
| **conciergeAppId** | `string` |  | [Defaults to `undefined`] |
| **updateConciergeAppRequest** | [UpdateConciergeAppRequest](UpdateConciergeAppRequest.md) |  | |

### Return type

[**ConciergeAppResponse**](ConciergeAppResponse.md)

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

