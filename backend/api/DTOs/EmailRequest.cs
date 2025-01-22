namespace api.DTOs.Helpers;

public record EmailRequest(
    string ToEmail,
    string Subject,
    string Body
);