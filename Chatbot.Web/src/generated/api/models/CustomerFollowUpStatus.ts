/* tslint:disable */
/* eslint-disable */
export const CustomerFollowUpStatus = {
    NUMBER_0: 0,
    NUMBER_1: 1,
    NUMBER_2: 2,
    NUMBER_3: 3,
    NUMBER_4: 4,
} as const;

export type CustomerFollowUpStatus = typeof CustomerFollowUpStatus[keyof typeof CustomerFollowUpStatus];

export function CustomerFollowUpStatusFromJSON(json: any): CustomerFollowUpStatus {
    return json as CustomerFollowUpStatus;
}

export function CustomerFollowUpStatusToJSON(value?: CustomerFollowUpStatus | null): any {
    return value;
}
