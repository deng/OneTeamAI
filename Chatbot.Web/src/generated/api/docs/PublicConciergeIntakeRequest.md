
# PublicConciergeIntakeRequest


## Properties

Name | Type
------------ | -------------
`displayName` | string
`email` | string
`phoneNumber` | string
`companyName` | string
`message` | string
`autoCreateTicket` | boolean
`autoTicketPriority` | [TicketPriority](TicketPriority.md)

## Example

```typescript
import type { PublicConciergeIntakeRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "displayName": null,
  "email": null,
  "phoneNumber": null,
  "companyName": null,
  "message": null,
  "autoCreateTicket": null,
  "autoTicketPriority": null,
} satisfies PublicConciergeIntakeRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as PublicConciergeIntakeRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


