
# TeamDetailResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`name` | string
`description` | string
`brandName` | string
`owner` | [TeamOwnerResponse](TeamOwnerResponse.md)
`memberCount` | number
`projectCount` | number

## Example

```typescript
import type { TeamDetailResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "name": null,
  "description": null,
  "brandName": null,
  "owner": null,
  "memberCount": null,
  "projectCount": null,
} satisfies TeamDetailResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as TeamDetailResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


