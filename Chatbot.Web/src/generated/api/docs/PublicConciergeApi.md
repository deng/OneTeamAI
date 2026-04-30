# PublicConciergeApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**createPublicConciergeIntake**](PublicConciergeApi.md#createpublicconciergeintake) | **POST** /api/public/concierge-apps/{conciergeAppId}/intake |  |
| [**getPublicConciergeApp**](PublicConciergeApi.md#getpublicconciergeapp) | **GET** /api/public/concierge-apps/{conciergeAppId} |  |



## createPublicConciergeIntake

> PublicConciergeIntakeResponse createPublicConciergeIntake(conciergeAppId, publicConciergeIntakeRequest)



### Example

```ts
import {
  Configuration,
  PublicConciergeApi,
} from '';
import type { CreatePublicConciergeIntakeRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new PublicConciergeApi(config);

  const body = {
    // string
    conciergeAppId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // PublicConciergeIntakeRequest
    publicConciergeIntakeRequest: ...,
  } satisfies CreatePublicConciergeIntakeRequest;

  try {
    const data = await api.createPublicConciergeIntake(body);
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
| **publicConciergeIntakeRequest** | [PublicConciergeIntakeRequest](PublicConciergeIntakeRequest.md) |  | |

### Return type

[**PublicConciergeIntakeResponse**](PublicConciergeIntakeResponse.md)

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## getPublicConciergeApp

> PublicConciergeAppResponse getPublicConciergeApp(conciergeAppId)



### Example

```ts
import {
  Configuration,
  PublicConciergeApi,
} from '';
import type { GetPublicConciergeAppRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new PublicConciergeApi(config);

  const body = {
    // string
    conciergeAppId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies GetPublicConciergeAppRequest;

  try {
    const data = await api.getPublicConciergeApp(body);
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

[**PublicConciergeAppResponse**](PublicConciergeAppResponse.md)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

