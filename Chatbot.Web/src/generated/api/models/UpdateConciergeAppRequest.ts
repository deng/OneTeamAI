/* tslint:disable */
/* eslint-disable */
import { ConciergeAppStatus, ConciergeAppStatusFromJSON, ConciergeAppStatusToJSON } from './ConciergeAppStatus';

export interface UpdateConciergeAppRequest {
    name?: string | null;
    description?: string | null;
    serviceScope?: string | null;
    welcomeMessage?: string | null;
    faqScope?: string | null;
    businessHours?: string | null;
    channelLabel?: string | null;
    status?: ConciergeAppStatus;
    primaryAiMemberId?: string | null;
    ticketCreationPolicy?: string | null;
    humanHandoffPolicy?: string | null;
}

export function UpdateConciergeAppRequestFromJSON(json: any): UpdateConciergeAppRequest {
    if (json == null) {
        return json;
    }

    return {
        name: json['name'] == null ? undefined : json['name'],
        description: json['description'] == null ? undefined : json['description'],
        serviceScope: json['serviceScope'] == null ? undefined : json['serviceScope'],
        welcomeMessage: json['welcomeMessage'] == null ? undefined : json['welcomeMessage'],
        faqScope: json['faqScope'] == null ? undefined : json['faqScope'],
        businessHours: json['businessHours'] == null ? undefined : json['businessHours'],
        channelLabel: json['channelLabel'] == null ? undefined : json['channelLabel'],
        status: json['status'] == null ? undefined : ConciergeAppStatusFromJSON(json['status']),
        primaryAiMemberId: json['primaryAiMemberId'] == null ? undefined : json['primaryAiMemberId'],
        ticketCreationPolicy: json['ticketCreationPolicy'] == null ? undefined : json['ticketCreationPolicy'],
        humanHandoffPolicy: json['humanHandoffPolicy'] == null ? undefined : json['humanHandoffPolicy'],
    };
}

export function UpdateConciergeAppRequestToJSON(value?: UpdateConciergeAppRequest | null): any {
    if (value == null) {
        return value;
    }

    return {
        name: value.name,
        description: value.description,
        serviceScope: value.serviceScope,
        welcomeMessage: value.welcomeMessage,
        faqScope: value.faqScope,
        businessHours: value.businessHours,
        channelLabel: value.channelLabel,
        status: ConciergeAppStatusToJSON(value.status),
        primaryAiMemberId: value.primaryAiMemberId,
        ticketCreationPolicy: value.ticketCreationPolicy,
        humanHandoffPolicy: value.humanHandoffPolicy,
    };
}
