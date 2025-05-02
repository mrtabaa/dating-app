namespace da.Application.DTOs;

public record PaginationHeader(
    int CurrentPage,
    int ItemsPerPage,
    int TotalItems,
    int TotalPages
);