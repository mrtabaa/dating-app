import { Pagination } from "./helpers/pagination";
import { Message } from "./message.model";

export interface MessagesWithPagination {
    messages: Message[];
    pagination: Pagination;
}
