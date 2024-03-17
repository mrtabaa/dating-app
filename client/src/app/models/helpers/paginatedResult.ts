import { Pagination } from "./pagination";

export class PaginatedResult<T> {
    result?: T; // api's response body
    pagination?: Pagination; // api's response pagination values
}
