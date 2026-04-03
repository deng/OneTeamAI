/* tslint:disable */
/* eslint-disable */
import { mapValues } from '../runtime';

export interface UpdateProjectRequest {
    name?: string | null;
    description?: string | null;
    stageLabel?: string | null;
    summary?: string | null;
    riskSummary?: string | null;
    nextSteps?: string | null;
    leadMemberId?: string | null;
    participantMemberIds?: Array<string> | null;
}

export function instanceOfUpdateProjectRequest(value: object): value is UpdateProjectRequest {
    return true;
}

export function UpdateProjectRequestFromJSON(json: any): UpdateProjectRequest {
    return UpdateProjectRequestFromJSONTyped(json, false);
}

export function UpdateProjectRequestFromJSONTyped(json: any, ignoreDiscriminator: boolean): UpdateProjectRequest {
    if (json == null) {
        return json;
    }
    return {
        'name': json['name'] == null ? undefined : json['name'],
        'description': json['description'] == null ? undefined : json['description'],
        'stageLabel': json['stageLabel'] == null ? undefined : json['stageLabel'],
        'summary': json['summary'] == null ? undefined : json['summary'],
        'riskSummary': json['riskSummary'] == null ? undefined : json['riskSummary'],
        'nextSteps': json['nextSteps'] == null ? undefined : json['nextSteps'],
        'leadMemberId': json['leadMemberId'] == null ? undefined : json['leadMemberId'],
        'participantMemberIds': json['participantMemberIds'] == null ? undefined : json['participantMemberIds'],
    };
}

export function UpdateProjectRequestToJSON(json: any): UpdateProjectRequest {
    return UpdateProjectRequestToJSONTyped(json, false);
}

export function UpdateProjectRequestToJSONTyped(value?: UpdateProjectRequest | null, ignoreDiscriminator: boolean = false): any {
    if (value == null) {
        return value;
    }

    return {
        'name': value['name'],
        'description': value['description'],
        'stageLabel': value['stageLabel'],
        'summary': value['summary'],
        'riskSummary': value['riskSummary'],
        'nextSteps': value['nextSteps'],
        'leadMemberId': value['leadMemberId'],
        'participantMemberIds': value['participantMemberIds'],
    };
}
