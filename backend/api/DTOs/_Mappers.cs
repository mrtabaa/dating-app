namespace api.DTOs;

public static class Mappers
{
    #region Generator Methods

    public static AppUser ConvertDummyRegisterDtoToAppUser(DummyRegisterDto userInput) => //, int likedCount, int likedByCount
        new()
        {
            Schema = AppVariablesExtensions.AppVersions.Last(),
            Email = userInput.Email.ToLower(), // required by AspNet Identity
            UserName = userInput.UserName.ToLower(), // required by AspNet Identity
            DateOfBirth = userInput.DateOfBirth,
            KnownAs = userInput.KnownAs,
            LastActive = DateTime.UtcNow,
            Gender = userInput.Gender.ToLower(),
            Country = userInput.Country,
            State = userInput.State,
            City = userInput.City,
            Introduction = userInput.Introduction,
            LookingFor = userInput.LookingFor,
            Interests = userInput.Interests,
            Photos = [],
            FollowersCount = 0
        };

    public static AppUser ConvertRegisterDtoToAppUser(RegisterDto userInput) => //, int likedCount, int likedByCount
        new()
        {
            Schema = AppVariablesExtensions.AppVersions.Last(),
            Email = userInput.Email.ToLower(), // required by AspNet Identity
            UserName = userInput.UserName.ToLower(), // required by AspNet Identity
            DateOfBirth = userInput.DateOfBirth,
            LastActive = DateTime.UtcNow,
            Gender = userInput.Gender.ToLower(),
            Photos = [],
            FollowersCount = 0
        };

    public static MemberDto ConvertAppUserToMemberDto(AppUser appUser, bool isFollowing = false) =>
        new(
            appUser.Schema,
            appUser.UserName,
            appUser.DateOfBirth.CalculateAge(),
            appUser.DateOfBirth,
            appUser.KnownAs,
            appUser.CreatedOn,
            appUser.LastActive,
            appUser.Gender,
            appUser.Introduction,
            appUser.LookingFor,
            appUser.Interests,
            appUser.CountryAcr,
            appUser.Country,
            appUser.State,
            appUser.City,
            appUser.Photos,
            isFollowing
        );

    public static LoggedInDto ConvertAppUserToLoggedInDto(AppUser appUser, string token, string? blobPhotoUrl,
        string? turnstileToken = null) =>
        new()
        {
            Token = token,
            KnownAs = appUser.KnownAs,
            UserName = appUser.UserName,
            Gender = appUser.Gender,
            ProfilePhotoUrl = blobPhotoUrl,
            IsProfileCompleted = appUser.IsProfileCompleted
        };

    public static Photo ConvertPhotoUrlsToPhoto(string[] photoUrls, bool isMain) =>
        new()
        {
            Schema = AppVariablesExtensions.AppVersions.Last(),
            Url165 = photoUrls[0],
            Url256 = photoUrls[1],
            UrlEnlarged = photoUrls[2],
            IsMain = isMain
        };

    public static Follow ConvertAppUserToFollow(ObjectId followerId, ObjectId followedMemberId) =>
        new(
            AppVariablesExtensions.AppVersions.Last(),
            FollowerId: followerId,
            FollowedMemberId: followedMemberId
        );

    public static Message ConvertMessageInDtoToMessage(string content, ObjectId userId, ObjectId receiverId) =>
        new(
            AppVariablesExtensions.AppVersions.Last(),
            SenderId: userId,
            ReceiverId: receiverId,
            Content: content.Trim(),
            SentOn: DateTime.UtcNow,
            ReadOn: null,
            SenderDeleted: false,
            ReceiverDeleted: false
        );

    public static MessageDto ConvertMessageToMessageDto(Message message, AppUser userOrTarget,
        string? profilePhotoSasUrl) =>
        new()
        {
            Id = message.Id.ToString(), // To delete/update
            UserOrTargetUserName = userOrTarget.UserName,
            UserOrTargetKnownAs = userOrTarget.KnownAs,
            UserOrTargetProfilePhoto = profilePhotoSasUrl,
            Content = message.Content,
            ReadOn = message.ReadOn,
            SentOn = message.SentOn
        };

    public static OnlineUsersDto ConvertAppUserToOnlineStatusDto(AppUser appUser) =>
        new(
            appUser.NormalizedUserName
            ?? throw new ArgumentNullException(nameof(appUser.NormalizedUserName), "NormalizedUserName cannot be null but it is."),
            appUser.LastActive
        );

    #endregion Generator Methods
}