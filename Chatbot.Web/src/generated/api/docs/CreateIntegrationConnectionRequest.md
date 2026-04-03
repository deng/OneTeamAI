
# CreateIntegrationConnectionRequest


## Properties

Name | Type
------------ | -------------
`externalSystemType` | [ExternalSystemType](ExternalSystemType.md)
`name` | string
`baseUrl` | string
`authConfig` | string
`isEnabled` | boolean

## Example

```typescript
import type { CreateIntegrationConnectionRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "externalSystemType": null,
  "name": null,
  "baseUrl": null,
  "authConfig": null,
  "isEnabled": null,
} satisfies CreateIntegrationConnectionRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CreateIntegrationConnectionRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


