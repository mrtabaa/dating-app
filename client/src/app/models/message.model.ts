export interface Message {
    tempId?: string; // Must be optional to be deleted after its work is done
    Id?: string;
    userOrTargetUserName: string | undefined;
    userOrTargetKnownAs: string | undefined;
    userOrTargetProfilePhoto: string | undefined;
    content: string;
    sentOn: Date
    readOn?: Date;
}
