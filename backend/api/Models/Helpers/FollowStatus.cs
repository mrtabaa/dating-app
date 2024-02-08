namespace api.Models.Helpers;

public class FolowStatus
{
    public bool IsSuccess { get; set; }
    public bool IsAlreadyFollowed { get; set; }
    public bool IsTargetMemberEmailWrong { get; set; }
}