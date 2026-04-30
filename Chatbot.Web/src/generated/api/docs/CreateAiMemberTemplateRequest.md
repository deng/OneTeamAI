
# CreateAiMemberTemplateRequest


## Properties

Name | Type
------------ | -------------
`label` | string
`displayName` | string
`jobTitle` | string
`responsibilitySummary` | string
`key` | string
`title` | string
`permissionBoundary` | string
`systemPrompt` | string
`allowedTools` | string
`executableActions` | string
`knowledgeScope` | string
`isAutonomous` | boolean
`sortOrder` | number

## Example

```typescript
import type { CreateAiMemberTemplateRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "label": null,
  "displayName": null,
  "jobTitle": null,
  "responsibilitySummary": null,
  "key": null,
  "title": null,
  "permissionBoundary": null,
  "systemPrompt": null,
  "allowedTools": null,
  "executableActions": null,
  "knowledgeScope": null,
  "isAutonomous": null,
  "sortOrder": null,
} satisfies CreateAiMemberTemplateRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CreateAiMemberTemplateRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


