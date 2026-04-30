
# CreateConciergeAppRequest


## Properties

Name | Type
------------ | -------------
`projectId` | string
`name` | string
`description` | string
`serviceScope` | string
`welcomeMessage` | string
`faqScope` | string
`businessHours` | string
`channelLabel` | string
`intakeGuidance` | string
`suggestedPrompts` | string
`requireEmail` | boolean
`requirePhoneNumber` | boolean
`primaryAiMemberId` | string
`ticketCreationPolicy` | string
`humanHandoffPolicy` | string

## Example

```typescript
import type { CreateConciergeAppRequest } from ''

// TODO: Update the object below with actual values
const example = {
  "projectId": null,
  "name": null,
  "description": null,
  "serviceScope": null,
  "welcomeMessage": null,
  "faqScope": null,
  "businessHours": null,
  "channelLabel": null,
  "intakeGuidance": null,
  "suggestedPrompts": null,
  "requireEmail": null,
  "requirePhoneNumber": null,
  "primaryAiMemberId": null,
  "ticketCreationPolicy": null,
  "humanHandoffPolicy": null,
} satisfies CreateConciergeAppRequest

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CreateConciergeAppRequest
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


