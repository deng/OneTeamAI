
# UserSessionResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`createdAt` | Date
`lastSeenAt` | Date
`expiresAt` | Date
`revokedAt` | Date
`revokedReason` | string
`userAgent` | string
`ipAddress` | string
`isCurrent` | boolean

## Example

```typescript
import type { UserSessionResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "createdAt": null,
  "lastSeenAt": null,
  "expiresAt": null,
  "revokedAt": null,
  "revokedReason": null,
  "userAgent": null,
  "ipAddress": null,
  "isCurrent": null,
} satisfies UserSessionResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as UserSessionResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


