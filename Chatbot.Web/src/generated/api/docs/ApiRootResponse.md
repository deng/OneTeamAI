
# ApiRootResponse


## Properties

Name | Type
------------ | -------------
`service` | string
`framework` | string
`agent` | string
`endpoints` | Array&lt;string&gt;

## Example

```typescript
import type { ApiRootResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "service": null,
  "framework": null,
  "agent": null,
  "endpoints": null,
} satisfies ApiRootResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ApiRootResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


