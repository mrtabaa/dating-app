namespace api.DTOs
{
    public static class Mappers
    {
        #region Generator Methods
        public static AppUser ConvertDummyRegisterDtoToAppUser(DummyRegisterDto userInput) //, int likedCount, int likedByCount
        {
            return new AppUser
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
        }

        public static AppUser ConvertUserRegisterDtoToAppUser(RegisterDto userInput) //, int likedCount, int likedByCount
        {
            return new AppUser
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
        }

        public static MemberDto ConvertAppUserToMemberDto(AppUser appUser, bool isFollowing = false)
        {
            return new MemberDto(
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
                Country: appUser.Country,
                State: appUser.State,
                City: appUser.City,
                Photos: appUser.Photos,
                IsFollowing: isFollowing
            );
        }

        public static LoggedInDto ConvertAppUserToLoggedInDto(AppUser appUser, string token, string? blobPhotoUrl)
        {
            return new LoggedInDto
            {
                Token = token,
                KnownAs = appUser.KnownAs,
                UserName = appUser.UserName,
                Gender = appUser.Gender,
                ProfilePhotoUrl = blobPhotoUrl,
                IsProfileCompleted = appUser.IsProfileCompleted
            };
        }

        public static Photo ConvertPhotoUrlsToPhoto(string[] photoUrls, bool isMain)
        {
            return new Photo
            {
                Schema = AppVariablesExtensions.AppVersions.Last<string>(),
                Url_165 = photoUrls[0],
                Url_256 = photoUrls[1],
                Url_enlarged = photoUrls[2],
                IsMain = isMain
            };
        }

        public static Follow? ConvertAppUsertoFollow(ObjectId? followerId, AppUser followedMember)
        {
            return new Follow(
                    Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                    Id: ObjectId.GenerateNewId(),
                    FollowerId: followerId,
                    FollowedMemberId: followedMember.Id
                );
        }

        #endregion Generator Methods

        #region Helper Functions
        // some Functions
        #endregion Helper Functions
    }
}