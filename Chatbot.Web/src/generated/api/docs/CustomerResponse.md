
# CustomerResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`displayName` | string
`email` | string
`phoneNumber` | string
`companyName` | string
`sourceLabel` | string
`tags` | string
`followUpStatus` | [CustomerFollowUpStatus](CustomerFollowUpStatus.md)
`lastContactedAt` | Date
`projectId` | string
`notes` | string
`status` | [CustomerStatus](CustomerStatus.md)
`sourceType` | [RecordSourceType](RecordSourceType.md)
`externalSystemType` | [ExternalSystemType](ExternalSystemType.md)
`externalId` | string

## Example

```typescript
import type { CustomerResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "displayName": null,
  "email": null,
  "phoneNumber": null,
  "companyName": null,
  "sourceLabel": null,
  "tags": null,
  "followUpStatus": null,
  "lastContactedAt": null,
  "projectId": null,
  "notes": null,
  "status": null,
  "sourceType": null,
  "externalSystemType": null,
  "externalId": null,
} satisfies CustomerResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CustomerResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


