
# AiMemberTemplateResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`key` | string
`label` | string
`displayName` | string
`jobTitle` | string
`responsibilitySummary` | string
`title` | string
`permissionBoundary` | string
`systemPrompt` | string
`allowedTools` | string
`executableActions` | string
`knowledgeScope` | string
`isAutonomous` | boolean
`teamId` | string
`isBuiltIn` | boolean
`isEnabled` | boolean
`sortOrder` | number

## Example

```typescript
import type { AiMemberTemplateResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "key": null,
  "label": null,
  "displayName": null,
  "jobTitle": null,
  "responsibilitySummary": null,
  "title": null,
  "permissionBoundary": null,
  "systemPrompt": null,
  "allowedTools": null,
  "executableActions": null,
  "knowledgeScope": null,
  "isAutonomous": null,
  "teamId": null,
  "isBuiltIn": null,
  "isEnabled": null,
  "sortOrder": null,
} satisfies AiMemberTemplateResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AiMemberTemplateResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


