namespace api.Models.Helpers;

public class FollowStatus
{
    public bool IsSuccess { get; set; }
    public string KnownAs { get; set; } = string.Empty;
    public bool IsAlreadyFollowed { get; set; }
    public bool IsFollowingThemself { get; set; }
    public bool IsTargetMemberNotFound { get; set; }
}