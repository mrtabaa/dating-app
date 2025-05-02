namespace Da.Application.DTOs;

public class MessagesWithPaginationDto
{
    public IEnumerable<MessageDto> Messages { get; set; } = [];
    public PaginationHeader? Pagination { get; set; }
}