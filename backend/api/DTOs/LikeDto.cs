namespace api.DTOs;

public record LikeDto(
    LoggedInUserDto LoggedInUserDto,
    TargetMemberDto TargetMemberDto
);

public record LoggedInUserDto(
    int Age,
    string KnownAs,
    string Gender,
    string City,
    string? PhotoUrl
);

public record TargetMemberDto(
    int Age,
    string KnownAs,
    string Gender,
    string City,
    string? PhotoUrl
);
