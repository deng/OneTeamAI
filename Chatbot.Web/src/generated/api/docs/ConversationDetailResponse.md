
# ConversationDetailResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`conciergeAppId` | string
`customerId` | string
`customer` | [ConversationCustomerResponse](ConversationCustomerResponse.md)
`status` | [ConversationStatus](ConversationStatus.md)
`messages` | [Array&lt;ConversationMessageResponse&gt;](ConversationMessageResponse.md)
`ticketIds` | Array&lt;string&gt;

## Example

```typescript
import type { ConversationDetailResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "conciergeAppId": null,
  "customerId": null,
  "customer": null,
  "status": null,
  "messages": null,
  "ticketIds": null,
} satisfies ConversationDetailResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ConversationDetailResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


