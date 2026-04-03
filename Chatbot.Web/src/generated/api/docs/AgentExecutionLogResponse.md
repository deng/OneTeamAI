
# AgentExecutionLogResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`memberId` | string
`memberName` | string
`toolName` | string
`toolCategory` | string
`boundarySummary` | string
`inputSummary` | string
`outputSummary` | string
`status` | [AgentExecutionLogStatus](AgentExecutionLogStatus.md)
`wasAllowed` | boolean
`executedAt` | Date

## Example

```typescript
import type { AgentExecutionLogResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "memberId": null,
  "memberName": null,
  "toolName": null,
  "toolCategory": null,
  "boundarySummary": null,
  "inputSummary": null,
  "outputSummary": null,
  "status": null,
  "wasAllowed": null,
  "executedAt": null,
} satisfies AgentExecutionLogResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AgentExecutionLogResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


