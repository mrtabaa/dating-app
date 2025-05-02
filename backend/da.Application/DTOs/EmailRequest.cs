namespace da.Application.DTOs;

public record EmailRequest(
    string ToEmail,
    string Subject,
    string Body
);