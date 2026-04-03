
# TeamSummaryResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`name` | string
`description` | string
`brandName` | string
`ownerUserId` | string
`currentMemberId` | string
`currentMemberRole` | [MemberRole](MemberRole.md)

## Example

```typescript
import type { TeamSummaryResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "name": null,
  "description": null,
  "brandName": null,
  "ownerUserId": null,
  "currentMemberId": null,
  "currentMemberRole": null,
} satisfies TeamSummaryResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as TeamSummaryResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


