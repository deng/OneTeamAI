
# IntegrationConnectionResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`externalSystemType` | [ExternalSystemType](ExternalSystemType.md)
`name` | string
`baseUrl` | string
`isEnabled` | boolean
`hasAuthConfig` | boolean
`createdAt` | Date

## Example

```typescript
import type { IntegrationConnectionResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "externalSystemType": null,
  "name": null,
  "baseUrl": null,
  "isEnabled": null,
  "hasAuthConfig": null,
  "createdAt": null,
} satisfies IntegrationConnectionResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as IntegrationConnectionResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


