/* tslint:disable */
/* eslint-disable */
import type { CustomerFollowUpStatus } from './CustomerFollowUpStatus';
import { CustomerFollowUpStatusFromJSON, CustomerFollowUpStatusToJSON } from './CustomerFollowUpStatus';
import type { CustomerStatus } from './CustomerStatus';
import { CustomerStatusFromJSON, CustomerStatusToJSON } from './CustomerStatus';

export interface UpdateCustomerRequest {
    displayName?: string | null;
    email?: string | null;
    phoneNumber?: string | null;
    companyName?: string | null;
    sourceLabel?: string | null;
    tags?: string | null;
    followUpStatus?: CustomerFollowUpStatus;
    lastContactedAt?: Date | null;
    projectId?: string | null;
    notes?: string | null;
    status?: CustomerStatus;
}

export function UpdateCustomerRequestFromJSON(json: any): UpdateCustomerRequest {
    if (json == null) {
        return json;
    }
    return {
        displayName: json['displayName'] == null ? undefined : json['displayName'],
        email: json['email'] == null ? undefined : json['email'],
        phoneNumber: json['phoneNumber'] == null ? undefined : json['phoneNumber'],
        companyName: json['companyName'] == null ? undefined : json['companyName'],
        sourceLabel: json['sourceLabel'] == null ? undefined : json['sourceLabel'],
        tags: json['tags'] == null ? undefined : json['tags'],
        followUpStatus: json['followUpStatus'] == null ? undefined : CustomerFollowUpStatusFromJSON(json['followUpStatus']),
        lastContactedAt: json['lastContactedAt'] == null ? undefined : new Date(json['lastContactedAt']),
        projectId: json['projectId'] == null ? undefined : json['projectId'],
        notes: json['notes'] == null ? undefined : json['notes'],
        status: json['status'] == null ? undefined : CustomerStatusFromJSON(json['status']),
    };
}

export function UpdateCustomerRequestToJSON(value?: UpdateCustomerRequest | null): any {
    if (value == null) {
        return value;
    }
    return {
        displayName: value.displayName,
        email: value.email,
        phoneNumber: value.phoneNumber,
        companyName: value.companyName,
        sourceLabel: value.sourceLabel,
        tags: value.tags,
        followUpStatus: CustomerFollowUpStatusToJSON(value.followUpStatus),
        lastContactedAt: value.lastContactedAt?.toISOString(),
        projectId: value.projectId,
        notes: value.notes,
        status: CustomerStatusToJSON(value.status),
    };
}
