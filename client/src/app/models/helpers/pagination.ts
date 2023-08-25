export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

export interface PaginationResult<T> {
    result?: T;
    pagination?: Pagination
}