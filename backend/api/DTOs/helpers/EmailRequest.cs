namespace api.DTOs.helpers;

public record EmailRequest(
    string ToEmail,
    string Subject,
    string Body
);