export interface Message {
    Id?: string;
    tempId?: string; // Must be optional to be deleted after its work is done
    userOrTargetUserName: string | undefined;
    userOrTargetKnownAs: string | undefined;
    userOrTargetProfilePhoto: string | undefined;
    content: string;
    sentOn?: Date;
    readOn?: Date;
}
