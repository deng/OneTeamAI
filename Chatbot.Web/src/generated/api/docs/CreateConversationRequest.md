
# CreateConversationRequest


## Properties

Name | Type
------------ | -------------
`customerId` | string
`customerDisplayName` | string
`customerEmail` | string
`initialMessage` | string
`autoCreateTicket` | boolean
`autoTicketPriority` | [TicketPriority](TicketPriority.md)

## Example

```typescript
import type { CreateConversationRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "customerId": null,
  "customerDisplayName": null,
  "customerEmail": null,
  "initialMessage": null,
  "autoCreateTicket": null,
  "autoTicketPriority": null,
} satisfies CreateConversationRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CreateConversationRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


