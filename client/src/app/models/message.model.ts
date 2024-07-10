export interface Message {
    Id: string;
    userOrTargetUserName: string;
    userOrTargetKnownAs: string;
    userOrTargetProfilePhoto: string;
    content: string;
    sentOn: Date
    readOn: Date;
}
