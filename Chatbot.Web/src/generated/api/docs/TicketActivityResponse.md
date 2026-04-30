
# TicketActivityResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`activityType` | [TicketActivityType](TicketActivityType.md)
`summary` | string
`detail` | string
`actorMemberId` | string
`actorMemberName` | string
`actorUserId` | string
`actorUserName` | string
`createdAt` | Date

## Example

```typescript
import type { TicketActivityResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "activityType": null,
  "summary": null,
  "detail": null,
  "actorMemberId": null,
  "actorMemberName": null,
  "actorUserId": null,
  "actorUserName": null,
  "createdAt": null,
} satisfies TicketActivityResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as TicketActivityResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


