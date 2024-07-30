import { MessagePredicate } from "../../enums/MessagePredicate.enum";
import { PaginationParams } from "./paginationParams";

export class MessageParams extends PaginationParams {
    predicate = MessagePredicate.INBOX;
    targetUserName: string | undefined;
}