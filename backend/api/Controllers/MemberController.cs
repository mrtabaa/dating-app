namespace api.Controllers;

[Authorize]
public class MemberController(IMemberRepository _memberRepository, IUserRepository _userRepository) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto?>>> GetMembers([FromQuery] MemberParams memberParams, CancellationToken cancellationToken)
    {
        if (memberParams.MinAge > memberParams.MaxAge)
            return BadRequest("Selected minAge cannot be greater than maxAge");

        IdAndStringValue? idAndGender = await _userRepository.GetGenderByHashedIdAsync(User.GetUserIdHashed(), cancellationToken);

        if (idAndGender is not null && string.IsNullOrEmpty(memberParams.Gender))
        {
            memberParams.UserId = idAndGender.Id;
            memberParams.Gender = idAndGender.Value == "male" ? "female" : "male"; // value is gender here
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

    [HttpGet("username/{userName}")]
    public async Task<ActionResult<MemberDto>> GetMemberByUserName(string userName, CancellationToken cancellationToken)
    {
        MemberDto? memberDto = await _memberRepository.GetMemberByUserNameAsync(userName, cancellationToken);

        return memberDto is null ? BadRequest("No user found by this username.") : memberDto;
    }
}
