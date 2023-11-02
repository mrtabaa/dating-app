export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

export interface PaginationResult<T> {
    result?: T; // api's response body
    pagination?: Pagination // api's response pagination values
}