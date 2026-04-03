
# AgentWorkflowStepResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`sequence` | number
`memberId` | string
`memberName` | string
`memberTitle` | string
`handoffToMemberId` | string
`handoffToMemberName` | string
`actionType` | string
`inputSummary` | string
`outputSummary` | string
`handoffSummary` | string
`status` | [AgentWorkflowStepStatus](AgentWorkflowStepStatus.md)
`executedAt` | Date
`executionLogs` | [Array&lt;AgentExecutionLogResponse&gt;](AgentExecutionLogResponse.md)

## Example

```typescript
import type { AgentWorkflowStepResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "sequence": null,
  "memberId": null,
  "memberName": null,
  "memberTitle": null,
  "handoffToMemberId": null,
  "handoffToMemberName": null,
  "actionType": null,
  "inputSummary": null,
  "outputSummary": null,
  "handoffSummary": null,
  "status": null,
  "executedAt": null,
  "executionLogs": null,
} satisfies AgentWorkflowStepResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AgentWorkflowStepResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


