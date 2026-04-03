
# UserResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`email` | string
`displayName` | string
`companyName` | string

## Example

```typescript
import type { UserResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "email": null,
  "displayName": null,
  "companyName": null,
} satisfies UserResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as UserResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


