# AuditApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**listMyAuditLogs**](AuditApi.md#listmyauditlogs) | **GET** /api/audit-logs/me |  |
| [**listTeamAuditLogs**](AuditApi.md#listteamauditlogs) | **GET** /api/teams/{teamId}/audit-logs |  |



## listMyAuditLogs

> Array&lt;AuditLogResponse&gt; listMyAuditLogs(take)



### Example

```ts
import {
  Configuration,
  AuditApi,
} from '';
import type { ListMyAuditLogsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new AuditApi(config);

  const body = {
    // number (optional)
    take: 56,
  } satisfies ListMyAuditLogsRequest;

  try {
    const data = await api.listMyAuditLogs(body);
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
| **take** | `number` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;AuditLogResponse&gt;**](AuditLogResponse.md)

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


## listTeamAuditLogs

> Array&lt;AuditLogResponse&gt; listTeamAuditLogs(teamId, take)



### Example

```ts
import {
  Configuration,
  AuditApi,
} from '';
import type { ListTeamAuditLogsRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const config = new Configuration({ 
    // Configure HTTP bearer authorization: Bearer
    accessToken: "YOUR BEARER TOKEN",
  });
  const api = new AuditApi(config);

  const body = {
    // string
    teamId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // number (optional)
    take: 56,
  } satisfies ListTeamAuditLogsRequest;

  try {
    const data = await api.listTeamAuditLogs(body);
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
| **take** | `number` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;AuditLogResponse&gt;**](AuditLogResponse.md)

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

