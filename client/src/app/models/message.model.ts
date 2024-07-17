export interface Message {
    Id: string;
    userOrTargetUserName: string | undefined;
    userOrTargetKnownAs: string | undefined;
    userOrTargetProfilePhoto: string | undefined;
    content: string;
    sentOn: Date
    readOn: Date;
}
