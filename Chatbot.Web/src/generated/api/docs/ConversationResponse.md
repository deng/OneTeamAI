
# ConversationResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`conciergeAppId` | string
`customerId` | string
`status` | [ConversationStatus](ConversationStatus.md)
`firstMessage` | string
`autoCreatedTicketId` | string

## Example

```typescript
import type { ConversationResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "conciergeAppId": null,
  "customerId": null,
  "status": null,
  "firstMessage": null,
  "autoCreatedTicketId": null,
} satisfies ConversationResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ConversationResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


