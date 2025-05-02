namespace Da.Application.DTOs.Parameters;

public class MemberParams : PaginationParams
{
    public string? UserId { get; set; }
    public string? UserNameOrKnownAs { get; set; }
    public string? Gender { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string OrderBy { get; set; } = "lastActive";
}