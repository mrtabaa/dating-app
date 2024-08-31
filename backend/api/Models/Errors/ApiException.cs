namespace api.Models.Errors;

public class ApiException
{
    public ObjectId Id { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public DateTime Time { get; set; }
    public int Number { get; set; }
    public MessageInDto? MessageInDto { get; set; }
    public MessageDto? MessageDto { get; set; }
    public ObjectId? UserId { get; set; }
};
