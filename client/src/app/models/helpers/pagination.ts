export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

export class PaginatedResult<T> { // Declare as a class so we can create an object of it
    result?: T; // api's response body
    pagination?: Pagination; // api's response pagination values
}