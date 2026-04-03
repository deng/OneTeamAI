
# ConversationSummaryResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`conciergeAppId` | string
`customerId` | string
`customerName` | string
`status` | [ConversationStatus](ConversationStatus.md)
`messageCount` | number
`latestMessage` | string
`createdAt` | Date

## Example

```typescript
import type { ConversationSummaryResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "conciergeAppId": null,
  "customerId": null,
  "customerName": null,
  "status": null,
  "messageCount": null,
  "latestMessage": null,
  "createdAt": null,
} satisfies ConversationSummaryResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ConversationSummaryResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


