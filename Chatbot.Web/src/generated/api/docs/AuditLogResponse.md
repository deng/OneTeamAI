
# AuditLogResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`userId` | string
`userDisplayName` | string
`actionType` | string
`entityType` | string
`entityId` | string
`summary` | string
`result` | string
`ipAddress` | string
`createdAt` | Date

## Example

```typescript
import type { AuditLogResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "userId": null,
  "userDisplayName": null,
  "actionType": null,
  "entityType": null,
  "entityId": null,
  "summary": null,
  "result": null,
  "ipAddress": null,
  "createdAt": null,
} satisfies AuditLogResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AuditLogResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


