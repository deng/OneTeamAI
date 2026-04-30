
# TicketResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`projectId` | string
`conciergeAppId` | string
`customerId` | string
`customerName` | string
`conversationId` | string
`title` | string
`summary` | string
`category` | string
`status` | [TicketStatus](TicketStatus.md)
`priority` | [TicketPriority](TicketPriority.md)
`dueAt` | Date
`resolutionSummary` | string
`resolvedAt` | Date
`lastActivityAt` | Date
`assignedMemberId` | string
`assignedMemberName` | string
`sourceType` | [RecordSourceType](RecordSourceType.md)
`externalSystemType` | [ExternalSystemType](ExternalSystemType.md)
`externalId` | string
`createdAt` | Date

## Example

```typescript
import type { TicketResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "projectId": null,
  "conciergeAppId": null,
  "customerId": null,
  "customerName": null,
  "conversationId": null,
  "title": null,
  "summary": null,
  "category": null,
  "status": null,
  "priority": null,
  "dueAt": null,
  "resolutionSummary": null,
  "resolvedAt": null,
  "lastActivityAt": null,
  "assignedMemberId": null,
  "assignedMemberName": null,
  "sourceType": null,
  "externalSystemType": null,
  "externalId": null,
  "createdAt": null,
} satisfies TicketResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as TicketResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


