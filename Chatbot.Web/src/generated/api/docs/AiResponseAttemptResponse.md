
# AiResponseAttemptResponse


## Properties

Name | Type
------------ | -------------
`attempt` | number
`outcome` | string
`schemaVersion` | string
`validationError` | string
`rawResponse` | string
`createdAt` | Date

## Example

```typescript
import type { AiResponseAttemptResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "attempt": null,
  "outcome": null,
  "schemaVersion": null,
  "validationError": null,
  "rawResponse": null,
  "createdAt": null,
} satisfies AiResponseAttemptResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AiResponseAttemptResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


