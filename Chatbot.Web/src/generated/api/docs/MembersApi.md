# MembersApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**createAiMember**](MembersApi.md#createaimemberoperation) | **POST** /api/teams/{teamId}/members/ai |  |
| [**createAiMemberTemplate**](MembersApi.md#createaimembertemplateoperation) | **POST** /api/teams/{teamId}/ai-member-templates |  |
| [**createHumanMember**](MembersApi.md#createhumanmemberoperation) | **POST** /api/teams/{teamId}/members/human |  |
| [**disableAiMemberTemplate**](MembersApi.md#disableaimembertemplate) | **DELETE** /api/teams/{teamId}/ai-member-templates/{templateId} |  |
| [**listAiMemberTemplates**](MembersApi.md#listaimembertemplates) | **GET** /api/ai-member-templates |  |
| [**listTeamMembers**](MembersApi.md#listteammembers) | **GET** /api/teams/{teamId}/members |  |
| [**removeMember**](MembersApi.md#removemember) | **DELETE** /api/teams/{teamId}/members/{memberId} |  |
| [**updateAiMemberTemplate**](MembersApi.md#updateaimembertemplateoperation) | **PUT** /api/teams/{teamId}/ai-member-templates/{templateId} |  |
| [**updateMember**](MembersApi.md#updatememberoperation) | **PATCH** /api/teams/{teamId}/members/{memberId} |  |



## createAiMember

> MemberResponse createAiMember(teamId, createAiMemberRequest)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { CreateAiMemberOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateAiMemberRequest
    createAiMemberRequest: ...,
  } satisfies CreateAiMemberOperationRequest;

  try {
    const data = await api.createAiMember(body);
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
| **createAiMemberRequest** | [CreateAiMemberRequest](CreateAiMemberRequest.md) |  | |

### Return type

[**MemberResponse**](MemberResponse.md)

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


## createAiMemberTemplate

> AiMemberTemplateResponse createAiMemberTemplate(teamId, createAiMemberTemplateRequest)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { CreateAiMemberTemplateOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateAiMemberTemplateRequest
    createAiMemberTemplateRequest: ...,
  } satisfies CreateAiMemberTemplateOperationRequest;

  try {
    const data = await api.createAiMemberTemplate(body);
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
| **createAiMemberTemplateRequest** | [CreateAiMemberTemplateRequest](CreateAiMemberTemplateRequest.md) |  | |

### Return type

[**AiMemberTemplateResponse**](AiMemberTemplateResponse.md)

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


## createHumanMember

> MemberResponse createHumanMember(teamId, createHumanMemberRequest)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { CreateHumanMemberOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // CreateHumanMemberRequest
    createHumanMemberRequest: ...,
  } satisfies CreateHumanMemberOperationRequest;

  try {
    const data = await api.createHumanMember(body);
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
| **createHumanMemberRequest** | [CreateHumanMemberRequest](CreateHumanMemberRequest.md) |  | |

### Return type

[**MemberResponse**](MemberResponse.md)

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


## disableAiMemberTemplate

> AiMemberTemplateResponse disableAiMemberTemplate(teamId, templateId)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { DisableAiMemberTemplateRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    templateId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies DisableAiMemberTemplateRequest;

  try {
    const data = await api.disableAiMemberTemplate(body);
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
| **templateId** | `string` |  | [Defaults to `undefined`] |

### Return type

[**AiMemberTemplateResponse**](AiMemberTemplateResponse.md)

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
| **409** | Conflict |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## listAiMemberTemplates

> Array&lt;AiMemberTemplateResponse&gt; listAiMemberTemplates(teamId, includeDisabled)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { ListAiMemberTemplatesRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string (optional)
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // boolean (optional)
    includeDisabled: true,
  } satisfies ListAiMemberTemplatesRequest;

  try {
    const data = await api.listAiMemberTemplates(body);
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
| **teamId** | `string` |  | [Optional] [Defaults to `undefined`] |
| **includeDisabled** | `boolean` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;AiMemberTemplateResponse&gt;**](AiMemberTemplateResponse.md)

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


## listTeamMembers

> Array&lt;MemberResponse&gt; listTeamMembers(teamId)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { ListTeamMembersRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ListTeamMembersRequest;

  try {
    const data = await api.listTeamMembers(body);
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

[**Array&lt;MemberResponse&gt;**](MemberResponse.md)

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


## removeMember

> removeMember(teamId, memberId)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { RemoveMemberRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    memberId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies RemoveMemberRequest;

  try {
    const data = await api.removeMember(body);
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
| **memberId** | `string` |  | [Defaults to `undefined`] |

### Return type

`void` (Empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | No Content |  -  |
| **400** | Bad Request |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## updateAiMemberTemplate

> AiMemberTemplateResponse updateAiMemberTemplate(teamId, templateId, updateAiMemberTemplateRequest)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { UpdateAiMemberTemplateOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    templateId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // UpdateAiMemberTemplateRequest
    updateAiMemberTemplateRequest: ...,
  } satisfies UpdateAiMemberTemplateOperationRequest;

  try {
    const data = await api.updateAiMemberTemplate(body);
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
| **templateId** | `string` |  | [Defaults to `undefined`] |
| **updateAiMemberTemplateRequest** | [UpdateAiMemberTemplateRequest](UpdateAiMemberTemplateRequest.md) |  | |

### Return type

[**AiMemberTemplateResponse**](AiMemberTemplateResponse.md)

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
| **409** | Conflict |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## updateMember

> MemberResponse updateMember(teamId, memberId, updateMemberRequest)



### Example

```ts
import {
  Configuration,
  MembersApi,
} from '';
import type { UpdateMemberOperationRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new MembersApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    memberId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // UpdateMemberRequest
    updateMemberRequest: ...,
  } satisfies UpdateMemberOperationRequest;

  try {
    const data = await api.updateMember(body);
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
| **memberId** | `string` |  | [Defaults to `undefined`] |
| **updateMemberRequest** | [UpdateMemberRequest](UpdateMemberRequest.md) |  | |

### Return type

[**MemberResponse**](MemberResponse.md)

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

