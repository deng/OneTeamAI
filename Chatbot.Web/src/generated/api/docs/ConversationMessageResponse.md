
# ConversationMessageResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`participantType` | [ConversationParticipantType](ConversationParticipantType.md)
`memberId` | string
`senderName` | string
`content` | string
`createdAt` | Date

## Example

```typescript
import type { ConversationMessageResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "participantType": null,
  "memberId": null,
  "senderName": null,
  "content": null,
  "createdAt": null,
} satisfies ConversationMessageResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ConversationMessageResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


