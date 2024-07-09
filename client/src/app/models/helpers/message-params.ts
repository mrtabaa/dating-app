import { Tabs } from "../../components/messages/tabs.enum";
import { PaginationParams } from "./paginationParams";

export class MessageParams extends PaginationParams {
    predicate = Tabs.inbox;
}