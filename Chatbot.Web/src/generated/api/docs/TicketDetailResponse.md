
# TicketDetailResponse


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
`activities` | [Array&lt;TicketActivityResponse&gt;](TicketActivityResponse.md)

## Example

```typescript
import type { TicketDetailResponse } from ''

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
  "activities": null,
} satisfies TicketDetailResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as TicketDetailResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


