public record MemberDto(
    string Schema,
    string Id,
    string Name,
    string Email,
    int Age,
    string KnownAs,
    DateTime Created,
    DateTime LastActive,
    string Gender,
    string Introduction,
    string LookingFor,
    string Interests,
    string City,
    string Country,
    List<Photo> Photos
);
