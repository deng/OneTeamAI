
# AgentWorkflowResponse


## Properties

Name | Type
------------ | -------------
`id` | string
`teamId` | string
`projectId` | string
`conversationId` | string
`ticketId` | string
`workflowType` | string
`triggerMode` | [AgentWorkflowTriggerMode](AgentWorkflowTriggerMode.md)
`goal` | string
`summary` | string
`summarySchemaVersion` | string
`summaryRawResponse` | string
`summaryAttempts` | [Array&lt;AiResponseAttemptResponse&gt;](AiResponseAttemptResponse.md)
`status` | [AgentWorkflowStatus](AgentWorkflowStatus.md)
`requestedByUserId` | string
`startedByMemberId` | string
`startedByMemberName` | string
`createdAt` | Date
`completedAt` | Date
`steps` | [Array&lt;AgentWorkflowStepResponse&gt;](AgentWorkflowStepResponse.md)

## Example

```typescript
import type { AgentWorkflowResponse } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "teamId": null,
  "projectId": null,
  "conversationId": null,
  "ticketId": null,
  "workflowType": null,
  "triggerMode": null,
  "goal": null,
  "summary": null,
  "summarySchemaVersion": null,
  "summaryRawResponse": null,
  "summaryAttempts": null,
  "status": null,
  "requestedByUserId": null,
  "startedByMemberId": null,
  "startedByMemberName": null,
  "createdAt": null,
  "completedAt": null,
  "steps": null,
} satisfies AgentWorkflowResponse

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AgentWorkflowResponse
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


