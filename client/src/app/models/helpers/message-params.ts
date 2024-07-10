import { MessagePredicate } from "../../components/messages/MessageEnum.enum";
import { PaginationParams } from "./paginationParams";

export class MessageParams extends PaginationParams {
    predicate = MessagePredicate.inbox;
    targetUserName: string | undefined;
}