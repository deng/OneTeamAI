
# CreateAiMemberRequest


## Properties

Name | Type
------------ | -------------
`displayName` | string
`jobTitle` | string
`responsibilitySummary` | string
`templateKey` | string
`permissionBoundary` | string
`title` | string
`systemPrompt` | string
`allowedTools` | string
`executableActions` | string
`knowledgeScope` | string
`isAutonomous` | boolean

## Example

```typescript
import type { CreateAiMemberRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "displayName": null,
  "jobTitle": null,
  "responsibilitySummary": null,
  "templateKey": null,
  "permissionBoundary": null,
  "title": null,
  "systemPrompt": null,
  "allowedTools": null,
  "executableActions": null,
  "knowledgeScope": null,
  "isAutonomous": null,
} satisfies CreateAiMemberRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CreateAiMemberRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


