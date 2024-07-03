export interface Message {
    Id: string;
    senderUserName: string;
    recipientUserName: string;
    content: string;
    readOn: Date;
    sentOn: Date
}