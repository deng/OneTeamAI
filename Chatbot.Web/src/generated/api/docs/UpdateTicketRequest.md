
# UpdateTicketRequest


## Properties

Name | Type
------------ | -------------
`status` | [TicketStatus](TicketStatus.md)
`priority` | [TicketPriority](TicketPriority.md)
`assignedMemberId` | string

## Example

```typescript
import type { UpdateTicketRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "status": null,
  "priority": null,
  "assignedMemberId": null,
} satisfies UpdateTicketRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as UpdateTicketRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


