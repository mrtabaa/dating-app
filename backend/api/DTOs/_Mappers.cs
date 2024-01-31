namespace api.DTOs
{
    public static class Mappers
    {
        #region Generator Methods
        public static AppUser ConvertUserRegisterDtoToAppUser(UserRegisterDto userInput) //, int likedCount, int likedByCount
        {
            // manually dispose HMACSHA512 after being done
            using var hmac = new HMACSHA512();

            return new AppUser(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Id: null,
                Email: userInput.Email.ToLower().Trim(),
                PasswordHash: hmac.ComputeHash(Encoding.UTF8.GetBytes(userInput.Password)),
                PasswordSalt: hmac.Key,
                DateOfBirth: userInput.DateOfBirth,
                KnownAs: userInput.KnownAs.Trim(),
                Created: DateTime.UtcNow,
                LastActive: DateTime.UtcNow,
                Gender: userInput.Gender.ToLower(),
                Introduction: userInput.Introduction?.Trim(),
                LookingFor: userInput.LookingFor?.Trim(),
                Interests: userInput.Interests?.Trim(),
                City: userInput.City.Trim(),
                Country: userInput.Country.Trim(),
                Photos: []
                // LikedCount: likedCount, // TODO add these
                // LikedByCount: likedByCount
            );
        }

        public static MemberDto? ConvertAppUserToMemberDto(AppUser appUser)
        {
            if (!(appUser.Id is null || appUser.Schema is null))
                return new MemberDto(
                    Schema: appUser.Schema, // TODO use this instead of calling the version list on other methods
                    Id: appUser.Id,
                    Email: appUser.Email,
                    Age: DateTimeExtenstions.CalculateAge(appUser.DateOfBirth),
                    KnownAs: appUser.KnownAs,
                    Created: appUser.Created,
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

        public static LoggedInDto? ConvertAppUserToLoggedInDto(AppUser appUser, string token)
        {
            if (!(appUser.Id is null || appUser.Schema is null)) // TODO remove schema from here
            {
                return new LoggedInDto(
                    Id: appUser.Id,
                    Token: token,
                    KnownAs: appUser.KnownAs,
                    Email: appUser.Email,
                    Gender: appUser.Gender,
                    ProfilePhotoUrl: appUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_165
                );
            }

            return null;
        }

        public static Photo ConvertPhotoUrlsToPhoto(string[] photoUrls, bool isMain)
        {
            if (isMain)
                return new Photo(
                        Schema: AppVariablesExtensions.AppVersions.Last<string>(), // TODO remove these, user appUser.Schema
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

        public static Like? ConvertAppUsertoLike(AppUser targetAppUser, string? loggedInUserId)
        {
            if (loggedInUserId is null || targetAppUser.Id is null)
                return null;

            return new Like(
                Schema: AppVariablesExtensions.AppVersions.Last<string>(),
                Id: null,
                LoggedInUserId: loggedInUserId,
                TargetMemberId: targetAppUser.Id,
                Email: targetAppUser.Email,
                Age: targetAppUser.DateOfBirth.CalculateAge(),
                KnownAs: targetAppUser.KnownAs,
                Gender: targetAppUser.Gender,
                City: targetAppUser.City,
                PhotoUrl: targetAppUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_256
            );
        }
        #endregion Generator Methods

        #region Helper Functions
        // some Functions
        #endregion Helper Functions
    }
}