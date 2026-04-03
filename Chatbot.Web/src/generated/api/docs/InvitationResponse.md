
# InvitationResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`teamName` | string
`email` | string
`role` | [MemberRole](MemberRole.md)
`title` | string
`status` | [InvitationStatus](InvitationStatus.md)
`invitedByDisplayName` | string
`expiresAt` | Date
`createdAt` | Date
`respondedAt` | Date

## Example

```typescript
import type { InvitationResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "teamName": null,
  "email": null,
  "role": null,
  "title": null,
  "status": null,
  "invitedByDisplayName": null,
  "expiresAt": null,
  "createdAt": null,
  "respondedAt": null,
} satisfies InvitationResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as InvitationResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


