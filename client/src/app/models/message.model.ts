export interface Message {
    Id: string;
    senderUserName: string;
    receiverUserName: string;
    targetUserProfilePhoto: string;
    content: string;
    sentOn: Date
    readOn: Date;
}
