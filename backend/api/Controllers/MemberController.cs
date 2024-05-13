namespace api.Controllers;

[Authorize]
public class MemberController(
    IMemberRepository _memberRepository,
    IUserRepository _userRepository,
    IFollowRepository _followRepository,
    ITokenService _tokenService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto?>>> GetAll([FromQuery] MemberParams memberParams, CancellationToken cancellationToken)
    {
        if (memberParams.MinAge > memberParams.MaxAge)
            return BadRequest("Selected minAge cannot be greater than maxAge");

        ObjectId? userId = await _tokenService.GetActualUserId(User.GetUserIdHashed(), cancellationToken);
        if (userId is null)
            return BadRequest("User id is invalid. Login again.");

        string? gender = await _userRepository.GetGenderByHashedIdAsync(userId.Value, cancellationToken);

        if (gender is not null && string.IsNullOrEmpty(memberParams.Gender))
        {
            memberParams.UserId = userId;
            memberParams.Gender = gender == "male" ? "female" : "male"; // value is gender here
        }

        PagedList<AppUser>? pagedAppUsers = await _memberRepository.GetAllAsync(memberParams, cancellationToken);

        if (pagedAppUsers is null) return BadRequest("Returning members has failed. Try again or contact the customer support.");

        /*  1- Response only exists in Contoller. So we have to set PaginationHeader here before converting AppUser to UserDto.
                If we convert AppUser before here, we'll lose PagedList's pagination values, e.g. CurrentPage, PageSize, etc.
        */
        Response.AddPaginationHeader(new PaginationHeader(pagedAppUsers.CurrentPage, pagedAppUsers.PageSize, pagedAppUsers.TotalItemsCount, pagedAppUsers.TotalPages));

        /*  2- PagedList<T> has to be AppUser first to retrieve data from DB and set pagination values. 
                After that step we can convert AppUser to MemberDto in here (NOT in the UserRepository) */
        List<MemberDto?> memberDtos = [];

        foreach (AppUser pagedAppUser in pagedAppUsers)
        {
            if (await _followRepository.CheckIsFollowing(userId.Value, pagedAppUser, cancellationToken))
                memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser, following: true));
            else
                memberDtos.Add(Mappers.ConvertAppUserToMemberDto(pagedAppUser));
        }

        return memberDtos;
    }

    [HttpGet("id/{memberId}")]
    public async Task<ActionResult<MemberDto>> GetById(string memberId, CancellationToken cancellationToken)
    {
        bool isValid = ObjectId.TryParse(memberId, out ObjectId memberObjectId);

        if (!isValid)
            return BadRequest("Invalid memberId. Contact the admin.");

        MemberDto? memberDto = await _memberRepository.GetByIdAsync(memberObjectId, cancellationToken);

        return memberDto is null ? BadRequest("No member found by this ID.") : memberDto;
    }

    [HttpGet("username/{userName}")]
    public async Task<ActionResult<MemberDto>> GetByUserName(string userName, CancellationToken cancellationToken)
    {
        MemberDto? memberDto = await _memberRepository.GetByUserNameAsync(userName, cancellationToken);

        return memberDto is null ? BadRequest("No user found by this username.") : memberDto;
    }
}
