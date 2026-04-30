
# UpdateProjectRequest


## Properties

Name | Type
------------ | -------------
`name` | string
`description` | string
`stageLabel` | string
`summary` | string
`riskSummary` | string
`nextSteps` | string
`leadMemberId` | string
`participantMemberIds` | Array&lt;string&gt;

## Example

```typescript
import type { UpdateProjectRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "name": null,
  "description": null,
  "stageLabel": null,
  "summary": null,
  "riskSummary": null,
  "nextSteps": null,
  "leadMemberId": null,
  "participantMemberIds": null,
} satisfies UpdateProjectRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as UpdateProjectRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


