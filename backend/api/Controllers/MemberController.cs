namespace api.Controllers;

[Authorize]
public class MemberController(IMemberRepository _memberRepository, IUserRepository _userRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto?>>> GetMembers([FromQuery] MemberParams memberParams, CancellationToken cancellationToken)
    {
        if (memberParams.MinAge > memberParams.MaxAge)
            return BadRequest("Selected minAge cannot be greater than maxAge");

        string? userIdHashed = User.GetUserIdHashed();

        AppUser? appUser = await _userRepository.GetByHashedIdAsync(userIdHashed, cancellationToken);

        if (appUser is not null && string.IsNullOrEmpty(memberParams.Gender))
        {
            memberParams.UserId = appUser.Id;
            memberParams.Gender = appUser.Gender == "male" ? "female" : "male";
        }

        PagedList<AppUser> pagedAppUsers = await _memberRepository.GetMembersAsync(memberParams, cancellationToken);

        /*  1- Response only exists in Contoller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                If we convert AppUser before here, we'll lose PagedList's pagination values, e.g. CurrentPage, PageSize, etc.
        */
        Response.AddPaginationHeader(new PaginationHeader(pagedAppUsers.CurrentPage, pagedAppUsers.PageSize, pagedAppUsers.TotalCount, pagedAppUsers.TotalPages));

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values. 
                After that step we can convert AppUser to MemberDto in here (NOT in the UserRepository) */
        List<MemberDto?> memberDtos = [];

        foreach (AppUser pagedAppUser in pagedAppUsers)
        {
            memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser));
        }

        return memberDtos;
    }

    [HttpGet("id/{memberId}")]
    public async Task<ActionResult<MemberDto>> GetMemberById(string memberId, CancellationToken cancellationToken)
    {
        bool isValid = ObjectId.TryParse(memberId, out ObjectId memberObjectId);

        if (!isValid)
            return BadRequest("Invalid memberId. Contact the admin.");

        MemberDto? memberDto = await _memberRepository.GetMemberByIdAsync(memberObjectId, cancellationToken);

        return memberDto is null ? BadRequest("No member found by this ID.") : memberDto;
    }

    // TODO change it to UserName
    [HttpGet("email/{email}")]
    public async Task<ActionResult<MemberDto>> GetMemberByEmail(
            [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$", ErrorMessage = "Bad Email Format. Contact the admin if persists.")]
            string email,
            CancellationToken cancellationToken
        )
    {
        MemberDto? memberDto = await _memberRepository.GetMemberByEmailAsync(email, cancellationToken);

        return memberDto is null ? BadRequest("No user found by this Email.") : memberDto;
    }
}
