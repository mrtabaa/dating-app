namespace api.DTOs
{
    public static class Mappers
    {
        #region Generator Methods
        public static AppUser ConvertDummyRegisterDtoToAppUser(DummyRegisterDto userInput) => //, int likedCount, int likedByCount
            new()
            {
                Schema = AppVariablesExtensions.AppVersions.Last<string>(),
                Email = userInput.Email, // required by AspNet Identity
                UserName = userInput.UserName, // required by AspNet Identity
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

        public static AppUser ConvertUserRegisterDtoToAppUser(RegisterDto userInput) => //, int likedCount, int likedByCount
            new()
            {
                Schema = AppVariablesExtensions.AppVersions.Last<string>(),
                Email = userInput.Email, // required by AspNet Identity
                UserName = userInput.UserName, // required by AspNet Identity
                DateOfBirth = userInput.DateOfBirth,
                LastActive = DateTime.UtcNow,
                Gender = userInput.Gender.ToLower(),
                Photos = [],
                FollowersCount = 0
            };

        public static MemberDto ConvertAppUserToMemberDto(AppUser appUser, bool isFollowing = false) =>
            new(
                Schema: appUser.Schema,
                UserName: appUser.UserName,
                Age: DateTimeExtenstions.CalculateAge(appUser.DateOfBirth),
                DateOfBirth: appUser.DateOfBirth,
                KnownAs: appUser.KnownAs,
                Created: appUser.CreatedOn,
                LastActive: appUser.LastActive,
                Gender: appUser.Gender,
                Introduction: appUser.Introduction,
                LookingFor: appUser.LookingFor,
                Interests: appUser.Interests,
                CountryAcr: appUser.CountryAcr,
                Country: appUser.Country,
                State: appUser.State,
                City: appUser.City,
                Photos: appUser.Photos,
                IsFollowing: isFollowing
            );

        public static LoggedInDto ConvertAppUserToLoggedInDto(AppUser appUser, string token, string? blobPhotoUrl, string? turnstileToken = null) =>
            new()
            {
                Token = token,
                KnownAs = appUser.KnownAs,
                UserName = appUser.UserName,
                Gender = appUser.Gender,
                ProfilePhotoUrl = blobPhotoUrl,
                IsProfileCompleted = appUser.IsProfileCompleted,
                RecaptchaToken = turnstileToken
            };

        public static Photo ConvertPhotoUrlsToPhoto(string[] photoUrls, bool isMain) =>
            new()
            {
                Schema = AppVariablesExtensions.AppVersions.Last<string>(),
                Url_165 = photoUrls[0],
                Url_256 = photoUrls[1],
                Url_enlarged = photoUrls[2],
                IsMain = isMain
            };

        public static Follow ConvertAppUsertoFollow(ObjectId followerId, ObjectId followedMemberId) =>
            new(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                FollowerId: followerId,
                FollowedMemberId: followedMemberId
            );

        public static Message ConvertMessageInDtoToMessage(string content, ObjectId userId, ObjectId receiverId) =>
            new(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                SenderId: userId,
                RecieverId: receiverId,
                Content: content.Trim(),
                SentOn: DateTime.UtcNow,
                ReadOn: null,
                SenderDeleted: false,
                ReceiverDeleted: false
            );

        public static MessageDto ConvertMessageToMessageDto(Message message, AppUser userOrTarget, string? profilePhotoSasUrl) =>
            new(
                Id: message.Id.ToString(), // To delete/update
                UserOrTargetUserName: userOrTarget.UserName,
                UserOrTargetKnownAs: userOrTarget?.KnownAs,
                UserOrTargetProfilePhoto: profilePhotoSasUrl,
                Content: message.Content,
                ReadOn: message.ReadOn,
                SentOn: message.SentOn
            );

        public static OnlineUsersDto ConvertAppUserToOnlineStatusDto(AppUser appUser) =>
            new(
                UserName: appUser.NormalizedUserName
                    ?? throw new ArgumentNullException("UserName cannot be null but it is.", nameof(appUser.NormalizedUserName)),
                LastActive: appUser.LastActive
            );

        #endregion Generator Methods
    }
}