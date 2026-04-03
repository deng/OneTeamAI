
# MemberResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`memberType` | [MemberType](MemberType.md)
`role` | [MemberRole](MemberRole.md)
`status` | [MemberStatus](MemberStatus.md)
`displayName` | string
`title` | string
`aiProfile` | [AiMemberProfileResponse](AiMemberProfileResponse.md)

## Example

```typescript
import type { MemberResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "memberType": null,
  "role": null,
  "status": null,
  "displayName": null,
  "title": null,
  "aiProfile": null,
} satisfies MemberResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as MemberResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


