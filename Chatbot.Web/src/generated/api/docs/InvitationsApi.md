# InvitationsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**acceptInvitation**](InvitationsApi.md#acceptinvitation) | **POST** /api/invitations/{invitationId}/accept |  |
| [**createInvitation**](InvitationsApi.md#createinvitationoperation) | **POST** /api/teams/{teamId}/invitations |  |
| [**listMyInvitations**](InvitationsApi.md#listmyinvitations) | **GET** /api/invitations/me |  |
| [**listTeamInvitations**](InvitationsApi.md#listteaminvitations) | **GET** /api/teams/{teamId}/invitations |  |
| [**revokeInvitation**](InvitationsApi.md#revokeinvitation) | **POST** /api/invitations/{invitationId}/revoke |  |



## acceptInvitation

> InvitationResponse acceptInvitation(invitationId)



### Example

```ts
import {
  Configuration,
  InvitationsApi,
} from '';
import type { AcceptInvitationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new InvitationsApi(config);

  const body = {
    // string
    invitationId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies AcceptInvitationRequest;

  try {
    const data = await api.acceptInvitation(body);
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
| **invitationId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**InvitationResponse**](InvitationResponse.md)

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


## createInvitation

> InvitationResponse createInvitation(teamId, createInvitationRequest)



### Example

```ts
import {
  Configuration,
  InvitationsApi,
} from '';
import type { CreateInvitationOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new InvitationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateInvitationRequest
    createInvitationRequest: ...,
  } satisfies CreateInvitationOperationRequest;

  try {
    const data = await api.createInvitation(body);
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
| **createInvitationRequest** | [CreateInvitationRequest](CreateInvitationRequest.md) |  | |

### Return type

[**InvitationResponse**](InvitationResponse.md)

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


## listMyInvitations

> Array&lt;InvitationResponse&gt; listMyInvitations()



### Example

```ts
import {
  Configuration,
  InvitationsApi,
} from '';
import type { ListMyInvitationsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new InvitationsApi(config);

  try {
    const data = await api.listMyInvitations();
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters

This endpoint does not need any parameter.

### Return type

[**Array&lt;InvitationResponse&gt;**](InvitationResponse.md)

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


## listTeamInvitations

> Array&lt;InvitationResponse&gt; listTeamInvitations(teamId)



### Example

```ts
import {
  Configuration,
  InvitationsApi,
} from '';
import type { ListTeamInvitationsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new InvitationsApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ListTeamInvitationsRequest;

  try {
    const data = await api.listTeamInvitations(body);
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

[**Array&lt;InvitationResponse&gt;**](InvitationResponse.md)

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


## revokeInvitation

> InvitationResponse revokeInvitation(invitationId)



### Example

```ts
import {
  Configuration,
  InvitationsApi,
} from '';
import type { RevokeInvitationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new InvitationsApi(config);

  const body = {
    // string
    invitationId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies RevokeInvitationRequest;

  try {
    const data = await api.revokeInvitation(body);
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
| **invitationId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**InvitationResponse**](InvitationResponse.md)

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

