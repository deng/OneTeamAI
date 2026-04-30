
# ProjectResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`name` | string
`description` | string
`stageLabel` | string
`summary` | string
`riskSummary` | string
`nextSteps` | string
`status` | [ProjectStatus](ProjectStatus.md)
`leadMemberId` | string
`participantMemberIds` | Array&lt;string&gt;
`participantCount` | number
`ticketCount` | number
`customerCount` | number
`sourceType` | [RecordSourceType](RecordSourceType.md)
`externalSystemType` | [ExternalSystemType](ExternalSystemType.md)
`externalId` | string

## Example

```typescript
import type { ProjectResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "name": null,
  "description": null,
  "stageLabel": null,
  "summary": null,
  "riskSummary": null,
  "nextSteps": null,
  "status": null,
  "leadMemberId": null,
  "participantMemberIds": null,
  "participantCount": null,
  "ticketCount": null,
  "customerCount": null,
  "sourceType": null,
  "externalSystemType": null,
  "externalId": null,
} satisfies ProjectResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ProjectResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


