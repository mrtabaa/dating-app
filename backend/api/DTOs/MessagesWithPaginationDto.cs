namespace api.DTOs;

public class MessagesWithPaginationDto
{
    public IEnumerable<MessageDto> MessageDtos { get; set; } = [];
    public PaginationHeader? PaginationHeader { get; set; }

}
