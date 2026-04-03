
# IntegrationConnectionHealthResponse


## Properties

Name | Type
------------ | -------------
`isReachable` | boolean
`isAuthenticated` | boolean
`message` | string
`checkedAt` | Date
`systemVersion` | string

## Example

```typescript
import type { IntegrationConnectionHealthResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "isReachable": null,
  "isAuthenticated": null,
  "message": null,
  "checkedAt": null,
  "systemVersion": null,
} satisfies IntegrationConnectionHealthResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as IntegrationConnectionHealthResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


