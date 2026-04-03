
# AiMemberProfileResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`templateKey` | string
`jobTitle` | string
`responsibilitySummary` | string
`permissionBoundary` | string
`systemPrompt` | string
`allowedTools` | string
`executableActions` | string
`knowledgeScope` | string
`isAutonomous` | boolean

## Example

```typescript
import type { AiMemberProfileResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "templateKey": null,
  "jobTitle": null,
  "responsibilitySummary": null,
  "permissionBoundary": null,
  "systemPrompt": null,
  "allowedTools": null,
  "executableActions": null,
  "knowledgeScope": null,
  "isAutonomous": null,
} satisfies AiMemberProfileResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AiMemberProfileResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


