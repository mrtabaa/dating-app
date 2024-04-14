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
                KnownAs = userInput.KnownAs.Trim(),
                LastActive = DateTime.UtcNow,
                Gender = userInput.Gender.ToLower(),
                City = userInput.City.Trim(),
                Country = userInput.Country.Trim(),
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
                KnownAs = userInput.KnownAs.Trim(),
                LastActive = DateTime.UtcNow,
                Gender = userInput.Gender.ToLower(),
                City = userInput.City.Trim(),
                Country = userInput.Country.Trim(),
                Photos = [],
                FollowersCount = 0
            };
        }

        public static MemberDto? ConvertAppUserToMemberDto(AppUser appUser)
        {
            if (appUser.Schema is not null)
                return new MemberDto(
                    Schema: appUser.Schema,
                    UserName: appUser.UserName,
                    Age: DateTimeExtenstions.CalculateAge(appUser.DateOfBirth),
                    KnownAs: appUser.KnownAs,
                    Created: appUser.CreatedOn,
                    LastActive: appUser.LastActive,
                    Gender: appUser.Gender,
                    Introduction: appUser.Introduction,
                    LookingFor: appUser.LookingFor,
                    Interests: appUser.Interests,
                    City: appUser.City,
                    Country: appUser.Country,
                    Photos: appUser.Photos
                );

            return null;
        }

        public static LoggedInDto ConvertAppUserToLoggedInDto(AppUser appUser, string token)
        {
            return new LoggedInDto
            {
                Token = token,
                KnownAs = appUser.KnownAs,
                UserName = appUser.UserName,
                Gender = appUser.Gender,
                ProfilePhotoUrl = appUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_165
            };
        }

        public static Photo ConvertPhotoUrlsToPhoto(string[] photoUrls, bool isMain)
        {
            if (isMain)
                return new Photo(
                        Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                        Url_165: photoUrls[0],
                        Url_256: photoUrls[1],
                        Url_enlarged: photoUrls[2],
                        IsMain: isMain
                );

            return new Photo(
                    Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                    Url_165: photoUrls[0],
                    Url_256: photoUrls[1],
                    Url_enlarged: photoUrls[2],
                    IsMain: isMain
            );
        }

        public static Follow? ConvertAppUsertoFollow(ObjectId? followerId, ObjectId? followedId)
        {
            return new Follow(
                    Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                    Id: ObjectId.GenerateNewId(),
                    FollowerId: followerId,
                    FollowedMemberId: followedId
                );
        }

        #endregion Generator Methods

        #region Helper Functions
        // some Functions
        #endregion Helper Functions
    }
}